using System.Text.Json.Serialization;
using ManagementHub.Models.Domain.General;
using ManagementHub.Models.Exceptions;
using Stripe;
using Stripe.Checkout;

namespace ManagementHub.Service.Areas.Payments;

/// <summary>
/// An abstract base class for collecting payments using Stripe.
/// </summary>
/// <typeparam name="TItem">
/// Type of the item being purchased. The type MUST override equality!
/// </typeparam>
public abstract class PaymentsService<TItem> : IPaymentsService<TItem> where TItem : class
{
	private readonly IStripeClient stripeClient;
	private readonly ILogger<PaymentsService<TItem>> logger;

	public PaymentsService(IStripeClient stripeClient, ILogger<PaymentsService<TItem>> logger)
	{
		this.stripeClient = stripeClient;
		this.logger = logger;
	}

	/// <summary>
	/// Produces an item object from the Stripe product. Returns null if product does not represent a valid item.
	/// </summary>
	protected abstract TItem? ItemOfProduct(Product product);

	/// <summary>
	/// Checks if the user can purchase a given item (e.g. that they haven't purchased one before if they can have just one, or that they're eligible).
	/// </summary>
	protected abstract Task<IDictionary<TItem, ProductStatus>> CanPurchaseItemsAsync(IEnumerable<TItem> items);

	/// <summary>
	/// Converts item into string format.
	/// </summary>
	protected abstract Dictionary<string, string> ItemAsMetadata(TItem item);

	/// <summary>
	/// Converts item back from string format.
	/// </summary>
	protected abstract TItem ItemOfMetadata(Dictionary<string, string> metadata);

	/// <summary>
	/// Schedules further processing of a completed session as a background job.
	/// This is to avoid long running work on a callback which could be interrupted and the session lost.
	/// The background job should first perform eligibility check.
	/// </summary>
	protected abstract void ScheduleCompletedSessionProcessing(string sessionId, string userEmail, TItem item);

	/// <summary>
	/// Gets all available products for this item type.
	/// </summary>
	public async Task<IEnumerable<Product<TItem>>> GetProductsAsync()
	{
		var productService = new ProductService(this.stripeClient);
		var priceService = new PriceService(this.stripeClient);
		var products = await productService.ListAsync(new ProductListOptions
		{
			Active = true,
			Expand = new List<string> { "data.default_price" },
		});

		var productWithItem = products.Select(p => (product: p, item: this.ItemOfProduct(p)))
			.Where(pp => pp.item != null)
			.Cast<(Product product, TItem item)>()
			.ToDictionary(pp => pp.product, pp => pp.item);

		var itemWithStatus = await this.CanPurchaseItemsAsync(productWithItem.Values);

		return productWithItem.Select(pp => new Product<TItem>
		{
			Item = pp.Value,
			DisplayName = pp.Key.Name,
			Description = pp.Key.Description,
			Status = itemWithStatus[pp.Value],
			Price = new Price
			{
				PriceId = pp.Key.DefaultPriceId,
				Currency = pp.Key.DefaultPrice.Currency,
				// Stripe returns the price in cents (0.01 of Currency unit)
				// I'm going to normalize it into the decimal type.
				UnitPrice = pp.Key.DefaultPrice.UnitAmount!.Value / 100m,
			}
		});
	}

	/// <summary>
	/// Creates a checkout session with Stripe to purchase the specified item.
	/// </summary>
	public async Task<CheckoutSession> CreateCheckoutSessionAsync(TItem item, Email userEmail, Uri successUrl, Uri? cancelUrl)
	{
		ArgumentNullException.ThrowIfNull(item);

		var availableProducts = await this.GetProductsAsync();
		var product = availableProducts.FirstOrDefault(o => o.Item.Equals(item));

		if (product == default)
		{
			throw new NotFoundException(item.ToString() ?? nameof(item));
		}

		if (product.Status != ProductStatus.Available)
		{
			throw new InvalidOperationException($"Item {item} cannot be purchased.");
		}

		var sessionOptions = new SessionCreateOptions
		{
			PaymentMethodTypes = new List<string> { "card" },
			Mode = "payment",
			LineItems = new List<SessionLineItemOptions>
			{
				new SessionLineItemOptions
				{
					Quantity = 1,
					Price = product.Price.PriceId,
				}
			},
			CustomerEmail = userEmail.Value,
			AllowPromotionCodes = true,
			Metadata = this.ItemAsMetadata(item),
			SuccessUrl = successUrl.AbsoluteUri,
			CancelUrl = cancelUrl?.AbsoluteUri,
		};

		var sessionService = new SessionService(this.stripeClient);
		var session = await sessionService.CreateAsync(sessionOptions);

		this.logger.LogInformation(0x62475600, "Created checkout session ({sessionId}) for item ({item}).", session.Id, item);

		return new CheckoutSession { SessionId = session.Id };
	}

	/// <summary>
	/// Submits the completed session for background processing.
	/// </summary>
	public void SubmitCheckoutSession(Session session)
	{
		this.logger.LogInformation(0x62475601, "Session ({sessionId}) has been submitted.", session.Id);

		if (string.IsNullOrWhiteSpace(session.CustomerEmail))
		{
			throw new ArgumentException("Missing user email.");
		}

		this.ScheduleCompletedSessionProcessing(session.Id, session.CustomerEmail, this.ItemOfMetadata(session.Metadata));
	}
}

public static class PaymentsServiceConstants
{
	public static readonly string CheckoutSessionCompleted = "checkout.session.completed";
}

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum ProductStatus
{
	Available,
	AlreadyPurchased,
}

public class Product<TItem>
{
	public required string DisplayName { get; set; }
	public required string Description { get; set; }
	public required TItem Item { get; set; }
	public required Price Price { get; set; }
	public required ProductStatus Status { get; set; }
}

public struct Price
{
	public required string PriceId { get; set; }
	public required decimal UnitPrice { get; set; }
	public required string Currency { get; set; }
}

public class CheckoutSession
{
	public required string SessionId { get; set; }
}
