# ProductWorkflow.API

The project is about to manage Product entities with CRUD operations and a file upload processing feature. We have front-end developed in React and backend is ASP.NET Core Web API.

## Table of Contents

- Setup Instructions
- Project Structure
- Design Decisions
- Trade-offs Considered
- Testing

## Setup Instructions

### Prerequisites

- .NET 8 (or later) runtime
- SQL Server (local/cloud)
- IDE: Visual Studio / VS Code

### Clone Repository

\> git clone <https://github.com/ahmed-msku/productworkflow-api.git>  
\> cd productworkflow-api

### Configure Database

1. Update appsettings.json with your connection string:

"ConnectionStrings": {  
"DbConnection": "Server=.;Database=MyApiDb;Trusted_Connection=True;"  
}

1. Apply EF Core migrations:

\> dotnet ef database update

1. Restore Nuget Packages  
    \> dotnet restore

### Run the API

\> dotnet run

- API will be available at local port e.g. <https://localhost:5001> (or <http://localhost:5000>)

### Test Endpoints

- **CRUD** for Product: /api/product  
    **File Upload**: /api/product/upload (processed in background)

##

## Design Decisions

1. Product CRUD operations implemented using basic CRUD endpoints with the Repository pattern.
2. File upload endpoint saves files on disk after validation, creates a job for background processing, and returns an Accepted status with JobId and a link to check file processing status, improving app responsiveness.
3. A background service processes jobs using a Publisher/Consumer pattern with Channel.CreateBounded and configurable buffer size to read FileStream, along with two concurrent consumer tasks, storing data in batches.
4. DbContextFactory used for safe multi-threading, preventing race conditions with scope-based DbContext.
5. Singleton repositories use IDbContextFactory to create a DbContext per operation for thread-safe operations.
6. The design keeps memory usage in check, especially for large file processing, ensuring efficiency and scalability.
7. Clean folder structure maintained for separation of concerns.

##

## Trade-offs Considered

1. Background service is chosen for simplicity and handling moderate workloads. For heavy workloads, a queue service like RabbitMQ can be used for better scalability.
2. For batch insert, EF AddRange is used for simplicity. EF performs multiple inserts internally, which is fine for most cases, but this can be optimized using stored procedures.
3. Using models directly instead of DTOs for simplicity; in production, DTOs should be used.
4. Background service can be moved to a separate project for better separation of concerns and scalability.

## Testing

1. xUnit Integration Test added to validate basic file upload validation scenarios.
2. Tests live in a separate ProductWorflow.Test project
3. Run tests:

\> dotnet test
