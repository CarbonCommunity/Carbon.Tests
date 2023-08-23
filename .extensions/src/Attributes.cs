using System;

namespace Carbon.Integrations;

[AttributeUsage(AttributeTargets.Method)]
public class Test : Attribute
{
    public bool StopOnError = false;
}
