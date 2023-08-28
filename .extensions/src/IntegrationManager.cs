using Carbon.Extensions;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Carbon.Integrations;

public class IntegrationManager
{
	public readonly static IntegrationManager Singleton = new();

	public readonly Dictionary<Plugin, List<Test>> Plugins = new();
	public readonly object[] Arg = new object[1];

	public const float Timeout = 0.25f;
	public const float Frequency = 0.1f;

	internal static readonly IntegrationLogger Logging = new();
	internal static readonly Stopwatch Stopwatch = new();

	public static void Log(object message)
	{
		Logger.Log(message);
		Logging.QueueLog($"[INFO] {message}");
	}
	public static void Warn(object message)
	{
		Logger.Warn(message);
		Logging.QueueLog($"[WARN] {message}");
	}
	public static void Error(object message, Exception ex = null)
	{
		Logger.Error(message, ex);
		Logging.QueueLog($"[ERRO] {message}");
	}

	public async Task<object> Create<T>(Plugin plugin, T test, MethodInfo origin) where T : Test
	{
		try
		{
			Arg[0] = test;
			var result = origin?.Invoke(plugin, Arg);
			await Task.CompletedTask;
			return result;
		}
		catch (Exception ex)
		{
			Error($"Test on plugin '{plugin.Name} by {plugin.Author}' failed", ex);
			test.HasErrored = true;
			return null;
		}
	}

	public void Boot()
	{
		Log($"Booting Carbon.Integrations");
	}
	public void Initialize()
	{
		Log($"Intializing Carbon.Integrations");
	}
	public void ResetTests()
	{
		foreach (var plugin in Plugins)
		{
			plugin.Value.Clear();
		}

		Plugins.Clear();
	}
	public void RegisterTest<T>(Plugin plugin, T test, MethodInfo origin) where T : Test
	{
		if (!Plugins.TryGetValue(plugin, out var tests))
		{
			Plugins.Add(plugin, tests = new());
		}

		Log($"Installed '{origin.Name}' ({test.GetType().Name}) for plugin '{plugin.Name} by {plugin.Author}'");

		tests.Add(test);

		test.Origin = origin;
		test.Task = Create(plugin, test, origin);
	}

	public async Task<IntegrationResult> ExecuteTests()
	{
		var hasEnded = false;
		var integrationResult = new IntegrationResult
		{
			Failed = new List<Test>(),
			Passed = new List<Test>()
		};

		foreach (var plugin in Plugins)
		{
			if (hasEnded)
			{
				break;
			}

			foreach (var test in plugin.Value)
			{
				await AsyncEx.WaitForSeconds(Timeout);

				if (hasEnded)
				{
					break;
				}

				Warn($" task '{test.Origin.Name}' [{test.GetType().Name}] - {plugin.Key.Name} v{plugin.Key.Version} by {plugin.Key.Author}");

				Stopwatch.Reset();
				Stopwatch.Start();

				var result = await test.Task;

				if (Stopwatch.ElapsedMilliseconds > 0)
				{
					Warn($" callback took {Stopwatch.ElapsedMilliseconds}ms.");
				}

				Stopwatch.Stop();

				//
				// Handle errors
				//
				if (test.CancelOnError)
				{
					if (test.HasErrored)
					{
						hasEnded = true;
						Error("   failure.");
						integrationResult.Failed.Add(test);
						continue;
					}
				}

				var booleanResult = result is bool value && value;

				//
				// Handle test types
				//
				switch (test)
				{
					case Test.Assert assert:
						if (!booleanResult)
						{
							test.Fail($" assert result '{booleanResult}' when expected true.");

							if (assert.CancelOnInvalid)
							{
								hasEnded = true;
								Log("   failed.");
								integrationResult.Failed.Add(test);
							}
						}
						else
						{
							test.Succeed();
							Log("   success.");
							integrationResult.Passed.Add(test);
						}
						break;

					case Test.WaitUntil waitUntil:
						TimeSince start = 0;
						while (!waitUntil.Finalized && !waitUntil.TimedOut)
						{
							if (start >= waitUntil.WaitTimeout)
							{
								Error($" timed out after ({(start - 0) * 1000:0.00}ms) - thrsh. {waitUntil.WaitTimeout * 1000:0}ms.");

								waitUntil.Timeout();
							}

							await AsyncEx.WaitForSeconds(Frequency);
						}
						break;
				}

				Warn("   task done.");
			}
		}

		return integrationResult;
	}
}
