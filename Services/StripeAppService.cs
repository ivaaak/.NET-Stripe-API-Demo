using Models;
using Models.Exceptions;
using Services;
using Stripe;

namespace Application
{
    public class StripeAppService : IStripeAppService
    {
        private readonly ChargeService _chargeService;
        private readonly CustomerService _customerService;
        private readonly TokenService _tokenService;

        public StripeAppService(
            ChargeService chargeService,
            CustomerService customerService,
            TokenService tokenService)
        {
            _chargeService = chargeService;
            _customerService = customerService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Create a new customer at Stripe through API using customer and card details from records.
        /// </summary>
        /// <param name="customer">Stripe Customer</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>Stripe Customer</returns>
        public async Task<StripeCustomer> AddStripeCustomerAsync(AddStripeCustomer customer, CancellationToken ct)
        {
            try
            {
                // Set Stripe Token options based on customer data
                TokenCreateOptions tokenOptions = new TokenCreateOptions
                {
                    Card = new TokenCardOptions
                    {
                        Name = customer.Name,
                        Number = customer.CreditCard.CardNumber,
                        ExpYear = customer.CreditCard.ExpirationYear,
                        ExpMonth = customer.CreditCard.ExpirationMonth,
                        Cvc = customer.CreditCard.Cvc
                    }
                };

                // Create new Stripe Token
                Token stripeToken = await _tokenService.CreateAsync(tokenOptions, null, ct);

                // Set Customer options using
                CustomerCreateOptions customerOptions = new CustomerCreateOptions
                {
                    Name = customer.Name,
                    Email = customer.Email,
                    Source = stripeToken.Id
                };

                // Create customer at Stripe
                Customer createdCustomer = await _customerService.CreateAsync(customerOptions, null, ct);

                // Return the created customer at stripe
                return new StripeCustomer(createdCustomer.Name, createdCustomer.Email, createdCustomer.Id);
            }
            catch (StripeException ex)
            {
                var errorType = ex.StripeError?.Type;
                switch (errorType)
                {
                    case "card_error":
                        throw new StripeCardException(ex.StripeError.Message, ex);
                    case "invalid_request_error":
                        throw new StripeInvalidRequestException(ex.StripeError.Message, ex);
                    default:
                        throw new Exception("An error occurred while processing the payment.", ex);
                }
            }
        }

        /// <summary>
        /// Add a new payment at Stripe using Customer and Payment details.
        /// Customer has to exist at Stripe already.
        /// </summary>
        /// <param name="payment">Stripe Payment</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns><Stripe Payment/returns>
        public async Task<StripePayment> AddStripePaymentAsync(AddStripePayment payment, CancellationToken ct)
        {
            try
            {
                // Set the options for the payment we would like to create at Stripe
                ChargeCreateOptions paymentOptions = new ChargeCreateOptions
                {
                    Customer = payment.CustomerId,
                    ReceiptEmail = payment.ReceiptEmail,
                    Description = payment.Description,
                    Currency = payment.Currency,
                    Amount = payment.Amount
                };

                // Create the payment
                var createdPayment = await _chargeService.CreateAsync(paymentOptions, null, ct);

                // Return the payment to requesting method
                return new StripePayment(
                  createdPayment.CustomerId,
                  createdPayment.ReceiptEmail,
                  createdPayment.Description,
                  createdPayment.Currency,
                  createdPayment.Amount,
                  createdPayment.Id);
            }
            catch (StripeException ex)
            {
                switch (ex.StripeError.Type)
                {
                    case "card_error":
                        throw new StripeCardException(ex.Message, ex);
                    case "api_connection_error":
                        throw new StripeException(ex.Message, ex);
                    case "invalid_request_error":
                        throw new StripeInvalidRequestException(ex.Message, ex);
                    default:
                        throw new Exception(ex.Message, ex);
                }
            }
        }
    }
}

