using Customs_Management_System.IRepository;
using Customs_Management_System.Repository;
using Microsoft.EntityFrameworkCore.Internal;

namespace Customs_Management_System.DependencyContainer
{
    public class DependencyInversion
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddTransient<ICustomsRepository, CustomsRepository>();
            services.AddTransient<IPaymentService,PaymentServiceRepository>();
        }
    }
}
