using Carbon.Extensions;
using Carbon.Plugins;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using static ProtoBuf.ItemCrafter;

namespace Carbon.Integrations;

public class IntegrationManager
{
    public readonly static IntegrationManager Singleton = new ();

    public readonly Dictionary<Plugin, List<Test>> Plugins = new ();
    public readonly object [] Arg = new object [ 1 ];   

    public const float Timeout = 0.5f;

    public static void Log ( object message )
    {
        Logger.Log ( message );
    }
    public static void Warn ( object message )
    {
        Logger.Warn ( message );
    }
    public static void Error ( object message, Exception ex = null)
    {
        Logger.Error ( message, ex );
    }

    public void RegisterTest<T> ( Plugin plugin, T test, MethodInfo source ) where T : Test
    {
        if (!Plugins.TryGetValue ( plugin, out var tests ))
        {
            Plugins.Add ( plugin, tests = new () );
        }

        Log ( $"Found '{plugin.Name} by {plugin.Author}' test on method '{source.Name}': {typeof(T).Name}" );

        tests.Add ( test );

        test.Task = new System.Threading.Tasks.Task<object> ( () =>
        {
            try
            {
                Arg [ 0 ] = test;
                return source?.Invoke ( plugin, Arg );
            }
            catch (Exception ex)
            {
                Error ( $"Test on plugin '{plugin.Name} by {plugin.Author}' failed", ex );
                test.HasErrored = true;
                return null;
            }
        } );
    }

    public async Task<bool> ExecuteTests ()
    {
        var hasEnded = false;

        foreach (var plugin in Plugins)
        {
            if (hasEnded)
            {
                break;
            }

            foreach (var test in plugin.Value)
            {
                await AsyncEx.WaitForSeconds ( Timeout );

                if (hasEnded)
                {
                    break;
                }

                var result = await test.Task;

                //
                // Handle errors
                //
                if (test.CancelOnError)
                {
                    if (test.HasErrored)
                    {
                        hasEnded = true;
                        break;
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
                            test.Fail ( $"Assert result '{booleanResult}' when expected true" );

                            if (assert.CancelOnInvalid)
                            {
                                hasEnded = true;
                                break;
                            }
                        }
                        else
                        {
                            test.Succeed ();
                        }
                        break;
                    case Test.WaitUntil waitUntil:
                        var start = new TimeSince ();
                        while (!waitUntil.Finalized && !waitUntil.TimedOut)
                        {
                            if (start >= waitUntil.WaitTimeout)
                            {
                                waitUntil.Timeout ();
                            }
                        }
                        break;
                }
            }
        }

        return !hasEnded;
    }
}