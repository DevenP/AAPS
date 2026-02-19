using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface ILanguageService
{
    Task<PagedResult<LanguageDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

    Task<LanguageDTO?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(LanguageDTO dto, CancellationToken ct = default);

    Task UpdateAsync(int id, LanguageDTO dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
