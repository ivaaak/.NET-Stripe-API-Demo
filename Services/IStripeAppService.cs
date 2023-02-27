using Models;
using Stripe;

namespace Services
{
    public interface IStripeAppService
    {
        Task<StripeCustomer> AddStripeCustomerAsync(
            AddStripeCustomer customer, 
            CancellationToken ct);

        Task<StripePayment> AddStripePaymentAsync(
            AddStripePayment payment, 
            CancellationToken ct);

        Task<StripeCustomer> GetStripeCustomerByIdAsync(
            string customerId,
            CustomerGetOptions options = null,
            RequestOptions requestOptions = null,
            CancellationToken cancellationToken = default);

        Task<PaymentIntent> GetPaymentIntentAsync(
            string paymentIntentId, 
            CancellationToken cancellationToken = default);
    }
}

