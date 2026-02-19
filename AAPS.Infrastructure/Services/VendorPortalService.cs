using AAPS.Application.Abstractions.Data;
using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services
{
    public class VendorPortalService : IVendorPortalService
    {

        private readonly IAppDbContext _db;
        public VendorPortalService(IAppDbContext db) 
        {
            _db = db;
        }

        public async Task<Application.Common.Paging.PagedResult<VendorPortalDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
        {
            var query = _db.VendorPortals.AsNoTracking().Select(ToDTO);

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

        public async Task<VendorPortalDTO?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.VendorPortals
                .AsNoTracking()
                .Where(v => v.VendorPortal_Id == id)
                .Select(ToDTO)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<int> CreateAsync(VendorPortalDTO dto, CancellationToken ct = default)
        {
            var entity = new VendorPortal
            {
                pSsn = dto.Ssn,
                pBoro = dto.Boro,
                pDist = dto.District,
                pSchool = dto.School,
                pFund = dto.Fund,
                Student_ID = dto.StudentId,
                pDur = dto.Duration,
                pFreq = dto.Frequency,
                pGrpSize = dto.GroupSize,
                pStartDate = dto.StartDate,
                Assign_Id = dto.AssignmentId,
                VPFile = dto.FileName,
                Entry_Id = dto.EntryId
            };
            _db.VendorPortals.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity.VendorPortal_Id;
        }

        public async Task UpdateAsync(int id, VendorPortalDTO dto, CancellationToken ct = default)
        {
            var entity = await _db.VendorPortals.FindAsync(new object[] { id }, ct)
                ?? throw new KeyNotFoundException();

            entity.pSsn = dto.Ssn; entity.pBoro = dto.Boro; entity.pDist = dto.District;
            entity.pSchool = dto.School; entity.pFund = dto.Fund; entity.Student_ID = dto.StudentId;
            entity.pDur = dto.Duration; entity.pFreq = dto.Frequency; entity.pGrpSize = dto.GroupSize;
            entity.pStartDate = dto.StartDate; entity.Assign_Id = dto.AssignmentId;
            entity.VPFile = dto.FileName; entity.Entry_Id = dto.EntryId;

            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.VendorPortals.FindAsync(new object[] { id }, ct);
            if (entity != null)
            {
                _db.VendorPortals.Remove(entity);
                await _db.SaveChangesAsync(ct);
            }
        }

        private static readonly Expression<Func<VendorPortal, VendorPortalDTO>> ToDTO = v => new VendorPortalDTO
        {
            Id = v.VendorPortal_Id,
            Ssn = v.pSsn,
            Boro = v.pBoro,
            District = v.pDist,
            School = v.pSchool,
            Fund = v.pFund,
            StudentId = v.Student_ID,
            Duration = v.pDur,
            Frequency = v.pFreq,
            GroupSize = v.pGrpSize,
            StartDate = v.pStartDate,
            AssignmentId = v.Assign_Id,
            FileName = v.VPFile,
            EntryId = v.Entry_Id
        };
    }
}
