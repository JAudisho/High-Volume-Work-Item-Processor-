create or alter view dbo.v_QueueStats as
select
  sum(case when Status = 0 then 1 else 0 end) as Queued,
  sum(case when Status = 1 then 1 else 0 end) as Started,
  sum(case when Status = 2 then 1 else 0 end) as Completed,
  sum(case when Status = 3 then 1 else 0 end) as Failed
from dbo.WorkItems;

create or alter view dbo.v_LatencyMs as
select Id,
       datediff(millisecond, CreatedUtc, ProcessedUtc) as LatencyMs
from dbo.WorkItems
where ProcessedUtc is not null;
