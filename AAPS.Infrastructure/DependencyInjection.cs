using AAPS.Application.Abstractions.Services;
using AAPS.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AAPS.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IGDistrictService, GDistrictService>();
            services.AddScoped<IBillingRateService, BillingRateService>();
            services.AddScoped<IEvalService, EvalService>();
            services.AddScoped<IImportLogService, ImportLogService>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<IMandateService, MandateService>();
            services.AddScoped<IProviderService, ProviderService>();
            services.AddScoped<IProviderContactService, ProviderContactService>();
            services.AddScoped<IProviderRateService, ProviderRateService>();
            services.AddScoped<IServiceTypeService, ServiceTypeService>();
            services.AddScoped<ISesiService, SesiService>();

            services.AddScoped<IVendorPortalService, VendorPortalService>();

            return services;
        }
    }
}
