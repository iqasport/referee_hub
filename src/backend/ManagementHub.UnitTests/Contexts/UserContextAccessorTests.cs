using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.User;
using ManagementHub.Models.Exceptions;
using ManagementHub.Service.Contexts;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace ManagementHub.UnitTests.Contexts;

public class UserContextAccessorTests
{
	[Fact]
	public async Task GetCurrentUserContextAsync_WhenAuthenticatedUserNoLongerExists_RequiresAuthentication()
	{
		var userId = UserIdentifier.NewUserId();
		var contextProvider = new Mock<IUserContextProvider>();
		contextProvider
			.Setup(provider => provider.GetUserContextAsync(userId, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new NotFoundException(userId.ToString()));

		var currentUserGetter = new Mock<ICurrentUserGetter>();
		currentUserGetter.SetupGet(getter => getter.CurrentUser).Returns(userId);

		var accessor = new UserContextAccessor(
			new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
			contextProvider.Object,
			currentUserGetter.Object);

		var action = () => accessor.GetCurrentUserContextAsync();

		await action.Should().ThrowAsync<AuthenticationRequiredException>()
			.WithMessage("The signed-in user no longer exists. Sign in again.");
	}
}