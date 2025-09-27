create or alter proc dbo.usp_WorkItems_Search
  @status int = null,
  @q nvarchar(100) = null,   -- matches ExternalRef
  @skip int = 0,
  @take int = 50
as
begin
  set nocount on;
  select Id, ExternalRef, Status, Priority, CreatedUtc, ProcessedUtc
  from dbo.WorkItems
  where (@status is null or Status = @status)
    and (@q is null or ExternalRef like @q + '%')
  order by CreatedUtc desc
  offset @skip rows fetch next @take rows only;
end
