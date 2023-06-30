using System;

namespace ManagementHub.Models.Misc;

/// <summary>
/// Attribute denoting sensitive data which shouldn't be logged.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = true)]
public class SensitiveDataAttribute : Attribute
{
}
