using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ManagementHub.Models.Domain.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Identity;
public class EmailTokenProvider : IUserTwoFactorTokenProvider<UserIdentity>
{
	private readonly IUserIdentityRepository userRepository;
	private readonly ILogger<EmailTokenProvider> logger;

	public EmailTokenProvider(
		IUserIdentityRepository userRepository,
		ILogger<EmailTokenProvider> logger)
	{
		this.userRepository = userRepository;
		this.logger = logger;
	}

	public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<UserIdentity> manager, UserIdentity user)
	{
		return Task.FromResult(true);
	}

	public async Task<string> GenerateAsync(string purpose, UserManager<UserIdentity> manager, UserIdentity user)
	{
		if (purpose == UserManager<UserIdentity>.ConfirmEmailTokenPurpose)
		{
			this.logger.LogInformation(0, "Generating token for email confirmation.");
			
			var token = new TokenData
			{
				Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)),
				Email = user.UserEmail.Value,
				Expires = DateTime.UtcNow.AddDays(1),
            };
			var base64encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(token)));

			this.logger.LogInformation(0, "Saving token for email confirmation to the database.");
			await this.userRepository.SetEmailConfirmationToken(user, base64encoded, default);

			return base64encoded;
		}

		throw new NotImplementedException();
	}

	public async Task<bool> ValidateAsync(string purpose, string token, UserManager<UserIdentity> manager, UserIdentity user)
	{
        if (purpose == UserManager<UserIdentity>.ConfirmEmailTokenPurpose)
        {
            this.logger.LogInformation(0, "Validating token for email confirmation.");

            var tokenData = JsonSerializer.Deserialize<TokenData>(Encoding.UTF8.GetString(Convert.FromBase64String(token)));

			if (tokenData is null)
			{
                this.logger.LogInformation(0, "Token could not be deserialized.");
				return false;
            }

            if (!string.Equals(tokenData.Email, user.UserEmail.Value, StringComparison.OrdinalIgnoreCase))
			{
                this.logger.LogInformation(0, "Token contains incorrect email.");
				return false;
            }

			if (tokenData.Expires < DateTime.UtcNow)
			{
				this.logger.LogInformation(0, "Token has expired.");
				return false;
			}

            this.logger.LogInformation(0, "Checking if token matches the one from the database.");
            var dbToken = await this.userRepository.GetEmailConfirmationToken(user);

            return token == dbToken;
        }

        throw new NotImplementedException();
    }

    private class TokenData
	{
		public string Token { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public DateTime Expires { get; set; }
	}
}
