namespace Carbon.Plugins;

using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using Carbon;
using Carbon.Test;

[Info("IntegrationTests", "Carbon Community", "1.0.0")]
public class IntegrationTests : CarbonPlugin
{
	private void OnServerInitialized()
	{
		foreach(var type in HookableType.GetNestedTypes(BindingFlags.Public))
		{
			Integrations.EnqueueBed(Integrations.Get(type.Name, type, Activator.CreateInstance(type)));
		}
		
		Integrations.EnqueueBed(Integrations.Get(nameof(Cleanup), typeof(Cleanup), new Cleanup()));

		Logger.Log(string.Empty);
		
		Integrations.Run(delay: 0.1f);
	}
	
	private class Cleanup
	{
		[Integrations.Test]
		public void quit(Integrations.Test test)
		{
			test.Log("Quitting");
			Logger.Log(string.Empty);

			SingletonComponent<ServerMgr>.Instance?.Shutdown();
			Rust.Application.isQuitting = true;
			Network.Net.sv?.Stop(nameof (quit));
			Process.GetCurrentProcess().Kill();
			Rust.Application.Quit();
		}
	}
	
	public class Grid
	{
		[Integrations.Test.Assert]
		public void find(Integrations.Test.Assert test)
		{
			test.Log("Hello world!");
		}
	}
}