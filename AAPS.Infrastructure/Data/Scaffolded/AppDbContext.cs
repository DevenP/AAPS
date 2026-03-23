using AAPS.Application.Abstractions.Data;
using AAPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Infrastructure.Data.Scaffolded;

public partial class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BillingRate> BillingRates { get; set; }

    public virtual DbSet<Eval> Evals { get; set; }

    public virtual DbSet<GDistrict> GDistricts { get; set; }

    public virtual DbSet<ImportLog> ImportLogs { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Mandate> Mandates { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Provider> Providers { get; set; }

    public virtual DbSet<ProviderRate> ProviderRates { get; set; }

    public virtual DbSet<Provider_Contact> Provider_Contacts { get; set; }

    public virtual DbSet<ServiceType> ServiceTypes { get; set; }

    public virtual DbSet<Sesi> Seses { get; set; }

    public virtual DbSet<VendorPortal> VendorPortals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Mandate>(entity =>
        {
            entity.Property(e => e.D75).IsFixedLength();
        });

        modelBuilder.Entity<Provider>(entity =>
        {
            entity.Property(e => e.Ssn).IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
