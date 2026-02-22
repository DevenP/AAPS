using AAPS.Application.Abstractions.Data;
using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Infrastructure;
using AAPS.Infrastructure.Data;
using AAPS.Infrastructure.Data.Scaffolded;
using AAPS.Web.Components;
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
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
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

            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Services
            builder.Services.AddInfrastructureServices();

            // Packages
            builder.Services.AddMudServices();

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
