using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Domain.Tests.Policies.Eligibility;
public class RefereeLanguageEligibilityPolicy : IRefereeEligibilityPolicy
{
	private readonly IUserContextProvider userContextProvider;

	public RefereeLanguageEligibilityPolicy(IUserContextProvider userContextProvider)
	{
		this.userContextProvider = userContextProvider;
	}

	public async Task<bool> IsUserEligibleForTestAsync(Test test, UserIdentifier userId)
	{
		var user = await this.userContextProvider.GetUserContextAsync(userId, default);

		if (test.Language == user.UserData.UserLang)
		{
			return true;
		}

		return false;
	}
}
