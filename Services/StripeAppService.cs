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
        private readonly PaymentIntentService _paymentIntentService;

        public StripeAppService(
            ChargeService chargeService,
            CustomerService customerService,
            TokenService tokenService,
            PaymentIntentService paymentIntentService)
        {
            _chargeService = chargeService;
            _customerService = customerService;
            _tokenService = tokenService;
            _paymentIntentService = paymentIntentService;
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
        /// <returns>Stripe Payment</returns>
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


        /// <summary>
        /// Retrieves a customer with the specified ID from Stripe API.
        /// </summary>
        /// <param name="customerId">The ID of the customer to retrieve.</param>
        /// <param name="options">Additional options for the customer retrieval request.</param>
        /// <param name="requestOptions">Request options for the API request.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Returns a Task that represents the asynchronous operation. The task result contains the retrieved Stripe customer object.</returns>
        public async Task<StripeCustomer> GetStripeCustomerByIdAsync(
            string customerId, 
            CustomerGetOptions options = null, 
            RequestOptions requestOptions = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Retrieve the customer from Stripe API
                CustomerService customerService = new CustomerService();
                Customer stripeCustomer = await customerService.GetAsync(customerId, options = null, requestOptions = null, cancellationToken = default);

                // Map the Stripe customer to our domain model
                StripeCustomer customer = new StripeCustomer(stripeCustomer.Name, stripeCustomer.Email, stripeCustomer.Id);

                return customer;
            }
            catch (StripeException ex)
            {
                switch (ex.StripeError.Type)
                {
                    case "api_connection_error":
                        throw new Exception("A problem occurred while connecting to the API.", ex);
                    case "api_error":
                        throw new Exception("An error occurred while making the API request.", ex);
                    case "authentication_error":
                        throw new Exception("Authentication with the API failed.", ex);
                    case "card_error":
                        throw new Exception("An error occurred while processing the credit card.", ex);
                    case "invalid_request_error":
                        throw new Exception("The request to the API was invalid.", ex);
                    case "rate_limit_error":
                        throw new Exception("The API rate limit was exceeded.", ex);
                    default:
                        throw new Exception("An unknown error occurred while communicating with the API.", ex);
                }
            }
        }


        /// <summary>
        /// Retrieves a PaymentIntent object from the Stripe API.
        /// </summary>
        /// <param name="paymentIntentId">The ID of the PaymentIntent to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The retrieved PaymentIntent object.</returns>
        public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
        {
            try
            {
                var paymentIntent = await _paymentIntentService.GetAsync(paymentIntentId, cancellationToken: cancellationToken);

                return paymentIntent;
            }
            catch (StripeException e)
            {
                if (e.StripeError != null)
                {
                    // Handle specific error types.
                    switch (e.StripeError.Type)
                    {
                        case "api_connection_error":
                            throw new Exception("Network communication with Stripe failed.");
                        case "api_error":
                            throw new Exception("An error occurred while communicating with Stripe.");
                        case "authentication_error":
                            throw new Exception("Authentication with Stripe failed.");
                        case "card_error":
                            throw new Exception("Card error: " + e.StripeError.Message);
                        case "idempotency_error":
                            throw new Exception("Idempotency error occurred.");
                        case "invalid_request_error":
                            throw new Exception("Invalid request to Stripe: " + e.StripeError.Message);
                        case "rate_limit_error":
                            throw new Exception("Too many requests made to the Stripe API.");
                        default:
                            throw new Exception("Unknown error occurred while communicating with Stripe.");
                    }
                }
                else
                {
                    // Handle other types of exceptions.
                    throw new Exception("An error occurred while communicating with Stripe.", e);
                }
            }
        }


    }
}

