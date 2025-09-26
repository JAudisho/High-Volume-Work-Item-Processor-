namespace AtlasQueue.Domain.DTO;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long Total);