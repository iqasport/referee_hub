﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using netDumbster.smtp;
using Service.API.Test.Settings;
using Xunit;

namespace Service.API.Test.EmailClient
{
	/// <summary>
	/// (singleton) Wrapper around an SMTP listener to allow for asserting tests asynchronously.
	/// </summary>
	public class EmailProvider : IHostedService, IDisposable
	{
		private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);
		private readonly List<SmtpMessage> emailMessages = new();
		private readonly EmailClientSettings emailClientSettings;
		private SimpleSmtpServer? smtpServer;

		public EmailProvider(ILoggerFactory loggerFactory, IOptions<EmailClientSettings> settings)
		{
			netDumbster.smtp.Logging.LogManager.GetLogger = type => new LogWrapper(loggerFactory.CreateLogger(type));
			this.emailClientSettings = settings.Value;
		}

		public IReadOnlyCollection<SmtpMessage> Messages => new ReadOnlyCollection<SmtpMessage>(emailMessages);

		public void Dispose() => this.smtpServer?.Stop();

		public async Task<SmtpMessage> PollAsync(Func<SmtpMessage, bool> predicate, TimeSpan? timeout = null)
		{
			if (this.smtpServer is null)
			{
				await this.StartAsync(CancellationToken.None);
			}

			timeout ??= TimeSpan.FromSeconds(10);
			using var cts = new CancellationTokenSource(timeout.Value);
			while (true)
			{
				Assert.False(cts.Token.IsCancellationRequested, "Smtp polling timeout");

				var message = this.emailMessages.SingleOrDefault(predicate);
				if (message != null)
					return message;

				await Task.Delay(PollingInterval);
			}
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			this.smtpServer = SimpleSmtpServer.Start(this.emailClientSettings.SmtpServerPort, useMessageStore: false /* I'm storing messages myself */);
			this.smtpServer.MessageReceived += (_, messageArgs) =>
				this.emailMessages.Add(messageArgs.Message);

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			this.smtpServer?.Stop();
			return Task.CompletedTask;
		}

		private class LogWrapper : netDumbster.smtp.Logging.ILog
		{
			private readonly ILogger logger;

			public LogWrapper(ILogger logger)
			{
				this.logger = logger;
			}

			public void Debug(object message) => logger.LogDebug(message.ToString());

			public void Debug(object message, Exception exception) => logger.LogDebug(exception, message.ToString());

			public void DebugFormat(string format, params object[] args) => logger.LogDebug(string.Format(format, args));

			public void DebugFormat(IFormatProvider provider, string format, params object[] args) => logger.LogDebug(string.Format(provider, format, args));

			public void Error(object message)
			=> logger.LogError(message.ToString());

			public void Error(object message, Exception exception)
			=> logger.LogError(exception, message.ToString());

			public void ErrorFormat(string format, params object[] args)
			=> logger.LogError(string.Format(format, args));

			public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
			=> logger.LogError(string.Format(provider, format, args));

			public void Fatal(object message)
			=> logger.LogCritical(message.ToString());

			public void Fatal(object message, Exception exception)
			=> logger.LogCritical(exception, message.ToString());

			public void FatalFormat(string format, params object[] args)
			=> logger.LogCritical(string.Format(format, args));

			public void FatalFormat(IFormatProvider provider, string format, params object[] args)
			=> logger.LogCritical(string.Format(provider, format, args));

			public void Info(object message)
			=> logger.LogInformation(message.ToString());

			public void Info(object message, Exception exception)
			=> logger.LogInformation(exception, message.ToString());

			public void InfoFormat(string format, params object[] args)
			=> logger.LogInformation(string.Format(format, args));

			public void InfoFormat(IFormatProvider provider, string format, params object[] args)
			=> logger.LogInformation(string.Format(provider, format, args));

			public void Warn(object message)
			=> logger.LogWarning(message.ToString());

			public void Warn(object message, Exception exception)
			=> logger.LogWarning(exception, message.ToString());

			public void WarnFormat(string format, params object[] args)
			=> logger.LogWarning(string.Format(format, args));

			public void WarnFormat(IFormatProvider provider, string format, params object[] args)
			=> logger.LogWarning(string.Format(provider, format, args));
		}
	}
}