using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carbon.Components;
using Carbon.Plugins;
using Facepunch;
using WebSocketSharp;

namespace Carbon.Integration;

public class BaseIntegration : CarbonPlugin
{
	public virtual TestSettings Settings { get; } = new();

	internal List<Test> _tests = new();

	public virtual void Install()
	{

	}
	public void Register(Test test)
	{
		_tests.Add(test);
	}

	public override void IInit()
	{
		base.IInit();

		Register(Test.Make("info", "Prints information about Rust and Carbon's live version the test's running on.",
			callback: (test, settings) =>
			{
				test.Message($" Rust");
				test.Message($"		Scm");
				test.Message($"			Type:		{BuildInfo.Current.Scm.Type}");
				test.Message($"			ChangeId:	{BuildInfo.Current.Scm.ChangeId}");
				test.Message($"			Branch:		{BuildInfo.Current.Scm.Branch}");
				test.Message($"			Repo:		{BuildInfo.Current.Scm.Repo}");
				test.Message($"			Comment:	{BuildInfo.Current.Scm.Comment}");
				test.Message($"			Author:		{BuildInfo.Current.Scm.Author}");
				test.Message($"			Date:		{BuildInfo.Current.Scm.Date}");
				test.Message($"		Build");
				test.Message($"			Id:			{BuildInfo.Current.Build.Id}");
				test.Message($"			Number:		{BuildInfo.Current.Build.Number}");
				test.Message($"			Tag:		{BuildInfo.Current.Build.Tag}");
				test.Message($"			Url:		{BuildInfo.Current.Build.Url}");
				test.Message($"			Name:		{BuildInfo.Current.Build.Name}");
				test.Message($"			Node:		{BuildInfo.Current.Build.Node}");
				test.Message($" Carbon");
				test.Message($"		System");
				test.Message($"			SystemID:	{Community.Runtime.Analytics.SystemID}");
				test.Message($"			SessionID:	{Community.Runtime.Analytics.SessionID}");
				test.Message($"			UserAgent:	{Community.Runtime.Analytics.UserAgent}");
				test.Message($"		Build");
				test.Message($"			Branch:		{Community.Runtime.Analytics.Branch}");
				test.Message($"			Version:	{Community.Runtime.Analytics.Version}");
				test.Message($"			Version2:	{Community.Runtime.Analytics.InformationalVersion}");
				test.Message($"			Protocol:	{Community.Runtime.Analytics.Protocol}");
				test.Message($"			Platform:	{Community.Runtime.Analytics.Platform}");

				return null;
			}));

		Install();
		_run();
	}

	internal async void _run()
	{
		foreach(var test in _tests)
		{
			await test.Execute(Settings);
			await Task.Delay(500);
		}

		await Shutdown();
	}

	public async Task Shutdown()
	{
		var passedTests = _tests.Count(x => string.IsNullOrEmpty(x.ResultMessage));
		var totalTests = _tests.Count;

		Puts($"Tests finalized: {passedTests} / {totalTests} passed");
		ServerMgr.Instance.Shutdown();
		await Task.CompletedTask;
	}

	public class TestSettings
	{
		public int TestTimeout;
	}

	public struct Test
	{
		internal string Name;
		internal string Description;
		internal string SuccessMessage;
		internal string FailureMessage;
		internal Func<Test, TestSettings, string> Callback;

		public string ResultMessage { get; internal set; }

		public async Task Execute(TestSettings arg)
		{
			Logger.Log($"Initializing test '{Name}'..");
			Logger.Log($" {Description}");
			Logger.Log(new string('.', 10));

			using (TimeMeasure.New($"Test '{Name}'", arg.TestTimeout))
			{
				try
				{
					ResultMessage = Callback(this, arg);

					Logger.Log(new string('.', 10));

					if (string.IsNullOrEmpty(ResultMessage))
					{
						Logger.Log($" Test '{Name}' succeeded!");
					}
					else
					{
						Logger.Warn($" Test '{Name}' failed: {ResultMessage}");
					}
				}
				catch (Exception exception)
				{
					exception = exception.InnerException ?? exception;
					Logger.Error($"Failed integration test '{Name}' ({exception.Message})\n{exception.StackTrace}");
					Logger.Log(new string('.', 10));
				}
			}

			await Task.CompletedTask;
		}
		public async void Message(object message)
		{
			if(message == null)
			{
				Logger.Log(null);
				return;
			}

			Logger.Log($" [TST-{Name}]: {message}");
		}
		public async void Notice(object message)
		{
			Logger.Log($" [TST-{Name}] [NOTICE]: {message?.ToString().ToUpper()}");
		}
		public void Break()
		{
			Message(null);
		}

		public static Test Make(string name, string description, string success = null, string failure = null, Func<Test, TestSettings, string> callback = null)
		{
			return new Test
			{
				Name = name,
				Description	= description,
				SuccessMessage = success,
				FailureMessage = failure,
				Callback = callback
			};
		}
	}
}
