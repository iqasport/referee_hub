using System;

namespace ManagementHub.Models.Exceptions;

/// <summary>
/// Exception thrown when configuration is invalid or missing.
/// </summary>
public class ConfigurationException : Exception
{
	public ConfigurationException(string? message) : base(message)
	{
	}
}
