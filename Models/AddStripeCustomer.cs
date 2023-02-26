namespace Models
{
    public record AddStripeCustomer(
        string Email,
        string Name,
        AddStripeCard CreditCard
    );
}

