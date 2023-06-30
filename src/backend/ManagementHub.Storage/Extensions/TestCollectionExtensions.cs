using System.Linq;
using ManagementHub.Models.Domain.Tests;

namespace ManagementHub.Storage.Extensions;
public static class TestCollectionExtensions
{
	public static IQueryable<Models.Data.Test> WithIdentifier(this IQueryable<Models.Data.Test> tests, TestIdentifier testId) =>
		tests.Where(test => test.UniqueId == testId.ToString() || (test.UniqueId == null && test.Id == testId.ToLegacyUserId()));
}
