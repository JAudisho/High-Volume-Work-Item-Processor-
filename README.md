# AtlasQueue

A compact, production-style example of a **high-volume background processing system** built with **.NET 8**, **ASP.NET Core**, **EF Core (SQL Server)**, and **async channels**, with a lightweight static dashboard.  

It showcases patterns for **scalable, reliable, and observable processing** that are common in enterprise systems.  

## Key Features
- Batch dequeue and processing using `BackgroundService`
- Bounded channel for back-pressure (`System.Threading.Channels`)
- Clean layered architecture (Domain / Infrastructure / API / Web)
- EF Core with SQL Server and fluent entity configurations
- Resiliency with Polly retries and structured event tracking per work item
- In-memory caching for list results
- Simple web UI to enqueue items, filter results, and observe throughput

> This project is intentionally domain-neutral so it highlights strong software engineering fundamentals without being tied to a single industry use case.  

---

## Run Locally

### Prerequisites
- .NET 8 SDK
- SQL Server LocalDB (default with Visual Studio on Windows) or any SQL Server instance

### 1) Restore & build
```bash
dotnet restore
dotnet build -c Release
