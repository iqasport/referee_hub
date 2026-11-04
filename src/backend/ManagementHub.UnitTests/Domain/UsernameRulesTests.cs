using ManagementHub.Models.Domain.User;
using Xunit;

namespace ManagementHub.UnitTests.Domain;

public class UsernameRulesTests
{
	[Theory]
	[InlineData("abc")]
	[InlineData("andres.vargas")]
	[InlineData("jimmy_ref-2")]
	[InlineData("123")]
	[InlineData("a23456789012345678901234567890")]
	public void IsValid_AcceptsValidUsernames(string username) => Assert.True(UsernameRules.IsValid(username));

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("ab")]
	[InlineData("me")]
	[InlineData(".abc")]
	[InlineData("abc.")]
	[InlineData("ab c")]
	[InlineData("Abc")]
	[InlineData("abc@example.com")]
	[InlineData("a234567890123456789012345678901")]
	public void IsValid_RejectsInvalidUsernames(string? username) => Assert.False(UsernameRules.IsValid(username));

	[Fact]
	public void Normalize_TrimsAndLowercases() => Assert.Equal("andres.vargas", UsernameRules.Normalize("  Andres.Vargas "));

	[Theory]
	[InlineData("a@b.com", true)]
	[InlineData("andres.vargas", false)]
	public void LooksLikeEmail(string input, bool expected) => Assert.Equal(expected, UsernameRules.LooksLikeEmail(input));

	[Theory]
	[InlineData("Andrés", "Vargas", "andres.vargas")]
	[InlineData("Jean-Luc", "Picard", "jean.luc.picard")]
	[InlineData("Jürgen", "Weiß", "jurgen.weiss")]
	[InlineData("Łukasz", "Øster", "lukasz.oster")]
	[InlineData("Mary Ann", "O'Brien", "mary.ann.o.brien")]
	[InlineData("Cher", null, "cher")]
	[InlineData(null, "Smith", "smith")]
	[InlineData("Bartholomew", "Featherstonehaugh-Smith", "bartholomew.featherstoneha")]
	public void CreateBase_GeneratesFromNames(string? first, string? last, string expected)
	{
		var result = UsernameRules.CreateBase(first, last);
		Assert.Equal(expected, result);
		Assert.True(UsernameRules.IsValid(result));
	}

	[Theory]
	[InlineData(null, null)]
	[InlineData("", " ")]
	[InlineData("Al", null)]
	[InlineData("李", "王")]
	public void CreateBase_ReturnsNullWhenNamesAreUnusable(string? first, string? last) => Assert.Null(UsernameRules.CreateBase(first, last));

	[Fact]
	public void CreateBase_LongNamesLeaveRoomForSuffix()
	{
		var result = UsernameRules.CreateBase(new string('a', 40), "b");
		Assert.NotNull(result);
		Assert.True(result!.Length <= UsernameRules.MaxBaseLength);
		Assert.True(UsernameRules.IsValid(UsernameRules.WithSuffix(result, 999)));
	}

	[Fact]
	public void WithSuffix_FirstCandidateIsBase()
	{
		Assert.Equal("andres.vargas", UsernameRules.WithSuffix("andres.vargas", 1));
		Assert.Equal("andres.vargas2", UsernameRules.WithSuffix("andres.vargas", 2));
	}

	[Fact]
	public void FallbackFor_IsValidUsername() => Assert.True(UsernameRules.IsValid(UsernameRules.FallbackFor(7)));
}
