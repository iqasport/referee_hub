using ManagementHub.Models.Domain.Tests;
using ManagementHub.Service.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace ManagementHub.Service.Areas.Payments;

/// <summary>
/// Actions related to processing certification related payments.
/// </summary>
[ApiController]
[Route("api/v2/certifications/payments/")]
[Produces("application/json")]
[Authorize]
public class CertificationPaymentsController : ControllerBase
{
	private readonly IUserContextAccessor contextAccessor;
	private readonly IPaymentsService<Certification> paymentsService;

	public CertificationPaymentsController(IUserContextAccessor contextAccessor, IPaymentsService<Certification> paymentsService)
	{
		this.contextAccessor = contextAccessor;
		this.paymentsService = paymentsService;
	}

	[HttpGet("")]
	public async Task<IEnumerable<Product<Certification>>> GetAvailablePayments()
	{
		return await this.paymentsService.GetProductsAsync();
	}

	[HttpPost("create")]
	public async Task<CheckoutSession> CreatePaymentSession([FromQuery] Certification certification)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();

		var serviceBaseUri = new Uri("http://localhost:5000"); // TODO: load from config
		var redirectPath = new Uri("/referees/me/tests", UriKind.Relative); // TODO: load from config
		var resultParameter = "paymentStatus";  // TODO: load from config

		Uri GetReturnUri(string result) => new Uri($"{serviceBaseUri.AbsoluteUri}{redirectPath}?{resultParameter}={result}", UriKind.Absolute);

		return await this.paymentsService.CreateCheckoutSessionAsync(
			certification,
			userContext.UserData.Email,
			successUrl: GetReturnUri("success"),
			cancelUrl: GetReturnUri("cancelled"));
	}

	[HttpPost("submit")]
	public Task SubmitPaymentSession([FromBody] Event stripeEvent)
	{
		if (stripeEvent.Type != PaymentsServiceConstants.CheckoutSessionCompleted)
		{
			throw new InvalidOperationException($"Could not process Stripe event of type {stripeEvent.Type} in this endpoint");
		}

		var session = (Session)stripeEvent.Data.Object;
		this.paymentsService.SubmitCheckoutSession(session);

		return Task.CompletedTask;
	}
}
