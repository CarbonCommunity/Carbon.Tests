#if CARBON

using System;
using System.Linq;
using System.Reflection;
using API.Assembly;
using Carbon;
using Carbon.Core;
using Carbon.Integrations;

namespace Extension;

public class ExtensionEntrypoint : ICarbonExtension
{
	public void OnLoaded ( EventArgs args )
	{
		IntegrationManager.Singleton.Boot();

		Community.Runtime.Events.Subscribe ( API.Events.CarbonEvent.OnServerInitialized, async arg =>
		{
			try
			{
				IntegrationManager.Singleton.Initialize();

				const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

				var newPackage = new ModLoader.ModPackage
				{
					Name = "Carbon.Integrations",
					IsCoreMod = true
				};

				ModLoader.LoadedPackages.Add(newPackage);
				ModLoader.InitializePlugin(typeof(MainIntegration), out _, newPackage);

				IntegrationManager.Singleton.ResetTests();

				foreach (var package in ModLoader.LoadedPackages)
				{
					foreach (var plugin in package.Plugins)
					{
						var methods = plugin.Type.GetMethods(flags);

						foreach (var method in methods)
						{
							var test = method.GetCustomAttribute<Test>();

							if (test == null)
							{
								continue;
							}

							IntegrationManager.Singleton.RegisterTest(plugin, test, method);
						}
					}
				}

				var result = await IntegrationManager.Singleton.ExecuteTests();
				var total = result.Passed.Count + result.Failed.Count;

				if (result.Passed.Count >= result.Failed.Count)
				{
					IntegrationManager.Warn($" There have been equally the same passed as failed amounts of tests: {result.Passed.Count:n0} / {total:n0} passed [out of {total} total]");
				}
				else if (result.Passed.Count > result.Failed.Count)
				{
					IntegrationManager.Warn($" The run has been a success: {result.Passed.Count:n0} / {total:n0} passed [out of {total} total]");
				}
				else
				{
					IntegrationManager.Warn($" The run has failed: {result.Failed.Count:n0} / {total:n0} failed [out of {total} total]");
				}

				ConsoleSystem.Run(ConsoleSystem.Option.Server, "quit");
			}
			catch (Exception ex)
			{
				Logger.Error("Failed doing something wild.", ex);
			}
		} );
	}

	public void Awake ( EventArgs args )
	{

	}

	public void OnUnloaded ( EventArgs args )
	{

	}
}

#endif
