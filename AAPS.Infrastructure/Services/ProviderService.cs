using AAPS.Application.Abstractions.Data;
using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services;

public class ProviderService : IProviderService
{
    private readonly IAppDbContext _db;

    public ProviderService(IAppDbContext db) => _db = db;

    public async Task<Application.Common.Paging.PagedResult<ProviderDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        // 1. Subquery for Assigned Student Counts from Sesis
        var assignedCounts = _db.Seses
            .Where(s => s.Provider_Id != null && s.Entry_Id != null)
            .GroupBy(s => s.Provider_Id)
            .Select(g => new {
                ProviderId = g.Key,
                Count = g.Select(x => x.Entry_Id).Distinct().Count()
            })
            .AsNoTracking();

        // 2. Main Query with Left Join for Assigned Counts
        var query = from p in _db.Providers.AsNoTracking()
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

                        // Duplicate Check: Check if any OTHER provider has the same name
                        IsDuplicateName = _db.Providers.Any(other =>
                            other.Provider_Id != p.Provider_Id &&
                            other.LastName == p.LastName &&
                            other.FirstName == p.FirstName),

                        // Null-safe Assigned Count
                        AssignedCount = (int?)(a.Count) ?? 0
                    };

        // 3. Special Case: If searching by SSN, search the REAL column before paging
        if (request.ColumnFilters?.ContainsKey("Ssn") == true)
        {
            var ssnValue = request.ColumnFilters["Ssn"];
            if (!string.IsNullOrWhiteSpace(ssnValue))
            {
                // We filter the DTO query by looking back at the original Ssn column
                query = query.Where(dto => _db.Providers
                    .Any(original => original.Provider_Id == dto.Id && original.Ssn.EndsWith(ssnValue)));
            }
        }

        // 4. Return with Global Search, Sorting, and Paging
        return await query.ToPagedResultAsync(request, ct);
    }


    public async Task<ProviderDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Providers
            .AsNoTracking()
            .Where(p => p.Provider_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> AddAsync(ProviderDTO dto, CancellationToken ct = default)
    {
        var entity = new Provider
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            // Clean masks before saving to DB
            Ssn = dto.Ssn?.Replace("-", ""),
            Phone = dto.Phone?.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", ""),
            Email = dto.Email,
            Status = ProviderStatus.Active,
            //IsActive = dto.IsActive ?? true, // Default to true for new providers
            TaxId = dto.TaxId,
            NpiNumber = dto.NpiNumber,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Zipcode = dto.Zipcode,
            Pets = dto.HasPets
        };

        _db.Providers.Add(entity);

        await _db.SaveChangesAsync(ct);

        return entity.Provider_Id;
    }

    public async Task<bool> UpdateAsync(ProviderDTO dto, CancellationToken ct = default)
    {
        // 1. Fetch the existing entity from the DB
        var provider = await _db.Providers
            .FirstOrDefaultAsync(p => p.Provider_Id == dto.Id, ct);

        if (provider == null) return false;

        // 2. Map the DTO values back to the Entity
        // (In a bigger app, use AutoMapper, but manual is safer for now)
        provider.FirstName = dto.FirstName;
        provider.LastName = dto.LastName;
        provider.Ssn = dto.Ssn?.Replace("-", ""); // Strip mask before saving
        provider.Email = dto.Email;
        provider.Phone = dto.Phone?.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");
        provider.Status = dto.IsActive.Value ? ProviderStatus.Active : ProviderStatus.Inactive;
        provider.License1Exp = dto.License1Expiration;
        provider.TaxId = dto.TaxId;
        provider.Address = dto.Address;
        provider.City = dto.City;
        provider.State = dto.State;
        provider.Zipcode = dto.Zipcode;
        provider.Pets = dto.HasPets;

        // 3. Save Changes
        return await _db.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> UpdateWithContactsAsync(ProviderDTO dto, List<ProviderContactDTO> contacts)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // 1. Update/Add the Provider
            int providerId = dto.Id == 0 ? await AddAsync(dto) : dto.Id;
            if (dto.Id > 0) await UpdateAsync(dto);

            // 2. Fetch the current state from DB
            var dbContacts = await _db.Provider_Contacts
                .Where(c => c.Provider_Id == providerId)
                .ToListAsync();

            // --- THE DELTA LOGIC ---

            // A. DELETE: Find IDs in DB that are NOT in the incoming list
            var incomingIds = contacts.Select(c => c.Id).ToList();
            var toDelete = dbContacts.Where(c => !incomingIds.Contains(c.Contact_Id));
            _db.Provider_Contacts.RemoveRange(toDelete);

            // B. ADD / UPDATE: Loop through incoming
            foreach (var c in contacts)
            {
                if (string.IsNullOrWhiteSpace(c.Notes)) continue;

                if (c.Id == 0)
                {
                    // It's a brand NEW row from the UI
                    _db.Provider_Contacts.Add(new Provider_Contact
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

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        // 1. Fetch the provider
        var provider = await _db.Providers.FindAsync(new object[] { id }, ct);
        if (provider == null) return false;

        // 2. CHECK BUSINESS RULES (The "Guard")
        var hasActiveMandates = ProviderHasAssignments(provider.Provider_Id);
        if (hasActiveMandates)
        {
            throw new InvalidOperationException("Cannot delete: This provider is still assigned to active mandates.");
        }

        // 3. WIPE ASSOCIATED DATA (Contacts & Rates)
        var contacts = _db.Provider_Contacts.Where(c => c.Provider_Id == id);
        _db.Provider_Contacts.RemoveRange(contacts);

        var rates = _db.ProviderRates.Where(r => r.Provider_Id == id);
        _db.ProviderRates.RemoveRange(rates);

        // 4. MARK PROVIDER FOR DELETION
        _db.Providers.Remove(provider);

        // 5. ATOMIC SAVE: SQL deletes everything in one shot
        return await _db.SaveChangesAsync(ct) > 0;
    }

    private static readonly Expression<Func<Provider, ProviderDTO>> ToDTO = p => new ProviderDTO
    {
        Id = p.Provider_Id,
        Ssn = p.Ssn,
        //Ssn = p.Ssn != null && p.Ssn.Length >= 4
        //  ? "***-**-" + p.Ssn.Substring(p.Ssn.Length - 4)
        //  : "N/A",
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

    private bool ProviderHasAssignments(int providerId)
    {
        var assignedCounts = _db.Seses
        .Where(s => s.Provider_Id != null && s.Entry_Id != null)
        .GroupBy(s => s.Provider_Id)
        .Select(g => new {
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
