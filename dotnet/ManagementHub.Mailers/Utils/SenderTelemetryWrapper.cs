using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using FluentEmail.Core.Models;

namespace ManagementHub.Mailers.Utils;
internal class SenderTelemetryWrapper : ISender
{
	private static readonly ActivitySource ActivitySource = new ActivitySource("ManagementHub.Mailers");
	private readonly ISender sender;

	public SenderTelemetryWrapper(ISender sender)
	{
		this.sender = sender;
	}

	public SendResponse Send(IFluentEmail email, CancellationToken? token = null)
	{
		using var activity = this.StartActivity(email);
		try
		{
			var response = this.sender.Send(email, token);
			if (response.Successful)
			{
				activity?.SetStatus(ActivityStatusCode.Ok);
			}
			else
			{
				activity?.SetStatus(ActivityStatusCode.Error, string.Join("; ", response.ErrorMessages));
			}
			return response;
		}
		catch (Exception ex)
		{
			activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
			return new SendResponse { ErrorMessages = { ex.Message } };
		}
	}

	public async Task<SendResponse> SendAsync(IFluentEmail email, CancellationToken? token = null)
	{
		using var activity = this.StartActivity(email);
		try
		{
			var response = await this.sender.SendAsync(email, token);
			if (response.Successful)
			{
				activity?.SetStatus(ActivityStatusCode.Ok);
			}
			else
			{
				activity?.SetStatus(ActivityStatusCode.Error, string.Join("; ", response.ErrorMessages));
			}
			return response;
		}
		catch (Exception ex)
		{
			activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
			return new SendResponse { ErrorMessages = { ex.Message } };
		}
	}

	private Activity? StartActivity(IFluentEmail email)
	{
		var tags = new Dictionary<string, object?>
		{
			["email.attachments"] = email.Data.Attachments.Count,
			["email.ishtml"] = email.Data.IsHtml,
		};

		return ActivitySource.StartActivity("SendEmail", ActivityKind.Internal, default(ActivityContext), tags);
	}
}
