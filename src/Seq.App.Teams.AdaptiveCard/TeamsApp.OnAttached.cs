using System.IO;
using System.Net;
using System.Net.Http;

namespace Seq.App.Teams;

public sealed partial class TeamsApp
{
    /// <inheritdoc />
    protected override void OnAttached()
    {
        _log = TraceMessage ? Log.ForContext("Uri", TeamsBaseUrl) : Log;

        _httpClientHandler = new HttpClientHandler();
        if (!string.IsNullOrEmpty(WebProxy))
        {
            ICredentials? credentials = null;
            if (!string.IsNullOrEmpty(WebProxyUserName))
            {
                credentials = new NetworkCredential(WebProxyUserName, WebProxyPassword);
            }
            _httpClientHandler.Proxy = new WebProxy(WebProxy, false, null, credentials);
            _httpClientHandler.UseProxy = true;
        }
        else
        {
            _httpClientHandler.UseProxy = false;
        }

        using var stream = GetType().Assembly.GetManifestResourceStream("Seq.App.Teams.AdaptiveCard.Resources.default-template.json");
        using var reader = new StreamReader(stream);
        _defaultTemplate = reader.ReadToEnd();

        RegisterCustomFunctions();
    }
}
