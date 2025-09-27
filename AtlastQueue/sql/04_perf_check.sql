set statistics io on;
set statistics time on;

exec dbo.usp_WorkItems_Search @status = 0, @skip = 0, @take = 25;
select top 1 * from dbo.v_QueueStats;
select avg(LatencyMs) as AvgLatencyMs from dbo.v_LatencyMs;
