using AAPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AAPS.Application.Abstractions.Data;

public interface IAppDbContext
{
    DbSet<BillingRate> BillingRates { get; }
    DbSet<Eval> Evals { get; }
    DbSet<GDistrict> GDistricts { get; }
    DbSet<ImportLog> ImportLogs { get; }
    DbSet<Language> Languages { get; }
    DbSet<Mandate> Mandates { get; }
    DbSet<Provider> Providers { get; }
    DbSet<ProviderRate> ProviderRates { get; }
    DbSet<Provider_Contact> Provider_Contacts { get; }
    DbSet<ServiceType> ServiceTypes { get; }
    DbSet<Sesi> Seses { get; }
    DbSet<VendorPortal> VendorPortals { get; }

    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
