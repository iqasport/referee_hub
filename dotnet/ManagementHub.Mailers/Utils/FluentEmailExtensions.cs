using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Microsoft.AspNetCore.Components.RenderTree;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ManagementHub.Mailers.Utils;
internal static class FluentEmailExtensions
{
	public static string RenderToString(this IFluentEmail email)
	{
		var builder = new StringBuilder();

		builder.AppendLine($"""
			From: {email.Data.FromAddress.Name} <{email.Data.FromAddress.EmailAddress}>
			To: {string.Join(",", email.Data.ToAddresses.Select(x => $"{x.Name} <{x.EmailAddress}>"))}
			Cc: {string.Join(",", email.Data.CcAddresses.Select(x => $"{x.Name} <{x.EmailAddress}>"))}
			Bcc: {string.Join(",", email.Data.BccAddresses.Select(x => $"{x.Name} <{x.EmailAddress}>"))}
			ReplyTo: {string.Join(",", email.Data.ReplyToAddresses.Select(x => $"{x.Name} <{x.EmailAddress}>"))}
			Subject: {email.Data.Subject}
			""");

		foreach (var dataHeader in email.Data.Headers)
		{
			builder.AppendLine($"{dataHeader.Key}: {dataHeader.Value}");
		}

		builder.AppendLine().AppendLine(email.Data.Body);

		return builder.ToString();
	}

	public static IFluentEmail UsingEmbeddedTemplate<T>(this IFluentEmail email, string templateName, T? model)
	{
		email = email.UsingTemplateEngine(new EmbeddedRazorRenderer(typeof(FluentEmailExtensions).Assembly, "ManagementHub.Mailers"));
		var result = email.Renderer.Parse($"Templates.{templateName}.cshtml", model, isHtml: true);
		email.Data.IsHtml = true;
		email.Data.Body = result;
		return email;
	}

	public static IFluentEmail AttachInlineImageFromFile(this IFluentEmail email, string fileName, out string cid)
	{
		var contentType = Path.GetExtension(fileName).ToLower() switch
		{
			"jpg" => new ContentType("image/jpeg"),
			"jpeg" => new ContentType("image/jpeg"),
			"png" => new ContentType("image/png"),
			"svg" => new ContentType("image/svg+xml"),
			string ext => throw new NotSupportedException($"The image file extension '{ext}' is not supported.")
		};

		return email.AttachInlineImage(File.OpenRead(fileName), contentType, out cid);
	}

	public static IFluentEmail AttachInlineImage(this IFluentEmail email, Stream image, ContentType contentType, out string cid)
	{
		var guid = Guid.NewGuid();
		cid = Convert.ToHexString(MemoryMarshal.Cast<Guid, byte>(new Span<Guid>(ref guid)));

		email.Attach(new Attachment()
		{
			IsInline = true,
			ContentId = cid,
			ContentType = contentType.ToString(),
			Data = image,
			Filename = cid,
		});

		return email;
	}
}
