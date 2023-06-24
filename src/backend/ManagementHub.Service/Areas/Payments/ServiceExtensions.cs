using ManagementHub.Models.Domain.Tests;
using Microsoft.Extensions.Options;
using Stripe;

namespace ManagementHub.Service.Areas.Payments;

public static class ServiceExtensions
{
	public static void AddPaymentServices(this IServiceCollection services)
	{
		services.AddScoped<IPaymentsService<Certification>, CertificationPaymentsService>();

		services.AddOptions<StripeSettings>().BindConfiguration("Stripe");

		const string HttpClientName = nameof(StripeClient);
		services.AddHttpClient(HttpClientName);
		services.AddHttpClient<IStripeClient, StripeClient>(HttpClientName, (client, services) =>
		{
			var settings = services.GetRequiredService<IOptionsSnapshot<StripeSettings>>().Value;
			return new StripeClient(settings.ApiKey, clientId: null, new SystemNetHttpClient(client));
		});
	}
}
