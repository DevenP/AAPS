using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services;

public class ProviderService : IProviderService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly ILogger<ProviderService> _logger;

    public ProviderService(IDbContextFactory<AppDbContext> factory, ILogger<ProviderService> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<PagedResult<ProviderDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(request.SortBy))
            request = request with { SortBy = "LastName" };

        await using var db = _factory.CreateDbContext();
        // 1. Apply global search on the raw entity BEFORE any joins/projections
        // so EF translates it against real indexed columns.
        var baseQuery = db.Providers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            baseQuery = baseQuery.Where(p =>
                (p.LastName != null && p.LastName.Contains(term)) ||
                (p.FirstName != null && p.FirstName.Contains(term)) ||
                (p.Email != null && p.Email.Contains(term)) ||
                (p.Phone != null && p.Phone.Contains(term)) ||
                (p.TaxId != null && p.TaxId.Contains(term)) ||
                (p.NpiNumber != null && p.NpiNumber.Contains(term)) ||
                (p.License1 != null && p.License1.Contains(term)) ||
                (p.License2 != null && p.License2.Contains(term)) ||
                (p.CorpName != null && p.CorpName.Contains(term)) ||
                (p.ServiceType != null && p.ServiceType.Contains(term)) ||
                (p.Address != null && p.Address.Contains(term)) ||
                (p.City != null && p.City.Contains(term)) ||
                (p.State != null && p.State.Contains(term)) ||
                (p.Zipcode != null && p.Zipcode.Contains(term)) ||
                (p.Langs != null && p.Langs.Contains(term)) ||
                (p.DDInfo != null && p.DDInfo.Contains(term)));
            // Note: Ssn intentionally excluded - it is masked in the DTO and
            // has its own dedicated column-filter path that searches the real column.
        }

        // 2. Pre-compute duplicate names as a set to avoid N+1 per-row subquery.
        // Pull just Id+name, group in memory - EF can't translate GroupBy+SelectMany together.
        var allProviderNames = await db.Providers
            .AsNoTracking()
            .Select(p => new { p.Provider_Id, p.LastName, p.FirstName })
            .ToListAsync(ct);

        var duplicateIds = allProviderNames
            .GroupBy(p => new { p.LastName, p.FirstName })
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.Select(p => p.Provider_Id))
            .ToHashSet();

        // 3. Subquery for Assigned Student Counts from Sesis
        var assignedCounts = db.Seses
            .Where(s => s.Provider_Id != null && s.Entry_Id != null)
            .GroupBy(s => s.Provider_Id)
            .Select(g => new
            {
                ProviderId = g.Key,
                Count = g.Select(x => x.Entry_Id).Distinct().Count()
            })
            .AsNoTracking();

        // 4. Main Query with Left Join for Assigned Counts
        var query = from p in baseQuery
                    join a in assignedCounts on p.Provider_Id equals a.ProviderId into aGroup
                    from a in aGroup.DefaultIfEmpty()
                    select new ProviderDTO
                    {
                        Id = p.Provider_Id,
                        // Security: Server-side SSN Masking
                        Ssn = (p.Ssn != null && p.Ssn.Length >= 4)
                              ? "***-**-" + p.Ssn.Substring(p.Ssn.Length - 4)
                              : (p.Ssn ?? "N/A"),
                        LastName = p.LastName,
                        FirstName = p.FirstName,
                        Phone = p.Phone,
                        Email = p.Email,
                        TaxId = p.TaxId,
                        Birthdate = p.Birthdate,
                        NpiNumber = p.NpiNumber,
                        LiabilityInsuranceDate = p.Liability,
                        License1 = p.License1,
                        License1Expiration = p.License1Exp,
                        License2 = p.License2,
                        License2Expiration = p.License2Exp,
                        MedicalDate = p.Medical,
                        HasPets = p.Pets ?? false,
                        W9Date = p.W9,
                        DirectDepositDate = p.DirectDeposit,
                        ContractDate = p.Contract,
                        PhotoIdDate = p.PhotoId,
                        ResumeDate = p.Resume,
                        HrBundleDate = p.HrBundle,
                        ProofOfCorpDate = p.ProofCorp,
                        PoliciesDate = p.Policies,
                        MedicaidDate = p.Medicaid,
                        SexualHarassmentTrainingDate = p.SexualHarassment,
                        CorporationName = p.CorpName,
                        ServiceType = p.ServiceType,
                        IsActive = p.Status == ProviderStatus.Active ? true : false,
                        Address = p.Address,
                        City = p.City,
                        State = p.State,
                        Zipcode = p.Zipcode,
                        BlExtDate = p.BlExt,
                        Languages = p.Langs,
                        DirectDepositInfo = p.DDInfo,

                        // IsDuplicateName is populated in-memory below after paging
                        IsDuplicateName = false,

                        // Null-safe Assigned Count
                        AssignedCount = (int?)(a.Count) ?? 0
                    };

        // 5. Special Case: If searching by SSN, search the REAL column before paging
        if (request.ColumnFilters?.ContainsKey("Ssn") == true)
        {
            var ssnValue = request.ColumnFilters["Ssn"];
            if (!string.IsNullOrWhiteSpace(ssnValue))
            {
                query = query.Where(dto => db.Providers
                    .Any(original => original.Provider_Id == dto.Id && original.Ssn != null && original.Ssn.EndsWith(ssnValue)));
            }
        }

        // 6. Page the query (search already applied above, so performSearch: false)
        var paged = await query.ToPagedResultAsync(request, ct, performSearch: false);

        // 7. Post-process: stamp IsDuplicateName from the pre-computed set (no extra DB round-trips)
        var stamped = paged.Items.Select(dto => dto with
        {
            IsDuplicateName = duplicateIds.Contains(dto.Id)
        }).ToList();

        return new PagedResult<ProviderDTO>(stamped, paged.Page, paged.PageSize, paged.TotalCount);
    }


    public async Task<ProviderDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.Providers
            .AsNoTracking()
            .Where(p => p.Provider_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(ProviderDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = new Provider { Status = ProviderStatus.Active };
        ApplyDto(entity, dto);
        entity.Ssn = dto.Ssn;

        // Duplicate SSN check (mirrors theProvider_Ssn proc: other providers with same SSN)
        if (!string.IsNullOrWhiteSpace(entity.Ssn))
        {
            var duplicate = await db.Providers.AnyAsync(p => p.Ssn == entity.Ssn, ct);
            if (duplicate)
                throw new InvalidOperationException("Another provider already has this SSN.");
        }

        db.Providers.Add(entity);

        await db.SaveChangesAsync(ct);

        return entity.Provider_Id;
    }

    public async Task<bool> UpdateAsync(ProviderDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        // 1. Fetch the existing entity from the DB
        var provider = await db.Providers
            .FirstOrDefaultAsync(p => p.Provider_Id == dto.Id, ct);

        if (provider == null) return false;

        // Duplicate SSN check - excludes this provider (mirrors theProvider_Ssn proc)
        if (!string.IsNullOrWhiteSpace(dto.Ssn))
        {
            var duplicate = await db.Providers.AnyAsync(p => p.Provider_Id != dto.Id && p.Ssn == dto.Ssn, ct);
            if (duplicate)
                throw new InvalidOperationException("Another provider already has this SSN.");
        }
        ApplyDto(provider, dto);
        provider.Ssn = dto.Ssn;
        provider.Status = (dto.IsActive ?? false) ? ProviderStatus.Active : ProviderStatus.Inactive;

        // 3. Save Changes
        return await db.SaveChangesAsync(ct) > 0;
    }

    public async Task<int> UpdateWithContactsAsync(ProviderDTO dto, List<ProviderContactDTO> contacts, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        // EnableRetryOnFailure requires all transactions to go through CreateExecutionStrategy
        // so EF can retry the entire unit if a transient failure occurs.
        var strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                bool isNew = dto.Id == 0;
                _logger.LogInformation("Saving provider {Id} ({FirstName} {LastName}), isNew={IsNew}, contacts={ContactCount}",
                    dto.Id, dto.FirstName, dto.LastName, isNew, contacts.Count);

                // 1. Update/Add the Provider
                int providerId;
                if (dto.Id == 0)
                {
                    var entity = new Provider { Status = ProviderStatus.Active };
                    ApplyDto(entity, dto);
                    entity.Ssn = dto.Ssn;
                    db.Providers.Add(entity);
                    await db.SaveChangesAsync();
                    providerId = entity.Provider_Id;
                }
                else
                {
                    providerId = dto.Id;
                    var provider = await db.Providers.FirstOrDefaultAsync(p => p.Provider_Id == dto.Id);
                    if (provider != null)
                    {
                        ApplyDto(provider, dto);
                        provider.Ssn = dto.Ssn;
                        provider.Status = (dto.IsActive ?? false) ? ProviderStatus.Active : ProviderStatus.Inactive;
                    }
                }

                // 2. Fetch the current state from DB
                var dbContacts = await db.Provider_Contacts
                    .Where(c => c.Provider_Id == providerId)
                    .ToListAsync();

                // --- THE DELTA LOGIC ---

                // A. DELETE: Find IDs in DB that are NOT in the incoming list
                var incomingIds = contacts.Select(c => c.Id).ToList();
                var toDelete = dbContacts.Where(c => !incomingIds.Contains(c.Contact_Id));
                db.Provider_Contacts.RemoveRange(toDelete);

                // B. ADD / UPDATE: Loop through incoming
                foreach (var c in contacts)
                {
                    if (string.IsNullOrWhiteSpace(c.Notes)) continue;

                    if (c.Id == 0)
                    {
                        // It's a brand NEW row from the UI
                        db.Provider_Contacts.Add(new Provider_Contact
                        {
                            Provider_Id = providerId,
                            ContactNote = c.Notes,
                            ContactDate = DateTime.Now
                        });
                    }
                    else
                    {
                        // It's an EXISTING row - find it and update it
                        var existing = dbContacts.FirstOrDefault(x => x.Contact_Id == c.Id);
                        if (existing != null)
                        {
                            existing.ContactNote = c.Notes;
                            // We DON'T change the date, keeping the original timestamp!
                        }
                    }
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Provider {Id} ({FirstName} {LastName}) saved successfully with {ContactCount} contact(s)",
                    providerId, dto.FirstName, dto.LastName, contacts.Count);

                return providerId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateWithContactsAsync failed for provider {ProviderId}", dto.Id);
                await transaction.RollbackAsync();
                return 0;
            }
        });
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        // 1. Fetch the provider
        var provider = await db.Providers.FindAsync(new object[] { id }, ct);
        if (provider == null) return false;

        // 2. CHECK BUSINESS RULES (The "Guard")
        var hasActiveMandates = ProviderHasAssignments(db, provider.Provider_Id);
        if (hasActiveMandates)
        {
            _logger.LogWarning("Delete blocked for provider {Id} — has active assignments", id);
            throw new InvalidOperationException("Cannot delete: This provider is still assigned to active mandates.");
        }

        // 3. WIPE ASSOCIATED DATA (Contacts & Rates)
        var contacts = db.Provider_Contacts.Where(c => c.Provider_Id == id);
        db.Provider_Contacts.RemoveRange(contacts);

        var rates = db.ProviderRates.Where(r => r.Provider_Id == id);
        db.ProviderRates.RemoveRange(rates);

        // 4. MARK PROVIDER FOR DELETION
        db.Providers.Remove(provider);

        // 5. ATOMIC SAVE: SQL deletes everything in one shot
        var deleted = await db.SaveChangesAsync(ct) > 0;
        if (deleted)
            _logger.LogInformation("Provider {Id} deleted", id);
        return deleted;
    }

    private static string? StripPhone(string? phone) =>
        phone?.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");

    private static void ApplyDto(Provider p, ProviderDTO dto)
    {
        p.FirstName = dto.FirstName;
        p.LastName = dto.LastName;
        p.Phone = StripPhone(dto.Phone);
        p.Email = dto.Email;
        p.TaxId = dto.TaxId;
        p.NpiNumber = dto.NpiNumber;
        p.Birthdate = dto.Birthdate;
        p.CorpName = dto.CorporationName;
        p.ServiceType = dto.ServiceType;
        p.Address = dto.Address;
        p.City = dto.City;
        p.State = dto.State;
        p.Zipcode = dto.Zipcode;
        p.Pets = dto.HasPets;
        p.Liability = dto.LiabilityInsuranceDate;
        p.License1 = dto.License1;
        p.License1Exp = dto.License1Expiration;
        p.License2 = dto.License2;
        p.License2Exp = dto.License2Expiration;
        p.Medical = dto.MedicalDate;
        p.W9 = dto.W9Date;
        p.DirectDeposit = dto.DirectDepositDate;
        p.Contract = dto.ContractDate;
        p.PhotoId = dto.PhotoIdDate;
        p.Resume = dto.ResumeDate;
        p.HrBundle = dto.HrBundleDate;
        p.ProofCorp = dto.ProofOfCorpDate;
        p.Policies = dto.PoliciesDate;
        p.Medicaid = dto.MedicaidDate;
        p.SexualHarassment = dto.SexualHarassmentTrainingDate;
        p.BlExt = dto.BlExtDate;
        p.Langs = dto.Languages;
        p.DDInfo = dto.DirectDepositInfo;
    }

    private static readonly Expression<Func<Provider, ProviderDTO>> ToDTO = p => new ProviderDTO
    {
        Id = p.Provider_Id,
        // Raw SSN - only used by GetByIdAsync for the edit form. Grid uses masked version from GetPagedAsync.
        Ssn = p.Ssn,
        LastName = p.LastName,
        FirstName = p.FirstName,
        Phone = p.Phone,
        Email = p.Email,
        TaxId = p.TaxId,
        Birthdate = p.Birthdate,
        NpiNumber = p.NpiNumber,
        LiabilityInsuranceDate = p.Liability,
        License1 = p.License1,
        License1Expiration = p.License1Exp,
        License2 = p.License2,
        License2Expiration = p.License2Exp,
        MedicalDate = p.Medical,
        HasPets = p.Pets ?? false,
        W9Date = p.W9,
        DirectDepositDate = p.DirectDeposit,
        ContractDate = p.Contract,
        PhotoIdDate = p.PhotoId,
        ResumeDate = p.Resume,
        HrBundleDate = p.HrBundle,
        ProofOfCorpDate = p.ProofCorp,
        PoliciesDate = p.Policies,
        MedicaidDate = p.Medicaid,
        SexualHarassmentTrainingDate = p.SexualHarassment,
        CorporationName = p.CorpName,
        ServiceType = p.ServiceType,
        IsActive = p.Status == ProviderStatus.Active ? true : false,
        Address = p.Address,
        City = p.City,
        State = p.State,
        Zipcode = p.Zipcode,
        BlExtDate = p.BlExt,
        Languages = p.Langs,
        DirectDepositInfo = p.DDInfo
    };

    private static bool ProviderHasAssignments(AppDbContext db, int providerId)
    {
        var assignedCounts = db.Seses
        .Where(s => s.Provider_Id != null && s.Entry_Id != null)
        .GroupBy(s => s.Provider_Id)
        .Select(g => new
        {
            ProviderId = g.Key,
            Count = g.Select(x => x.Entry_Id).Distinct().Count()
        })
        .AsNoTracking();

        return assignedCounts.Any(a => a.ProviderId == providerId && a.Count > 0);
    }
}


public static class ProviderStatus
{
    public const string Active = "Active";

    public const string Inactive = "Inactive";
}
