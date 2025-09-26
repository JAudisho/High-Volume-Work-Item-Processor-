using AtlasQueue.Domain.ValueObjects;

namespace AtlasQueue.Domain.DTO;

public record CreateWorkItemDto(string ExternalRef, string PayloadJson, Priority Priority = Priority.Medium);
