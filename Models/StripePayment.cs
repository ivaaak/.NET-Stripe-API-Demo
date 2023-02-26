namespace Models
{
    public record StripePayment(
        string CustomerId,
        string ReceiptEmail,
        string Description,
        string Currency,
        long Amount,
        string PaymentId
    );
}

