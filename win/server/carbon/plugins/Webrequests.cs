using Carbon.Integrations;
using Oxide.Core.Libraries;

namespace Carbon.Plugins;

[Info ( "Webrequests", "Carbon Community", "1.0.0" )]
public class Webrequests : CarbonPlugin
{
    internal const string GoogleUrl = "https://google.com";

    [Test.WaitUntil]
    public async void SyncGooglePing ( Test.WaitUntil wait )
    {
        webrequest.Enqueue ( GoogleUrl, null, ( code, result ) =>
        {
            wait.Done ();
        }, this, RequestMethod.GET, onException: ( code, obj, ex ) =>
        {
            wait.Fail ( $"Failed requesting '{GoogleUrl}'", ex );
        } );
    }

    [Test.WaitUntil]
    public async void AsyncGooglePing ( Test.WaitUntil wait )
    {
        await webrequest.EnqueueAsync ( GoogleUrl, null, ( code, result ) => { }, this, RequestMethod.GET, onException: ( code, obj, ex ) =>
        {
            wait.Fail ( $"Failed requesting '{GoogleUrl}'", ex );
        } );

        wait.Done ();
    }
}