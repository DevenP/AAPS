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

public class ProviderContactService : IProviderContactService
{
    private readonly IAppDbContext _db;

    public ProviderContactService(IAppDbContext db) => _db = db;

    public async Task<Application.Common.Paging.PagedResult<ProviderContactDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _db.Provider_Contacts.AsNoTracking().Select(ToDTO);

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

    public async Task<ProviderContactDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Provider_Contacts
            .AsNoTracking()
            .Where(c => c.Contact_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(ProviderContactDTO dto, CancellationToken ct = default)
    {
        var entity = new Provider_Contact { Provider_Id = dto.ProviderId, ContactDate = dto.ContactDate, ContactNote = dto.Notes };
        _db.Provider_Contacts.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Contact_Id;
    }

    public async Task UpdateAsync(int id, ProviderContactDTO dto, CancellationToken ct = default)
    {
        var entity = await _db.Provider_Contacts.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();
        entity.Provider_Id = dto.ProviderId;
        entity.ContactDate = dto.ContactDate;
        entity.ContactNote = dto.Notes;
        await _db.SaveChangesAsync(ct);
    }


    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Provider_Contacts.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _db.Provider_Contacts.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    private static readonly Expression<Func<Provider_Contact, ProviderContactDTO>> ToDTO = c => new ProviderContactDTO
    {
        Id = c.Contact_Id,
        ProviderId = c.Provider_Id,
        ContactDate = c.ContactDate,
        Notes = c.ContactNote
    };
}
