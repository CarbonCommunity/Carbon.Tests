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
		IntegrationManager.Log ( $"Booting" );

		Community.Runtime.Events.Subscribe ( API.Events.CarbonEvent.OnServerInitialized, async arg =>
		{
			try
            {
                IntegrationManager.Log ( $"Intializing Carbon.Integrations" );

                const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

                ModLoader.InitializePlugin ( typeof ( MainIntegration ), out _, new ModLoader.ModPackage
                {
                    Name = "Carbon.Integrations",
                    IsCoreMod = true
                } );

				IntegrationManager.Singleton.ResetTests ();

                foreach (var package in ModLoader.LoadedPackages)
				{
					foreach (var plugin in package.Plugins)
					{
						var methods = plugin.Type.GetMethods ( flags );

						foreach (var method in methods)
						{
							var test = method.GetCustomAttribute<Test> ();

							if (test == null)
							{
								continue;
							}

							IntegrationManager.Singleton.RegisterTest ( plugin, test, method );
						}
					}
				}

				var success = await IntegrationManager.Singleton.ExecuteTests ();

				if (success)
				{
					IntegrationManager.Log ( $"The run has been a success!" );
				}
				else
				{
                    IntegrationManager.Warn ( $"The run hasn't been a success." );
                }
            }
			catch (Exception ex)
			{
				Logger.Error ( "Failed doing something wild.", ex );
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
