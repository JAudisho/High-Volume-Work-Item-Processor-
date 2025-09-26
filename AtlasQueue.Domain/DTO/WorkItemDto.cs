using AtlasQueue.Domain.Enums;
using AtlasQueue.Domain.ValueObjects;

namespace AtlasQueue.Domain.DTO;

public record WorkItemDto(
    Guid Id,
    string ExternalRef,
    WorkItemStatus Status,
    Priority Priority,
    DateTime CreatedUtc,
    DateTime? ProcessedUtc,
    string? FailureReason
);