using ManagementHub.Models.Domain.Tests;
using ManagementHub.Service.Contexts;
using ManagementHub.Service.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
	private readonly ILogger logger;

	public CertificationPaymentsController(IUserContextAccessor contextAccessor, IPaymentsService<Certification> paymentsService, ILogger<CertificationPaymentsController> logger)
	{
		this.contextAccessor = contextAccessor;
		this.paymentsService = paymentsService;
		this.logger = logger;
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
	[ExternalParameterInBody("stripeEvent", MediaType = "application/json")]
	public Task SubmitPaymentSession()
	{
		Event? stripeEvent = null;
		try
		{

			using (var reader = new StreamReader(this.HttpContext.Request.Body, leaveOpen: true))
			using (var jsonReader = new JsonTextReader(reader))
			{
				stripeEvent = JsonSerializer.Create().Deserialize<Event>(jsonReader);
			}
		}
		catch (Exception ex)
		{
			this.logger.LogError(0, ex, "Failed to deserialize Stripe event object.");
		}

		if (stripeEvent == null)
		{
			throw new ArgumentException();
		}

		if (stripeEvent.Type != PaymentsServiceConstants.CheckoutSessionCompleted)
		{
			throw new InvalidOperationException($"Could not process Stripe event of type {stripeEvent.Type} in this endpoint");
		}

		var session = (Session)stripeEvent.Data.Object;
		this.paymentsService.SubmitCheckoutSession(session);

		return Task.CompletedTask;
	}
}
