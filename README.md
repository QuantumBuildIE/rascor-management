# RASCOR Stock Management System

A multi-tenant SaaS stock management system built with ASP.NET Core 8 and Clean Architecture.

## Solution Structure

```
Rascor.StockManagement/
├── src/
│   ├── Rascor.StockManagement.Domain/           # Domain layer (no dependencies)
│   │   ├── Common/                              # Base classes and interfaces
│   │   ├── Entities/                            # Domain entities
│   │   ├── ValueObjects/                        # Value objects
│   │   ├── Events/                              # Domain events
│   │   └── Enums/                               # Domain enumerations
│   │
│   ├── Rascor.StockManagement.Application/      # Application layer (depends on Domain)
│   │   ├── Common/                              # Common application classes
│   │   ├── DTOs/                                # Data transfer objects
│   │   ├── UseCases/                            # Application use cases/handlers
│   │   ├── Interfaces/                          # Application interfaces
│   │   └── Validators/                          # FluentValidation validators
│   │
│   ├── Rascor.StockManagement.Infrastructure/   # Infrastructure layer (depends on Application)
│   │   ├── Data/                                # EF Core DbContext
│   │   │   ├── Configurations/                  # Entity configurations
│   │   │   └── Migrations/                      # EF Core migrations
│   │   ├── Repositories/                        # Repository implementations
│   │   ├── Services/                            # External services
│   │   └── Identity/                            # ASP.NET Identity setup
│   │
│   └── Rascor.StockManagement.API/              # API layer (depends on Infrastructure)
│       ├── Controllers/                         # API controllers
│       ├── Middleware/                          # Custom middleware
│       ├── Filters/                             # Action filters
│       └── Extensions/                          # Extension methods
│
├── CLAUDE.md                                    # Project instructions and conventions
└── Rascor.StockManagement.sln                  # Solution file
```

## Project Dependencies

```
Domain (no dependencies)
   ↑
Application (references Domain)
   ↑
Infrastructure (references Application)
   ↑
API (references Infrastructure)
```

## Getting Started

### Prerequisites
- .NET 8 SDK or later
- PostgreSQL (or use Docker)
- IDE (Visual Studio, VS Code, or Rider)

### Build the Solution
```bash
dotnet build
```

### Run the API
```bash
dotnet run --project src/Rascor.StockManagement.API
```

## Architecture Principles

- **Clean Architecture**: Dependency rule flows inward (Domain ← Application ← Infrastructure ← API)
- **Multi-Tenancy**: All entities include `TenantId` with automatic filtering
- **Soft Deletes**: All entities support soft delete via `IsDeleted` flag
- **Audit Trail**: Automatic tracking of `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`

## Next Steps

Refer to [CLAUDE.md](CLAUDE.md) for:
- Detailed entity schemas
- Business rules and workflows
- Coding conventions
- Development roadmap

## Technology Stack

- ASP.NET Core 8 Web API
- Entity Framework Core 8
- PostgreSQL
- FluentValidation
- ASP.NET Identity + JWT
- QuestPDF (for documents)
- Next.js 14+ (frontend - to be added)
