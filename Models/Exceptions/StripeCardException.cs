namespace Models.Exceptions
{
    public class StripeCardException : Exception
    {
        public StripeCardException(string message) : base(message)
        {
        }

        public StripeCardException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
