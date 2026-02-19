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
        var query = _db.Providers.AsNoTracking().Select(ToDTO);

        if (request.ColumnFilters?.Any() == true)
        {
            foreach (var col in request.ColumnFilters)
            {
                if (string.IsNullOrWhiteSpace(col.Value)) continue;
                query = query.Where($"{col.Key}.Contains(@0)", col.Value);
            }
        }

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

    public async Task<int> CreateAsync(ProviderDTO dto, CancellationToken ct = default)
    {
        var entity = new Provider
        {
            Ssn = dto.Ssn,
            LastName = dto.LastName,
            FirstName = dto.FirstName,
            Phone = dto.Phone,
            Email = dto.Email,
            TaxId = dto.TaxId,
            Status = dto.Status,
            ServiceType = dto.ServiceType,
            Langs = dto.Languages
        };
        _db.Providers.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Provider_Id;
    }

    public async Task UpdateAsync(int id, ProviderDTO dto, CancellationToken ct = default)
    {
        var entity = await _db.Providers.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException();

        entity.LastName = dto.LastName;
        entity.FirstName = dto.FirstName;
        entity.Status = dto.Status;
        entity.Email = dto.Email;
        // ... update other fields

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Providers.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _db.Providers.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    private static readonly Expression<Func<Provider, ProviderDTO>> ToDTO = p => new ProviderDTO
    {
        Id = p.Provider_Id,
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
        Status = p.Status,
        Address = p.Address,
        City = p.City,
        State = p.State,
        Zipcode = p.Zipcode,
        BlExtDate = p.BlExt,
        Languages = p.Langs,
        DirectDepositInfo = p.DDInfo
    };
}
