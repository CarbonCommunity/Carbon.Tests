using Carbon.Core;
using Carbon.Test;
using Carbon.Profiler;
using Carbon.Components;
using Carbon.Extensions;

using Oxide.Core.Libraries;

using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;

namespace Carbon.Plugins;

[Info("IntegrationTests", "Carbon Community", "1.0.0")]
public class IntegrationTests : CarbonPlugin
{
	public static IntegrationTests singleton;

	private void Init()
	{
		singleton = this;

		ToggleAllHookDebugging(true);
	}
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
	private static void ToggleAllHookDebugging(bool wants)
	{
		foreach (var plugin in ModLoader.Packages.SelectMany(package => package.Plugins))
		{
			plugin.HookPool.EnableDebugging(wants);
		}
		foreach (var module in Community.Runtime.ModuleProcessor.Modules)
		{
			module.HookPool.EnableDebugging(wants);
		}
	}

	private class Cleanup
	{
		[Integrations.Test]
		public void quit(Integrations.Test test)
		{
			test.Log("Quitting");
			Logger.Log(string.Empty);
			ToggleAllHookDebugging(false);

			SingletonComponent<ServerMgr>.Instance?.Shutdown();
			Rust.Application.isQuitting = true;
			Network.Net.sv?.Stop(nameof (quit));
			Process.GetCurrentProcess().Kill();
			Rust.Application.Quit();
		}
	}

	public class Permission
	{
		public const string userId = "76561198158946080";
		public const string groupId = "integrationtestgroup";

		public UserData user;
		public GroupData group;

		[Integrations.Test.Assert]
		public void valid_id(Integrations.Test.Assert test)
		{
			test.IsTrue(singleton.permission.UserIdValid(userId), $"singleton.permission.UserIdValid(\"{userId}\")");
		}

		[Integrations.Test.Assert(CancelOnFail = false)]
		public void create_user(Integrations.Test.Assert test)
		{
			test.IsFalse(singleton.permission.UserExists(userId), $"singleton.permission.UserExists(\"{userId}\")");
			test.IsNull(user, "user");

			user = singleton.permission.GetUserData(userId, addIfNotExisting: true);
			test.IsNotNull(user, "user");
		}

		[Integrations.Test.Assert]
		public void create_group(Integrations.Test.Assert test)
		{
			test.IsFalse(singleton.permission.GroupExists(groupId), $"singleton.permission.GroupExists(\"{groupId}\")");
			test.IsNull(group, "group");

			test.IsTrue(singleton.permission.CreateGroup(groupId, groupId, 0), $"singleton.permission.CreateGroup(\"{groupId}\", \"{groupId}\", 0)");

			group = singleton.permission.GetGroupData(groupId);
			test.IsNotNull(group, "group");
		}

		[Integrations.Test.Assert]
		public void add_user_to_group(Integrations.Test.Assert test)
		{
			test.IsFalse(singleton.permission.UserHasGroup(userId, groupId), $"singleton.permission.UserHasGroup(\"{userId}\", \"{groupId}\")");
			singleton.permission.AddUserGroup(userId, groupId);
			test.Log($"singleton.permission.AddUserGroup(\"{userId}\", \"{groupId}\")");
		}

		[Integrations.Test.Assert]
		public void remove_user_from_group(Integrations.Test.Assert test)
		{
			test.IsTrue(singleton.permission.UserHasGroup(userId, groupId), $"singleton.permission.UserHasGroup(\"{userId}\", \"{groupId}\")");
			singleton.permission.RemoveUserGroup(userId, groupId);
			test.Log($"singleton.permission.RemoveUserGroup(\"{userId}\", \"{groupId}\")");
			test.IsFalse(singleton.permission.UserHasGroup(userId, groupId), $"singleton.permission.UserHasGroup(\"{userId}\", \"{groupId}\")");
		}

		[Integrations.Test.Assert]
		public void delete_user(Integrations.Test.Assert test)
		{
			test.IsTrue(singleton.permission.UserExists(userId), $"singleton.permission.UserExists(\"{userId}\")");
			test.IsTrue(singleton.permission.userdata.Remove(userId), $"singleton.permission.userdata.Remove(\"{userId}\")");
		}

