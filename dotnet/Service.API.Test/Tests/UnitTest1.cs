using System.Net.Http;
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
        var res = await requestBuilder.GetStringAsync();
        Assert.Contains("Argentina", res);
    }

    [Fact]
    public async void Test2()
    {
        var requestBuilder = new RequestBuilder(httpClientFactory, logger)
            .AsReferee()
            .WithEndpoint("/api/v1/tests");
        var res = await requestBuilder.GetStringAsync();
        Assert.Contains("\"type\":\"test\"", res);
    }
}