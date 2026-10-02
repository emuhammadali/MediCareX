using Microsoft.EntityFrameworkCore;

namespace MediCareX.Application.Common.Models;

public sealed record PaginatedList<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var total = await source.CountAsync(ct);
        var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PaginatedList<T>(items, total, page, pageSize);
    }
}

public abstract record PagedQuery(int Page = 1, int PageSize = 20);
