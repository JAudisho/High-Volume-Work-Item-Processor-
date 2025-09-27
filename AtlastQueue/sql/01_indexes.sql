create index IX_WorkItems_StatusPriorityCreated
  on dbo.WorkItems(Status, Priority, CreatedUtc)
  include (ProcessedUtc, ExternalRef);

create index IX_WorkItemEvents_ItemCreated
  on dbo.WorkItemEvents(WorkItemId, CreatedUtc);
