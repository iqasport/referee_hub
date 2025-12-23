using ManagementHub.Models.Domain.General;
using Stripe.Checkout;

namespace ManagementHub.Service.Areas.Payments;

public interface IPaymentsService<TItem> where TItem : class
{
	/// <summary>
	/// Gets all available products for this item type.
	/// </summary>
	Task<CheckoutSession> CreateCheckoutSessionAsync(TItem item, Email userEmail, Uri successUrl, Uri? cancelUrl);

	/// <summary>
	/// Creates a checkout session with Stripe to purchase the specified item.
	/// </summary>
	Task<IEnumerable<Product<TItem>>> GetProductsAsync();

	/// <summary>
	/// Submits the completed session for background processing.
	/// </summary>
	void SubmitCheckoutSession(Session session);
}
