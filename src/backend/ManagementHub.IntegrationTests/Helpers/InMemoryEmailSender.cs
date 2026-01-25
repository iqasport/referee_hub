using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using FluentEmail.Core.Models;

namespace ManagementHub.IntegrationTests.Helpers;

/// <summary>
/// In-memory email sender for integration tests that collects sent emails.
/// </summary>
public class InMemoryEmailSender : ISender
{
	private readonly ConcurrentBag<SentEmail> _sentEmails = new();

	public SendResponse Send(IFluentEmail email, CancellationToken? token = null)
	{
		var sentEmail = new SentEmail
		{
			To = string.Join(", ", email.Data.ToAddresses),
			Subject = email.Data.Subject,
			Body = email.Data.Body,
			IsHtml = email.Data.IsHtml
		};

		this._sentEmails.Add(sentEmail);
		return new SendResponse();
	}

	public Task<SendResponse> SendAsync(IFluentEmail email, CancellationToken? token = null)
	{
		return Task.FromResult(this.Send(email, token));
	}

	/// <summary>
	/// Gets all sent emails.
	/// </summary>
	public SentEmail[] GetSentEmails() => this._sentEmails.ToArray();

	/// <summary>
	/// Clears all sent emails.
	/// </summary>
	public void Clear() => this._sentEmails.Clear();
}

/// <summary>
/// Represents a sent email for testing purposes.
/// </summary>
public class SentEmail
{
	public required string To { get; init; }
	public required string Subject { get; init; }
	public required string Body { get; init; }
	public required bool IsHtml { get; init; }
}
