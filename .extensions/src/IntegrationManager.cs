using Carbon.Extensions;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Carbon.Integrations;

public class IntegrationManager
{
    public readonly static IntegrationManager Singleton = new ();

    public readonly Dictionary<Plugin, List<Test>> Plugins = new ();
    public readonly object [] Arg = new object [ 1 ];

    public const float Timeout = 0.25f;
    public const float Frequency = 0.1f;

    public static void Log ( object message )
    {
        Logger.Log ( message );
        Console.WriteLine ( $"[INFO] {message}" );
    }
    public static void Warn ( object message )
    {
        Logger.Warn ( message );
        Console.WriteLine ( $"[WARN] {message}" );
    }
    public static void Error ( object message, Exception ex = null)
    {
        Logger.Error ( message, ex );
        Console.WriteLine ( $"[ERRO] {message}" );
    }

    public void RegisterTest<T> ( Plugin plugin, T test, MethodInfo origin ) where T : Test
    {
        if (!Plugins.TryGetValue ( plugin, out var tests ))
        {
            Plugins.Add ( plugin, tests = new () );
        }

        Log ( $"Installed '{origin.Name}' ({test.GetType ().Name}) for plugin '{plugin.Name} by {plugin.Author}'" );

        tests.Add ( test );

        test.Origin = origin;
        test.Task = new System.Threading.Tasks.Task<object> ( () =>
        {
            try
            {
                Arg [ 0 ] = test;
                return origin?.Invoke ( plugin, Arg );
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

                Warn ( $" Executing task '{test.Origin.Name}' {test.GetType().Name}" );

                var result = await test.Task;

                Log ( " into it" );

                //
                // Handle errors
                //
                if (test.CancelOnError)
                {
                    if (test.HasErrored)
                    {
                        hasEnded = true;
                        Log ( " failure." );
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
                            test.Fail ( $"Assert result '{booleanResult}' when expected true" );

                            if (assert.CancelOnInvalid)
                            {
                                hasEnded = true;
                            }
                        }
                        else
                        {
                            test.Succeed ();
                        }
                        break;

                    case Test.WaitUntil waitUntil:
                        TimeSince start = 0;
                        Log ( " wait until" );

                        while (!waitUntil.Finalized && !waitUntil.TimedOut)
                        {
                            Log ( " ping" );

                            if (start >= waitUntil.WaitTimeout)
                            {
                                Log ( " timed out" );

                                waitUntil.Timeout ();
                            }

                            await AsyncEx.WaitForSeconds ( Frequency );
                        }
                        break;
                }

                Warn ( " done." );
            }
        }

        return !hasEnded;
    }
}