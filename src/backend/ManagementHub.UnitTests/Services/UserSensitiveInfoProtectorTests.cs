using System.Security.Cryptography;
using FluentAssertions;
using ManagementHub.Storage.Services;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using Xunit;

namespace ManagementHub.UnitTests.Services;

public class UserSensitiveInfoProtectorTests
{
	[Fact]
	public void Unprotect_WhenLegacyPlaintextStartsWithProtectionPrefix_ReturnsOriginalValue()
	{
		var dataProtector = new Mock<IDataProtector>();
		dataProtector
			.Setup(protector => protector.Unprotect(It.IsAny<byte[]>()))
			.Throws(new CryptographicException());

		var dataProtectionProvider = new Mock<IDataProtectionProvider>();
		dataProtectionProvider
			.Setup(provider => provider.CreateProtector("ManagementHub.UserSensitiveInfo"))
			.Returns(dataProtector.Object);

		var sensitiveInfoProtector = new UserSensitiveInfoProtector(dataProtectionProvider.Object);

		sensitiveInfoProtector.Unprotect("dp:legacy-value").Should().Be("dp:legacy-value");
	}

	[Fact]
	public void Unprotect_WhenLegacyPlaintextHasMalformedProtectedSuffix_ReturnsOriginalValue()
	{
		var dataProtector = new Mock<IDataProtector>();
		var dataProtectionProvider = new Mock<IDataProtectionProvider>();
		dataProtectionProvider
			.Setup(provider => provider.CreateProtector("ManagementHub.UserSensitiveInfo"))
			.Returns(dataProtector.Object);

		var sensitiveInfoProtector = new UserSensitiveInfoProtector(dataProtectionProvider.Object);

		sensitiveInfoProtector.Unprotect("dp:legacy value").Should().Be("dp:legacy value");
	}
}
