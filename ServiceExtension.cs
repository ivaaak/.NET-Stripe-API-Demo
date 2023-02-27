using Application;
using Microsoft.OpenApi.Models;
using Models;
using Models.Exceptions;
using Services;
using Stripe;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

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
				.AddScoped<PaymentIntentService>()
				.AddScoped<IStripeAppService, StripeAppService>();
		}

        
		public static IServiceCollection AddSwaggerWithSchemaOptions(this IServiceCollection services, IConfiguration configuration)
		{
			return services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Stripe API Demo", Version = "v1" });

                // Customize the Swagger schema
                c.SchemaFilter<ExcludeStripeTypesSchemaFilter>();
            });
        }


        // Define a custom schema filter to exclude types from the Stripe.NET library
        public class ExcludeStripeTypesSchemaFilter : ISchemaFilter
        {
            public void Apply(OpenApiSchema schema, SchemaFilterContext context)
            {
                var type = context.Type;

                // Check whether the type is defined in the Stripe.NET library
                if (type == typeof(AddStripeCard) 
                    || type == typeof(AddStripeCustomer) 
                    || type == typeof(AddStripePayment) 
                    || type == typeof(StripeCardException) 
                    || type == typeof(StripeInvalidRequestException) 
                    || type == typeof(StripeCustomer) 
                    || type == typeof(StripePayment))
                {
                    // Include the type in the Swagger schema
                }
                else
                {
                    // Exclude the type from the Swagger schema
                    schema.Example = null;
                    schema.Properties.Clear();
                }
            }
        }
        
    }
}

