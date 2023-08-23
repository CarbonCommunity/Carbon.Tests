using Carbon.Extensions;
using Carbon.Plugins;
using Facepunch;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Carbon.Integrations;

public class MainIntegration : CarbonPlugin
{
    public string ProtocolSource = CommandLineEx.GetArgumentResult ( "+test.protocol" );

    [Test ( CancelOnError = false )]
    public void Metadata ()
    {
        IntegrationManager.Log ( $" Rust" );
        IntegrationManager.Log ( $"   Scm" );
        IntegrationManager.Log ( $"     Type:       {BuildInfo.Current.Scm.Type}" );
        IntegrationManager.Log ( $"     ChangeId:   {BuildInfo.Current.Scm.ChangeId}" );
        IntegrationManager.Log ( $"     Branch:     {BuildInfo.Current.Scm.Branch}" );
        IntegrationManager.Log ( $"     Repo:       {BuildInfo.Current.Scm.Repo}" );
        IntegrationManager.Log ( $"     Comment:    {BuildInfo.Current.Scm.Comment}" );
        IntegrationManager.Log ( $"     Author:     {BuildInfo.Current.Scm.Author}" );
        IntegrationManager.Log ( $"     Date:       {BuildInfo.Current.Scm.Date}" );
        IntegrationManager.Log ( $"   Build" );
        IntegrationManager.Log ( $"     Id:         {BuildInfo.Current.Build.Id}" );
        IntegrationManager.Log ( $"     Number:     {BuildInfo.Current.Build.Number}" );
        IntegrationManager.Log ( $"     Tag:        {BuildInfo.Current.Build.Tag}" );
        IntegrationManager.Log ( $"     Url:        {BuildInfo.Current.Build.Url}" );
        IntegrationManager.Log ( $"     Name:       {BuildInfo.Current.Build.Name}" );
        IntegrationManager.Log ( $"     Node:       {BuildInfo.Current.Build.Node}" );
        IntegrationManager.Log ( $" Carbon" );
        IntegrationManager.Log ( $"   System" );
        IntegrationManager.Log ( $"     SystemID:   {Community.Runtime.Analytics.SystemID}" );
        IntegrationManager.Log ( $"     SessionID:  {Community.Runtime.Analytics.SessionID}" );
        IntegrationManager.Log ( $"     UserAgent:  {Community.Runtime.Analytics.UserAgent}" );
        IntegrationManager.Log ( $"   Build" );
        IntegrationManager.Log ( $"     Branch:     {Community.Runtime.Analytics.Branch}" );
        IntegrationManager.Log ( $"     Version:    {Community.Runtime.Analytics.Version}" );
        IntegrationManager.Log ( $"     Version2:   {Community.Runtime.Analytics.InformationalVersion}" );
        IntegrationManager.Log ( $"     Protocol:   {Community.Runtime.Analytics.Protocol}" );
        IntegrationManager.Log ( $"     Platform:   {Community.Runtime.Analytics.Platform}" );
        // IntegrationManager.Log ( $"   Git" );
        // IntegrationManager.Log ( $"     Branch:        {Community.Runtime.Analytics.Branch}" );
        // IntegrationManager.Log ( $"     Version:    {Community.Runtime.Analytics.Version}" );
        // IntegrationManager.Log ( $"     Version2:    {Community.Runtime.Analytics.InformationalVersion}" );
        // IntegrationManager.Log ( $"     Protocol:    {Community.Runtime.Analytics.Protocol}" );
        // IntegrationManager.Log ( $"     Platform:    {Community.Runtime.Analytics.Platform}" );
    }

    [Test.Assert ( CancelOnInvalid = true, CancelOnError = true )]
    public bool RunsLatestProtocol ()
    {
        var liveProtocol = JObject.Parse ( new WebClient ().DownloadString ( ProtocolSource ) ) [ "Protocol" ].Value<string> ();
        return Test.Assert.IsTrue ( Community.Runtime.Analytics.Protocol == liveProtocol, "Protocol valid", "Protocol invalid" );
    }
}