		[Integrations.Test.Assert]
		public void delete_group(Integrations.Test.Assert test)
		{
			test.IsTrue(singleton.permission.GroupExists(groupId), $"singleton.permission.GroupExists(\"{groupId}\")");
			test.IsTrue(singleton.permission.groupdata.Remove(groupId), $"singleton.permission.groupdata.Remove(\"{groupId}\")");
		}
	}

	public class Profiling
	{
		public const MonoProfiler.ProfilerArgs Args = MonoProfiler.AllFlags;
		public MonoProfiler.Sample Sample1;
		public MonoProfiler.Sample Sample2;
		public byte[] Data;

		[Integrations.Test.Assert(CancelOnFail = true)]
		public void validate(Integrations.Test.Assert test)
		{
			test.IsTrue(MonoProfiler.Enabled, "MonoProfiler.Enabled");
			test.IsFalse(MonoProfiler.Crashed, "MonoProfiler.Crashed");
		}

		[Integrations.Test.Assert(DurationTimeout = 10000)]
		public async void profile_start_sample1(Integrations.Test.Assert test)
		{
			test.IsTrue(MonoProfiler.ToggleProfiling(Args, false).GetValueOrDefault(), "MonoProfiler.ToggleProfiling(Args, false)");
			test.Log("SAMPLE-1: Recording started");

			await AsyncEx.WaitForSeconds(4);
			test.Complete();
		}

		[Integrations.Test.Assert]
		public void profile_stop_sample1(Integrations.Test.Assert test)
		{
			test.IsFalse(MonoProfiler.ToggleProfiling(Args, false).GetValueOrDefault(), "MonoProfiler.ToggleProfiling(Args, false)");
			test.Log("SAMPLE-1: Recording stopped");
		}

		[Integrations.Test.Assert]
		public void profile_resampling_sample1(Integrations.Test.Assert test)
		{
			Sample1.Resample();
			test.Log("SAMPLE-1: Sample.Resample()");

			test.Log($"SAMPLE-1: Assemblies = {Sample1.Assemblies.Count} | Calls = {Sample1.Calls.Count} | Memory = {Sample1.Memory.Count}");
		}

		[Integrations.Test.Assert(DurationTimeout = 10000)]
		public async void profile_start_sample2(Integrations.Test.Assert test)
		{
			test.IsTrue(MonoProfiler.ToggleProfiling(Args, false).GetValueOrDefault(), "MonoProfiler.ToggleProfiling(Args, false)");
			test.Log("SAMPLE-2: Recording started");

			await AsyncEx.WaitForSeconds(4);
			test.Complete();
		}

		[Integrations.Test.Assert]
		public void profile_stop_sample2(Integrations.Test.Assert test)
		{
			test.IsFalse(MonoProfiler.ToggleProfiling(Args, false).GetValueOrDefault(), "MonoProfiler.ToggleProfiling(Args, false)");
			test.Log("SAMPLE-2: Recording stopped");
		}

		[Integrations.Test.Assert]
		public void profile_resampling_sample2(Integrations.Test.Assert test)
		{
			Sample2.Resample();
			test.Log("SAMPLE-2: Sample.Resample()");

			test.Log($"SAMPLE-2: Assemblies = {Sample2.Assemblies.Count} | Calls = {Sample2.Calls.Count} | Memory = {Sample2.Memory.Count}");
		}

		[Integrations.Test.Assert]
		public void profile_compare(Integrations.Test.Assert test)
		{
			var compare = Sample1.Compare(Sample2);

			test.Log($"COMPARE: Assemblies = {compare.Assemblies.Count} | Calls = {compare.Calls.Count} | Memory = {compare.Memory.Count}");
		}

		[Integrations.Test.Assert]
		public void profile_save(Integrations.Test.Assert test)
		{
			Data = MonoProfiler.SerializeSample(Sample1);
			test.Log($"Sample saved file size: {ByteEx.Format(Data.Length).ToUpper()}");
		}

		[Integrations.Test.Assert]
		public void profile_load(Integrations.Test.Assert test)
		{
			var sample = MonoProfiler.DeserializeSample(Data);
			test.Log($"Loaded sample: Assemblies = {sample.Assemblies.Count} | Calls = {sample.Calls.Count} | Memory = {sample.Memory.Count}");
		}
	}
}