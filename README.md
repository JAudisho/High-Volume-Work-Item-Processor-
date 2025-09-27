# High Volume Work Item Processor

Background processing demo on .NET 8 with batching, back pressure, and retries. Backed by SQL Server with indexes, views, and a paged search procedure.

## Stack
.NET 8, ASP.NET Core, EF Core, SQL Server, BackgroundService

## Prerequisites
- .NET 8 SDK
- Docker Desktop (for SQL Server)

## Run
1. Start SQL Server  
   `docker compose -f docker/docker-compose.yml up -d` (if present) or use local SQL
2. Configure connection string  
   Set `ConnectionStrings:Sql` for the API.
3. Create database and run  
   `dotnet ef database update` (project args as needed)  
   `dotnet run --project <ApiProjectPath>`  
   Open Swagger.

## SQL pack
Run in SSMS or sqlcmd:
1) `sql/01_indexes.sql`  
2) `sql/02_views.sql`  
3) `sql/03_proc_search.sql`  
4) `sql/04_perf_check.sql`

## Quick test (curl)
List latest items:
```bash
curl "http://localhost:5000/api/work-items?page=1&pageSize=25"

