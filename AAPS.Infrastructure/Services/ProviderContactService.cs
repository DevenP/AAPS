using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services;

public class ProviderContactService : IProviderContactService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ProviderContactService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Application.Common.Paging.PagedResult<ProviderContactDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var query = db.Provider_Contacts.AsNoTracking().Select(ToDTO);

        return await query.ToPagedResultAsync(request, ct);
    }

    public async Task<ProviderContactDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.Provider_Contacts
            .AsNoTracking()
            .Where(c => c.Contact_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<ProviderContactDTO>> GetByProviderIdAsync(int providerId, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.Provider_Contacts
            .AsNoTracking()
            .Where(x => x.Provider_Id == providerId)
            .OrderByDescending(x => x.ContactDate) // Newest logs first
            .Select(x => new ProviderContactDTO
            {
                Id = x.Contact_Id,
                ProviderId = x.Provider_Id,
                ContactDate = x.ContactDate,
                Notes = x.ContactNote
            })
            .ToListAsync(ct);
    }

    public async Task<int> CreateAsync(ProviderContactDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = new Provider_Contact { Provider_Id = dto.ProviderId, ContactDate = dto.ContactDate, ContactNote = dto.Notes };
        db.Provider_Contacts.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Contact_Id;
    }

    public async Task UpdateAsync(int id, ProviderContactDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.Provider_Contacts.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();
        entity.Provider_Id = dto.ProviderId;
        entity.ContactDate = dto.ContactDate;
        entity.ContactNote = dto.Notes;
        await db.SaveChangesAsync(ct);
    }


    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.Provider_Contacts.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            db.Provider_Contacts.Remove(entity);
            await db.SaveChangesAsync(ct);
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
