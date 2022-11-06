using System.Net.Http;

using ManagementHub.Models;

using Microsoft.Extensions.Logging;

using Service.API.Test.WebsiteClient;

using Xunit;

namespace Service.API.Test.Tests;

public class UnitTest1
{
	private readonly IHttpClientFactory httpClientFactory;
	private readonly ILogger<UnitTest1> logger;

	public UnitTest1(IHttpClientFactory httpClientFactory, ILogger<UnitTest1> logger)
	{
		this.httpClientFactory = httpClientFactory;
		this.logger = logger;
	}

	[Fact]
	public async void Test1()
	{
		var requestBuilder = new RequestBuilder(httpClientFactory, logger)
			.Anonymous()
			.WithEndpoint("/api/v1/national_governing_bodies");
		var result = await requestBuilder.GetModelListAsync<NationalGoverningBody>();
		Assert.NotNull(result);
		Assert.Contains("Argentina", result[0].Name);
	}

	[Fact]
	public async void Test2()
	{
		var requestBuilder = new RequestBuilder(httpClientFactory, logger)
			.AsReferee()
			.WithEndpoint("/api/v1/tests");
		var result = await requestBuilder.GetModelListAsync<ManagementHub.Models.Test>();
		Assert.NotNull(result);
	}

	[Fact]
	public async void Test3()
	{
		var requestBuilder = new RequestBuilder(httpClientFactory, logger)
			.AsReferee()
			.WithEndpoint("/api/v1/users/current_user");
		var result = await requestBuilder.GetModelAsync<User>();
		Assert.NotNull(result);
		Assert.Equal("Orval", result.FirstName);
		Assert.Equal("Kris", result.LastName);
	}
}
