using AAPS.Application.Abstractions.Data;
using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Infrastructure;
using AAPS.Infrastructure.Data;
using AAPS.Infrastructure.Data.Scaffolded;
using AAPS.Web.Components;
using AAPS.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Extensions;
using MudBlazor.Services;

namespace AAPS.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            RegisterDependencies(builder);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseMiddleware<GlobalExceptionHandler>();

            if (!app.Environment.IsDevelopment())
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            CreateFileExplorerEndpoints(app);

            app.Run();
        }
        private static void RegisterDependencies(WebApplicationBuilder builder)
        {
            // 1. Get Connection String
            var connectionString = builder.Configuration.GetConnectionString("Default");

            // 2. Register DbContextFactory (Crucial for Blazor)
            builder.Services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 3. Register the Interface to map to the Factory-created context
            builder.Services.AddScoped<IAppDbContext>(p =>
                p.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

            // Services
            builder.Services.AddInfrastructureServices(builder.Configuration);

            // Error Handling
            builder.Services.AddScoped<IErrorService, Infrastructure.Services.ErrorService>();

            // Packages
            builder.Services.AddMudServices();

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