using System.Collections.Generic;

namespace Carbon.Integrations;

public struct IntegrationResult
{
	public IList<Test> Passed;
	public IList<Test> Failed;
}
