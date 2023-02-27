namespace Models.Exceptions
{
    public class StripeInvalidRequestException : Exception
    {
        public StripeInvalidRequestException(string message) : base(message)
        {
        }

        public StripeInvalidRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
