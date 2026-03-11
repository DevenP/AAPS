using AAPS.Application.Abstractions.Data;
using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Infrastructure;
using AAPS.Infrastructure.Data;
using AAPS.Infrastructure.Data.Scaffolded;
using AAPS.Web.Components;
using AAPS.Web.Middleware;
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

            CreateVendorPortalsDataEndpoints(app);

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
            builder.Services.AddInfrastructureServices();

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
                var stream = File.OpenRead(absolute);

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
                var stream = File.OpenRead(absolute);

                // Return inline so browser renders it instead of downloading
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
        private static void CreateVendorPortalsDataEndpoints(WebApplication app) 
        {
            app.MapGet("/vendorportals", async (
                IVendorPortalService service,
                int page = 1,
                int pageSize = 25,
                string? sortBy = null,
                string sortDir = "asc",
                string? search = null,
                CancellationToken ct = default) =>
            {
                // Explicitly map the values to the record properties by name
                var request = new PagedRequest(
                    Search: search,
                    SortBy: sortBy,
                    SortDir: sortDir,
                    Page: page,
                    PageSize: pageSize
                );

                var result = await service.GetPagedAsync(request, ct);

                return Results.Ok(result);
            });

            //app.MapGet("/vendorportals", async (
            //    IVendorPortalQueryService service,
            //    int page = 1,
            //    int pageSize = 25,
            //    string? sortBy = null,
            //    string sortDir = "asc",
            //    string? search = null,
            //    CancellationToken ct = default) =>
            //{
            //    var result = await service.GetAsync(
            //        new GetVendorPortalsRequest(page, pageSize, sortBy, sortDir, search),
            //        ct);

            //    return Results.Ok(result);
            //});

            //app.MapGet("/vendorportals/{id:int}", async (int id, IVendorPortalCrudService svc, CancellationToken ct) =>
            //{
            //    var result = await svc.GetByIdAsync(id, ct);
            //    return result.Status switch
            //    {
            //        Application.Common.Crud.CrudStatus.Success => Results.Ok(result.Data),
            //        Application.Common.Crud.CrudStatus.NotFound => Results.NotFound(result.Message),
            //        _ => Results.Problem(result.Message)
            //    };
            //});

            //app.MapPost("/vendorportals", async (Dictionary<string, object?> body, IVendorPortalCrudService svc, CancellationToken ct) =>
            //{
            //    var result = await svc.CreateAsync(body, ct);
            //    return result.Status switch
            //    {
            //        Application.Common.Crud.CrudStatus.Success => Results.Ok(new { id = result.Data }),
            //        _ => Results.Problem(result.Message)
            //    };
            //});

            //app.MapPut("/vendorportals/{id:int}", async (
            //    int id,
            //    Dictionary<string, object?> body,
            //    string? rowVersion, // pass as querystring: ?rowVersion=BASE64
            //    IVendorPortalCrudService svc,
            //    CancellationToken ct) =>
            //{
            //    var result = await svc.UpdateAsync(id, body, rowVersion, ct);
            //    return result.Status switch
            //    {
            //        Application.Common.Crud.CrudStatus.Success => Results.NoContent(),
            //        Application.Common.Crud.CrudStatus.NotFound => Results.NotFound(result.Message),
            //        Application.Common.Crud.CrudStatus.Conflict => Results.Conflict(new { message = result.Message }),
            //        _ => Results.Problem(result.Message)
            //    };
            //});

            //app.MapDelete("/vendorportals/{id:int}", async (
            //    int id,
            //    string? rowVersion,
            //    IVendorPortalCrudService svc,
            //    CancellationToken ct) =>
            //{
            //    var result = await svc.DeleteAsync(id, rowVersion, ct);
            //    return result.Status switch
            //    {
            //        Application.Common.Crud.CrudStatus.Success => Results.NoContent(),
            //        Application.Common.Crud.CrudStatus.NotFound => Results.NotFound(result.Message),
            //        Application.Common.Crud.CrudStatus.Conflict => Results.Conflict(new { message = result.Message }),
            //        _ => Results.Problem(result.Message)
            //    };
            //});
        }

    }
}
