using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.Models.Exceptions;
using ManagementHub.Service.Authorization;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ManagementHub.UnitTests.Authorization;

public class AuthenticationRequiredExceptionMiddlewareTests
{
	[Fact]
	public async Task InvokeAsync_WhenAuthenticationIsRequired_ReturnsUnauthorized()
	{
		var context = new DefaultHttpContext();
		context.Response.Body = new MemoryStream();
		var middleware = new AuthenticationRequiredExceptionMiddleware(
			_ => throw new AuthenticationRequiredException("Sign in again."));

		await middleware.InvokeAsync(context);

		context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
		context.Response.Body.Position = 0;
		using var reader = new StreamReader(context.Response.Body);
		(await reader.ReadToEndAsync()).Should().Be("Sign in again.");
	}

	[Fact]
	public async Task InvokeAsync_WhenAnotherExceptionOccurs_RethrowsException()
	{
		var context = new DefaultHttpContext();
		var middleware = new AuthenticationRequiredExceptionMiddleware(
			_ => throw new InvalidOperationException("Unexpected failure."));

		var action = () => middleware.InvokeAsync(context);

		await action.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("Unexpected failure.");
	}
}