using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Service.API.Test.WebsiteClient.HttpMessageMiddleware;

/// <summary>
/// Message handler which substitutes the handling of cookies of the <see cref="HttpClientHandler"/>.
/// It allows to reuse a single instance of a underlying handler across multiple cookie sessions.
/// </summary>
public class CookieSessionMessageHandler : DelegatingHandler
{
	public static readonly HttpRequestOptionsKey<CookieContainer> CookieContainerOption = new("CookieContainer");
	private readonly ILogger<CookieSessionMessageHandler> logger;

	public CookieSessionMessageHandler(ILogger<CookieSessionMessageHandler> logger)
	{
		this.logger = logger;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (request.Options.TryGetValue(CookieContainerOption, out var cookieContainer))
		{
			if (cookieContainer.Count > 0)
			{
				var cookieString = cookieContainer.GetCookieHeader(request.RequestUri!);
				request.Headers.Add("Cookie", cookieString);
				this.logger.LogDebug(0x6eb44401, "Sending cookies: {cookieString}", cookieString);
			}

			var response = await base.SendAsync(request, cancellationToken);

			if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
			{
				foreach (var cookieValue in cookieValues)
				{
					this.logger.LogDebug(0x6eb44403, "Received cookie: {cookie}", cookieValue);
					cookieContainer.SetCookies(request.RequestUri!, cookieValue);
				}
			}

			return response;
		}

		this.logger.LogDebug(0x6eb44402, "Skipping cookies");
		return await base.SendAsync(request, cancellationToken);
	}
}
