# AtlasQueue

A compact, production-flavored example of a **high-volume background processing** system built on **.NET 8**, **ASP.NET Core**, **EF Core (SQL Server)**, and **async channels**—plus a tiny static dashboard.

It demonstrates:
- Batch dequeue/processing with a `BackgroundService`
- Bounded channel for back-pressure (`System.Threading.Channels`)
- Clean layering (Domain / Infrastructure / API / Web)
- EF Core with SQL Server and fluent configurations
- Resiliency (Polly retry) and structured events per work item
- Caching list results (in-memory)
- Simple SPA to enqueue and observe throughput

> It’s intentionally domain-neutral: perfect to showcase strong SWE fundamentals without screaming “tailored for P&C insurance”.

---

## Run locally

### Prereqs
- .NET 8 SDK
- SQL Server LocalDB (default on Windows with Visual Studio). You can point to any SQL Server.

### 1) Restore & build
```bash
dotnet restore
dotnet build -c Release
