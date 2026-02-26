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
            var query = from vp in _db.VendorPortals.AsNoTracking()

                            // 1. Get the Provider Name via SSN (Simple Lookup)
                        join p in _db.Providers.AsNoTracking()
                          on vp.pSsn.Replace("-", "") equals p.Ssn.Replace("-", "") into pGroup
                        from p in pGroup.DefaultIfEmpty()

                            // 2. The "Intelligent" Match: 
                            // We look for a Mandate that matches the Student, Duration, and StartDate.
                            // We use .FirstOrDefault() so we don't multiply rows!
                        let mandateMatch = _db.Mandates.AsNoTracking()
                            .Where(m => m.Student_ID.Trim() == vp.Student_ID.Trim())
                            .Where(m => m.Dur.StartsWith(vp.pDur))
                            .Where(m => m.MandateStart == vp.pStartDate)
                            .FirstOrDefault()


                        select new VendorPortalDTO
                        {
                            Id = vp.VendorPortal_Id,
                            ProviderSSN = vp.pSsn,
                            Boro = vp.pBoro,
                            District = vp.pDist,
                            School = vp.pSchool,
                            Fund = vp.pFund,
                            StudentId = vp.Student_ID,
                            Duration = vp.pDur,
                            Frequency = vp.pFreq,
                            GroupSize = vp.pGrpSize,
                            ApprovalStartDate = vp.pStartDate,
                            AssignmentId = vp.Assign_Id,
                            VenderPortalFile = vp.VPFile,
                            EntryId = vp.Entry_Id,

                           
                            // Provider Names
                            ProviderFirstName = p != null ? p.FirstName : "Unknown",
                            ProviderLastName = p != null ? p.LastName : "Unknown",

                            // Student Names (Only if the fuzzy match is successful)
                            StudentFirstName = mandateMatch != null ? mandateMatch.First_Name : "Unlinked",
                            StudentLastName = mandateMatch != null ? mandateMatch.Last_Name : "Unlinked",

                            // Mismatch Logic
                            Mismatch = mandateMatch == null ? "Approval" : (p == null ? "Provider" : null)
                        };

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
                pSsn = dto.ProviderSSN,
                pBoro = dto.Boro,
                pDist = dto.District,
                pSchool = dto.School,
                pFund = dto.Fund,
                Student_ID = dto.StudentId,
                pDur = dto.Duration,
                pFreq = dto.Frequency,
                pGrpSize = dto.GroupSize,
                pStartDate = dto.ApprovalStartDate,
                Assign_Id = dto.AssignmentId,
                VPFile = dto.VenderPortalFile,
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

            entity.pSsn = dto.ProviderSSN; entity.pBoro = dto.Boro; entity.pDist = dto.District;
            entity.pSchool = dto.School; entity.pFund = dto.Fund; entity.Student_ID = dto.StudentId;
            entity.pDur = dto.Duration; entity.pFreq = dto.Frequency; entity.pGrpSize = dto.GroupSize;
            entity.pStartDate = dto.ApprovalStartDate; entity.Assign_Id = dto.AssignmentId;
            entity.VPFile = dto.VenderPortalFile; entity.Entry_Id = dto.EntryId;

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
            ProviderSSN = v.pSsn,
            Boro = v.pBoro,
            District = v.pDist,
            School = v.pSchool,
            Fund = v.pFund,
            StudentId = v.Student_ID,
            Duration = v.pDur,
            Frequency = v.pFreq,
            GroupSize = v.pGrpSize,
            ApprovalStartDate = v.pStartDate,
            AssignmentId = v.Assign_Id,
            VenderPortalFile = v.VPFile,
            EntryId = v.Entry_Id
        };
    }
}
