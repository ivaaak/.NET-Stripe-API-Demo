using Microsoft.AspNetCore.Mvc;
using Models;
using Services;
using Stripe;

namespace Controllers
{
    [Route("api/[controller]")]
    public class StripeController : Controller
    {
        private readonly IStripeAppService _stripeService;

        public StripeController(IStripeAppService stripeService)
        {
            _stripeService = stripeService;
        }


        /// <summary>
        /// Create a new customer at Stripe through API using customer and card details from records.
        /// </summary>
        /// <param name="customer">Stripe Customer</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>Stripe Customer</returns>
        [HttpPost("customer/add")]
        public async Task<ActionResult<StripeCustomer>> AddStripeCustomer(
            [FromBody] AddStripeCustomer customer,
            CancellationToken ct)
        {
            try
            {
                StripeCustomer createdCustomer = await _stripeService.AddStripeCustomerAsync(customer, ct);

                return StatusCode(StatusCodes.Status200OK, createdCustomer);
            }
            catch (StripeException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
        }


        /// <summary>
        /// Add a new payment at Stripe using Customer and Payment details.
        /// Customer has to exist at Stripe already.
        /// </summary>
        /// <param name="payment">Stripe Payment</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>Stripe Payment</returns>
        [HttpPost("payment/add")]
        public async Task<ActionResult<StripePayment>> AddStripePayment(
            [FromBody] AddStripePayment payment,
            CancellationToken ct)
        {
            
            try
            {
                StripePayment createdPayment = await _stripeService.AddStripePaymentAsync(payment, ct);

                return StatusCode(StatusCodes.Status200OK, createdPayment);
            }
            catch (StripeException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
        }


        /// <summary>
        /// Retrieves a customer with the specified ID from Stripe API.
        /// </summary>
        /// <param name="customerId">The ID of the customer to retrieve.</param>
        /// <param name="options">Additional options for the customer retrieval request.</param>
        /// <param name="requestOptions">Request options for the API request.</param>
        /// <param name="ct">Cancellation token for the async operation.</param>
        /// <returns>Returns a Task that represents the asynchronous operation.The task result contains the retrieved Stripe customer object.</returns>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<StripeCustomer>> GetStripeCustomer(
            string customerId,
            CustomerGetOptions options,
            RequestOptions requestOptions,
            CancellationToken ct)
        {
            try
            {
                var customer = await _stripeService.GetStripeCustomerByIdAsync(customerId, options, requestOptions, ct);
                return StatusCode(StatusCodes.Status200OK, customer);
            }
            catch (StripeException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
        }


        /// <summary>
        /// Retrieves a PaymentIntent object from the Stripe API.
        /// </summary>
        /// <param name="paymentIntentId">The ID of the PaymentIntent to retrieve.</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The retrieved PaymentIntent object.</returns>
        [HttpGet("paymentIntent/{paymentIntentId}")]
        public async Task<ActionResult<PaymentIntent>> GetPaymentIntent(string paymentIntentId, CancellationToken ct)
        {
            return await _stripeService.GetPaymentIntentAsync(paymentIntentId, ct); ;
        }
    }
}

