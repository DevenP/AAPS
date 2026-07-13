using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services
{
    // Raw result type matching the stored proc SELECT columns
    internal sealed class VendorPortalRaw
    {
        public int VendorPortal_Id { get; set; }
        public string? pSsn { get; set; }
        public string? pBoro { get; set; }
        public string? pDist { get; set; }
        public string? pSchool { get; set; }
        public string? pFund { get; set; }
        public string? Student_ID { get; set; }
        public string? pDur { get; set; }
        public string? pFreq { get; set; }
        public string? pGrpSize { get; set; }
        public DateTime? pStartDate { get; set; }
        public string? Assign_Id { get; set; }
        public string? VPFile { get; set; }
        public int? Entry_Id { get; set; }
        // Joined columns
        public string? Last_Name { get; set; }   // Students.Last_Name
        public string? First_Name { get; set; }  // Students.First_Name
        public string? LastName { get; set; }    // Providers.LastName
        public string? FirstName { get; set; }   // Providers.FirstName
        public string? Mismatch { get; set; }
    }

    public class VendorPortalService : IVendorPortalService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<VendorPortalService> _logger;

        public VendorPortalService(IDbContextFactory<AppDbContext> factory, ILogger<VendorPortalService> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        private static IEnumerable<VendorPortalDTO> ProjectRaw(IEnumerable<VendorPortalRaw> raw) =>
            raw.Select(r => new VendorPortalDTO
            {
                Id = r.VendorPortal_Id,
                ProviderSSN = r.pSsn,
                Boro = r.pBoro,
                District = r.pDist,
                School = r.pSchool,
                Fund = r.pFund,
                StudentId = r.Student_ID,
                Duration = r.pDur,
                Frequency = r.pFreq,
                GroupSize = r.pGrpSize,
                ApprovalStartDate = r.pStartDate,
                AssignmentId = r.Assign_Id,
                VenderPortalFile = r.VPFile,
                EntryId = r.Entry_Id,
                StudentFirstName = r.First_Name,
                StudentLastName = r.Last_Name,
                ProviderFirstName = r.FirstName,
                ProviderLastName = r.LastName,
                Mismatch = r.Mismatch,
                MismatchedVendorPortal = r.Entry_Id == null
            });

        private static async Task<List<VendorPortalRaw>> FetchRawAsync(AppDbContext db, CancellationToken ct)
        {
            try
            {
                return await db.Database
                    .SqlQueryRaw<VendorPortalRaw>(
                        "EXEC [dbo].[VendorPortal_Select] @searchBy=0, @searchByValue=NULL, @dateSearch=0, @from=NULL, @to=NULL, @unbound=0")
                    .ToListAsync(ct);
            }
            catch (OperationCanceledException) { return []; }
            catch (Microsoft.Data.SqlClient.SqlException ex)
                when (ct.IsCancellationRequested || ex.Message.Contains("Operation cancelled"))
            {
                return [];
            }
        }

        public async Task<PagedResult<VendorPortalDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
        {
            await using var db = _factory.CreateDbContext();
            var raw = await FetchRawAsync(db, ct);
            if (raw.Count == 0 && ct.IsCancellationRequested)
                return new PagedResult<VendorPortalDTO>([], request.Page, request.PageSize, 0);

            // Semester filter on the approval start date (proc results are already in memory).
            if (request.DateFrom.HasValue)
                raw = raw.Where(r => r.pStartDate.HasValue && r.pStartDate.Value >= request.DateFrom.Value).ToList();
            if (request.DateTo.HasValue)
                raw = raw.Where(r => r.pStartDate.HasValue && r.pStartDate.Value <= request.DateTo.Value).ToList();

            return await ProjectRaw(raw).ToPagedResultAsync(request, ct);
        }

        public async Task<List<int>> GetIdsAsync(PagedRequest request, CancellationToken ct = default)
        {
            await using var db = _factory.CreateDbContext();
            var raw = await FetchRawAsync(db, ct);

            if (request.DateFrom.HasValue)
                raw = raw.Where(r => r.pStartDate.HasValue && r.pStartDate.Value >= request.DateFrom.Value).ToList();
            if (request.DateTo.HasValue)
                raw = raw.Where(r => r.pStartDate.HasValue && r.pStartDate.Value <= request.DateTo.Value).ToList();

            var allRequest = new PagedRequest(request.Search, request.ColumnFilters, PageSize: -1);
            var result = await ProjectRaw(raw).ToPagedResultAsync(allRequest, ct);
            return result.Items.Select(i => i.Id).ToList();
        }

        public async Task<VendorPortalDTO?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            await using var db = _factory.CreateDbContext();
            return await db.VendorPortals
                .AsNoTracking()
                .Where(v => v.VendorPortal_Id == id)
                .Select(ToDTO)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<int> CreateAsync(VendorPortalDTO dto, CancellationToken ct = default)
        {
            await using var db = _factory.CreateDbContext();
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
            _logger.LogInformation("Creating vendor portal record for assign {AssignId}", dto.AssignmentId);

            db.VendorPortals.Add(entity);
            await db.SaveChangesAsync(ct);

            _logger.LogInformation("Vendor portal record {Id} created for assign {AssignId}", entity.VendorPortal_Id, dto.AssignmentId);

            return entity.VendorPortal_Id;
        }

        public async Task UpdateAsync(int id, VendorPortalDTO dto, CancellationToken ct = default)
        {
            await using var db = _factory.CreateDbContext();
            var entity = await db.VendorPortals.FindAsync(new object[] { id }, ct)
                ?? throw new KeyNotFoundException();

            _logger.LogInformation("Updating vendor portal record {Id} (assign {AssignId})", id, entity.Assign_Id);

            entity.pSsn = dto.ProviderSSN;
            entity.pBoro = dto.Boro;
            entity.pDist = dto.District;
            entity.pSchool = dto.School;
            entity.pFund = dto.Fund;
            entity.Student_ID = dto.StudentId;
            entity.pDur = dto.Duration;
            entity.pFreq = dto.Frequency;
            entity.pGrpSize = dto.GroupSize;
            entity.pStartDate = dto.ApprovalStartDate;
            entity.Assign_Id = dto.AssignmentId;
            entity.VPFile = dto.VenderPortalFile;
            entity.Entry_Id = dto.EntryId;

            await db.SaveChangesAsync(ct);

            _logger.LogInformation("Vendor portal record {Id} updated", id);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            await using var db = _factory.CreateDbContext();
            var entity = await db.VendorPortals.FindAsync(new object[] { id }, ct);
            if (entity != null)
            {
                _logger.LogInformation("Deleting vendor portal record {Id} (assign {AssignId})", id, entity.Assign_Id);
                db.VendorPortals.Remove(entity);
                await db.SaveChangesAsync(ct);
                _logger.LogInformation("Vendor portal record {Id} deleted", id);
            }
        }

        public async Task DeleteManyAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            await using var db = _factory.CreateDbContext();
            var idList = ids.ToList();
            var entities = await db.VendorPortals
                .Where(v => idList.Contains(v.VendorPortal_Id))
                .ToListAsync(ct);

            _logger.LogInformation("Deleting {Count} vendor portal records", entities.Count);
            db.VendorPortals.RemoveRange(entities);
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Deleted {Count} vendor portal records", entities.Count);
        }

        public async Task ReplaceEntryIdAsync(IEnumerable<int> ids, int newEntryId, CancellationToken ct = default)
        {
            await using var db = _factory.CreateDbContext();
            var idList = ids.ToList();
            var entities = await db.VendorPortals
                .Where(v => idList.Contains(v.VendorPortal_Id))
                .ToListAsync(ct);
            foreach (var entity in entities)
                entity.Entry_Id = newEntryId;
            await db.SaveChangesAsync(ct);

            _logger.LogInformation("Replaced Entry_Id with {NewEntryId} on {Count} vendor portal records", newEntryId, entities.Count);
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
