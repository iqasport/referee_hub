using ManagementHub.Models.Domain.Tests;
using ManagementHub.Models.Exceptions;
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
    private readonly IStripeClient stripeClient;

	public CertificationPaymentsController(IUserContextAccessor contextAccessor, IStripeClient stripeClient)
	{
		this.contextAccessor = contextAccessor;
		this.stripeClient = stripeClient;
	}

    // TODO: move as much Stripe logic as possible to a service class (2nd layer with certification specific stuff)

	[HttpGet("")]
	public async Task GetAvailablePayments()
	{
		var productService = new ProductService(this.stripeClient);
		var priceService = new PriceService(this.stripeClient);
        var products = await productService.ListAsync(new ProductListOptions
        {
            Active = true,
            Expand = new List<string> { "defaultprice" }, // TODO: validate this
            // Type = "service"?"referee_hub" // TODO: check with Jamie about types and stuff
        });
        
        var response = products.Select(p => new {
            Certification = (object?)null, // TODO
            DisplayName = p.Name,
            Price = new { p.DefaultPrice.UnitAmount, p.DefaultPrice.Currency }
        });
        // TODO: see what data we need to pass to ux and for other endpoints
        // TODO: viewmodel
		throw new NotImplementedException();
	}

    [HttpPost("create")]
    public async Task CreatePaymentSession([FromQuery] Certification certification)
    {
        // TODO: get product id and price for certification from parameter
        var productService = new ProductService(this.stripeClient);
        var products = await productService.ListAsync(new ProductListOptions
        {
            Active = true,
            // Type = "referee_hub_{version}_{level}" // TODO: check with Jamie about types and other metadata
        });
        var p = products./*Where(p => p is what I want).*/FirstOrDefault();

        if (p is null)
        {
            throw new NotFoundException(certification.ToString());
        }
        
        var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
        // TODO: add logging

        var serviceBaseUri = new Uri("http://localhost:5000"); // TODO: load from config
        var redirectPath = new Uri("/referees/me/tests", UriKind.Relative); // TODO: load from config
        var resultParameter = "paymentStatus";  // TODO: load from config

        string GetReturnUri(string result) => $"{new Uri(serviceBaseUri, redirectPath)}?{resultParameter}={result}";

        var sessionService = new SessionService(this.stripeClient);
        var session = await sessionService.CreateAsync(new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string>{ "card" },
            Mode = "payment",
            LineItems =  new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions 
                {
                    Quantity = 1,
                    Price = p.DefaultPriceId,
                }
            },
            CustomerEmail = userContext.UserData.Email.Value,
            AllowPromotionCodes = true,
            Metadata = new Dictionary<string, string>
            {
                ["certification_level"] = certification.Level.ToString(),
                ["certification_version"] = certification.Version.ToString(),
            },
            SuccessUrl = GetReturnUri("success"),
            CancelUrl = GetReturnUri("cancelled"),
        });

        var response = new {SessionId = session.Id};
        return; // TODO: create response model
    }

    [HttpPost("submit")]
    public async Task SubmitPaymentSession([FromBody] Event stripeEvent)
    {
        // TODO: log the event metadata
        // TODO: if event is 'session completed' or smth like that
        if (stripeEvent.Type != "TODO")
        {
            throw new InvalidOperationException($"Could not process Stripe event of type {stripeEvent.Type} in this endpoint");
        }
        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
        var email = session.CustomerEmail;
        // TODO: get user by email
        // TODO: get certification from metadata
    }
}
