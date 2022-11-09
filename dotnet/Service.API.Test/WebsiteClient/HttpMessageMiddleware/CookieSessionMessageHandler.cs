using System;
using System.Collections.Concurrent;
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
	public static readonly HttpRequestOptionsKey<Guid> CookieContainerIdOption = new HttpRequestOptionsKey<Guid>("CookieContainerId");
	private readonly ConcurrentDictionary<Guid, CookieContainer> cookies = new();
	private readonly ILogger<CookieSessionMessageHandler> logger;

	public CookieSessionMessageHandler(ILogger<CookieSessionMessageHandler> logger)
	{
		this.logger = logger;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (request.Options.TryGetValue<Guid>(CookieContainerIdOption, out var id))
		{
			this.logger.LogDebug(0x6eb44400, "Processing cookies for cookie container id: {id}", id);
			var cookieContainer = cookies.GetOrAdd(id, _ => new CookieContainer());

			if (cookieContainer.Count > 0)
			{
				var cookieString = cookieContainer.GetCookieHeader(request.RequestUri!);
				request.Headers.Add("Cookie", cookieString);
				this.logger.LogDebug(0x6eb44401, "Sending cookies: {cookieString}", cookieString);
			}

			var response = await base.SendAsync(request, cancellationToken);

			if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
			{
				SetCookies(request, cookieContainer, cookieValues);
			}

			return response;
		}

		this.logger.LogDebug(0x6eb44402, "Skipping cookies");
		return await base.SendAsync(request, cancellationToken);
	}

	private void SetCookies(HttpRequestMessage request, CookieContainer cookieContainer, IEnumerable<string> cookieValues)
	{
		foreach (var item in SetCookieHeaderValue.ParseList(cookieValues.ToList()))
		{
			var uri = new Uri(request.RequestUri!, item.Path.Value);
			var cookie = new Cookie(item.Name.Value, item.Value.Value, item.Path.Value);

			if (item.MaxAge.HasValue)
			{
				cookie.Expires = DateTime.UtcNow + item.MaxAge.Value;
			}
			else if (item.Expires.HasValue)
			{
				cookie.Expires = item.Expires.Value.DateTime;
			}

			this.logger.LogDebug(0x6eb44403, "Received cookie: {cookie}", cookie);
			cookieContainer.Add(uri, cookie);
		}
	}
}
