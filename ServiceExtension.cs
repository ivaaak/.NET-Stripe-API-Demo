using Application;
using Services;
using Stripe;

namespace Stripe_Web_Api
{
    public static class ServiceExtension
    {
		public static IServiceCollection AddStripeInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			StripeConfiguration.ApiKey = configuration.GetValue<string>("StripeSettings:SecretKey");

			return services
				.AddScoped<CustomerService>()
				.AddScoped<ChargeService>()
				.AddScoped<TokenService>()
				.AddScoped<IStripeAppService, StripeAppService>();
		}
	}
}

