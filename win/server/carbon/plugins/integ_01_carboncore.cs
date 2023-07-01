using Carbon.Integration;

namespace Carbon.Plugins;

[Info("IntegrationTest_01_CarbonCore", "Carbon Community", "1.0.0")]
public class IntegrationTest_01_CarbonCore : BaseIntegration
{
	public override TestSettings Settings => new()
	{
		TestTimeout = 10
	};

	public override void Install()
	{
		Register(Test.Make("hook_calls", "Testing various types of hooks.",
			callback: (test, settings) =>
			{
				CallHook("HookTest1");

				test.Message("Test if unmatching parameters are properly validated");
				CallHook("HookTest2");
				CallHook("HookTest2", null);
				CallHook("HookTest2", "string object");

				test.Message("Test if hooks return expected objects");
				test.Message($"HookTest3 result: '{CallHook("HookTest3")}'");

				test.Message("Test if same-named hooks get proper priority relative to parameter types");
				test.Message($"HookTest3 result: '{CallHook("HookTest3", "yolo")}'");

				return null;
			}));
	}

	private void HookTest1()
	{
		Logger.Log($" HookTest1");
	}
	private void HookTest2(object value)
	{
		Logger.Log($" HookTest2: '{value}'");
	}

	private object HookTest3()
	{
		Logger.Log($" HookTest3");
		return "HookTest3 object result!";
	}
	private object HookTest3(string value)
	{
		Logger.Log($" HookTest3");
		return $"HookTest3 with value parameter '{value}' result!";
	}
}
