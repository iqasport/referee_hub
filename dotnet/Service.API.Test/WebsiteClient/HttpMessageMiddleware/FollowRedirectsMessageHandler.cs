using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Service.API.Test.WebsiteClient.HttpMessageMiddleware;

/// <summary>
/// Message handler which substitutes the handling of redirects of the <see cref="HttpClientHandler"/>.
/// We needed redirection layer on top of the custom cookie middleware.
/// </summary> 
public class FollowRedirectsMessageHandler : DelegatingHandler
{
    private readonly ILogger<FollowRedirectsMessageHandler> logger;

    public FollowRedirectsMessageHandler(ILogger<FollowRedirectsMessageHandler> logger)
    {
        this.logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.Headers.TryGetValues("Location", out var locations))
        {
            var target = locations.First();
            this.logger.LogInformation(0x54980000, "Location header present in reposnse. Redirecting to {url}", target);

            var redirectRequest = new HttpRequestMessage(HttpMethod.Get, target);
            if (request.Options.TryGetValue(CookieSessionMessageHandler.CookieContainerIdOption, out var cookieSession))
            {
                redirectRequest.Options.Set(CookieSessionMessageHandler.CookieContainerIdOption, cookieSession);
            }

            return await this.SendAsync(redirectRequest, cancellationToken);
        }

        return response;
    }
}