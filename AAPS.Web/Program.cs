using AAPS.Application.Abstractions.Data;
using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Infrastructure;
using AAPS.Infrastructure.Data;
using AAPS.Infrastructure.Data.Scaffolded;
using AAPS.Web.Components;
using AAPS.Web.Middleware;
using AAPS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Extensions;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace AAPS.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("Default")!;
            var logFilePath = builder.Configuration.GetValue<string>("Logging:FilePath", "C:\\AAPS\\Logs\\aaps-.log")!;
            var retainedFileDays = builder.Configuration.GetValue<int>("Logging:RetainedFileDays", 30);
            var seqUrl = builder.Configuration.GetValue<string>("Logging:SeqUrl", "http://localhost:5341")!;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: retainedFileDays,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.MSSqlServer(
                    connectionString: connectionString,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "Logs",
                        AutoCreateSqlTable = true
                    },
                    restrictedToMinimumLevel: LogEventLevel.Warning)
                .WriteTo.Seq(
                    serverUrl: seqUrl,
                    restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();

            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents(options =>
                {
                    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(30);
                });

            RegisterDependencies(builder);

            var app = builder.Build();

            app.UseExceptionHandler("/error");

            if (!app.Environment.IsDevelopment())
                app.UseHsts();

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            CreateFileExplorerEndpoints(app);
            CreateReportEndpoints(app);       // ← NEW

            app.Run();
        }
        private static void RegisterDependencies(WebApplicationBuilder builder)
        {
            // 1. Get Connection String
            var connectionString = builder.Configuration.GetConnectionString("Default");
            var maxRetryCount = builder.Configuration.GetValue<int>("Database:MaxRetryCount", 3);
            var maxRetryDelay = builder.Configuration.GetValue<int>("Database:MaxRetryDelaySeconds", 5);

            // 2. Register DbContextFactory (Crucial for Blazor)
            builder.Services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlServer(connectionString, sql =>
                    sql.EnableRetryOnFailure(
                        maxRetryCount: maxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelay),
                        errorNumbersToAdd: null)));

            // Services
            builder.Services.AddInfrastructureServices(builder.Configuration);

            // Error Handling
            builder.Services.AddScoped<IErrorService, Infrastructure.Services.ErrorService>();

            // Packages
            builder.Services.AddMudServices();

            // Background services
            builder.Services.AddHostedService<LogCleanupService>();

        }

        private static void CreateFileExplorerEndpoints(WebApplication app)
        {
            // Download endpoint — streams the file as an attachment
            app.MapGet("/files/download", (string path, IFileExplorerService fileService) =>
            {
                if (!fileService.IsPathSafe(path))
                    return Results.Forbid();

                var absolute = fileService.GetAbsolutePath(path);
                if (!File.Exists(absolute))
                    return Results.NotFound();

                var fileName = System.IO.Path.GetFileName(absolute);
                var contentType = GetContentType(fileName);
                var stream = new FileStream(absolute, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                return Results.File(stream, contentType, fileName);
            });

            // Preview endpoint — serves the file inline (for iframe/img tags)
            app.MapGet("/files/preview", (string path, IFileExplorerService fileService) =>
            {
                if (!fileService.IsPathSafe(path))
                    return Results.Forbid();

                var absolute = fileService.GetAbsolutePath(path);
                if (!File.Exists(absolute))
                    return Results.NotFound();

                var fileName = System.IO.Path.GetFileName(absolute);
                var contentType = GetContentType(fileName);
                var stream = new FileStream(absolute, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                // Return inline so browser renders it instead of downloading
                return Results.File(stream, contentType, enableRangeProcessing: true);
            });

            // Provider files download — uses the ProviderFiles keyed service (C:\AAPS\Provider Profiles)
            app.MapGet("/provider-files/download", (string path, [FromKeyedServices("ProviderFiles")] IFileExplorerService fileService) =>
            {
                if (!fileService.IsPathSafe(path))
                    return Results.Forbid();

                var absolute = fileService.GetAbsolutePath(path);
                if (!File.Exists(absolute))
                    return Results.NotFound();

                var fileName = System.IO.Path.GetFileName(absolute);
                var contentType = GetContentType(fileName);
                var stream = new FileStream(absolute, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                return Results.File(stream, contentType, fileName);
            });

            // Provider files preview — serves inline for iframe/img/new window
            app.MapGet("/provider-files/preview", (string path, [FromKeyedServices("ProviderFiles")] IFileExplorerService fileService) =>
            {
                if (!fileService.IsPathSafe(path))
                    return Results.Forbid();

                var absolute = fileService.GetAbsolutePath(path);
                if (!File.Exists(absolute))
                    return Results.NotFound();

                var fileName = System.IO.Path.GetFileName(absolute);
                var contentType = GetContentType(fileName);
                var stream = new FileStream(absolute, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                return Results.File(stream, contentType, enableRangeProcessing: true);
            });

            // Eval files download — uses the EvalFiles keyed service (C:\AAPS\Evaluation Documents)
            app.MapGet("/eval-files/download", (string path, [FromKeyedServices("EvalFiles")] IFileExplorerService fileService) =>
            {
                if (!fileService.IsPathSafe(path))
                    return Results.Forbid();

                var absolute = fileService.GetAbsolutePath(path);
                if (!File.Exists(absolute))
                    return Results.NotFound();

                var fileName = System.IO.Path.GetFileName(absolute);
                var contentType = GetContentType(fileName);
                var stream = new FileStream(absolute, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                return Results.File(stream, contentType, fileName);
            });

            // Eval files preview — serves inline for iframe/img/new window
            app.MapGet("/eval-files/preview", (string path, [FromKeyedServices("EvalFiles")] IFileExplorerService fileService) =>
            {
                if (!fileService.IsPathSafe(path))
                    return Results.Forbid();

                var absolute = fileService.GetAbsolutePath(path);
                if (!File.Exists(absolute))
                    return Results.NotFound();

                var fileName = System.IO.Path.GetFileName(absolute);
                var contentType = GetContentType(fileName);
                var stream = new FileStream(absolute, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                return Results.File(stream, contentType, enableRangeProcessing: true);
            });
        }

        // ── NEW: Consent report endpoint ──────────────────────────────────────────
        private static void CreateReportEndpoints(WebApplication app)
        {
            // GET /reports/consent/{id}
            // Returns the consent letter PDF inline so the browser opens it in a new tab.
            app.MapGet("/reports/consent/{id:int}", async (
                int id,
                IEvalService evalService,
                IConsentReportService reportService,
                IWebHostEnvironment env,
                CancellationToken ct) =>
            {
                var eval = await evalService.GetByIdAsync(id, ct);
                if (eval is null)
                    return Results.NotFound($"Evaluation {id} not found.");

                // Supply logo — wwwroot/images/doe-logo.png (replace with your real file)
                var logoPath = Path.Combine(env.WebRootPath, "images", "nyc-doe-logo.png");
                reportService.SetLogoPath(logoPath);

                var pdfBytes = reportService.GenerateConsentPdf(eval);

                // Inline (not attachment) so browser renders it in a new tab
                return Results.File(pdfBytes, "application/pdf");
            });
        }

        private static string GetContentType(string fileName)
        {
            return System.IO.Path.GetExtension(fileName).ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".txt" => "text/plain",
                ".md" => "text/plain",
                ".csv" => "text/csv",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }
    }
}