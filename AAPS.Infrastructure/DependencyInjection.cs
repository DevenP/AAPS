using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Settings;
using AAPS.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AAPS.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ImportSettings>(options =>
                configuration.GetSection("Import").Bind(options));
            services.Configure<BillingSettings>(options =>
                configuration.GetSection("Billing").Bind(options));
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
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IBillingService, BillingService>();

            services.AddScoped<IVendorPortalService, VendorPortalService>();

            services.AddScoped<IFileExplorerService, FileExplorerService>();

            // Provider files — separate root path, keyed so ProviderForm injects it independently
            services.AddKeyedScoped<IFileExplorerService, FileExplorerService>(
                "ProviderFiles",
                (sp, _) => new FileExplorerService(
                    configuration["ProviderFiles:RootPath"]
                    ?? throw new InvalidOperationException("ProviderFiles:RootPath is not configured in appsettings.json")));

            // Eval files — C:\AAPS\Evaluation Documents\{evalId}\
            services.AddKeyedScoped<IFileExplorerService, FileExplorerService>(
                "EvalFiles",
                (sp, _) => new FileExplorerService(
                    configuration["EvalFiles:RootPath"]
                    ?? throw new InvalidOperationException("EvalFiles:RootPath is not configured in appsettings.json")));

            services.AddScoped<IImportService, ImportService>();
            services.AddScoped<IDashboardService, DashboardService>();

            // Reports — logo path resolved at registration time via IWebHostEnvironment
            services.AddScoped<IConsentReportService, ConsentReportService>();
            services.AddScoped<IStatementService, StatementService>();

            return services;
        }
    }
}