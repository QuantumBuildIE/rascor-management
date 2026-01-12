# RASCOR Business Suite

A multi-tenant SaaS management system for construction companies built with a modular monolith architecture.

---

## Project Overview

Originally prototyped in Zoho Creator, now rebuilt as a custom application for production use, code ownership, and potential resale.

### Business Context
- **Primary User:** RASCOR (construction company) managing stock across 30+ construction sites
- **Scale:** ~200 products, 10 suppliers, 1 main warehouse + site stores
- **Key Workflows:** Stock ordering by sites, purchase orders, goods receiving, stock tracking, site attendance, proposals, RAMS, toolbox talks

### Currently Implemented
- **Stock Management Module** - Full CRUD + business workflows with bay location tracking
- **Proposals Module** - Proposals with sections, line items, Product Kits, workflow, PDF generation, reports
- **Site Attendance Module** - GPS-based entry/exit tracking, daily summaries, SPA compliance, executive dashboard
- **Admin Module (Core)** - Sites, Employees, Companies, Users management
- **Authentication & Authorization** - JWT with permission-based policies
- **Dashboard** - Module selector with charts and analytics
- **Stock Reports** - Valuation reports, analytics, and QR code stocktaking

---

## Technology Stack

### Backend
| Technology | Version | Purpose |
|------------|---------|---------|
| ASP.NET Core | 9.0 | Web API Framework |
| Entity Framework Core | 9.0 | ORM |
| PostgreSQL | Latest | Database |
| ASP.NET Identity | 9.0 | Authentication |
| FluentValidation | Latest | Request validation |
| Swashbuckle | 7.2.0 | Swagger/OpenAPI |

### Frontend
| Technology | Version | Purpose |
|------------|---------|---------|
| Next.js | 16.0.10 | React framework (App Router) |
| React | 19.2.1 | UI library |
| TailwindCSS | 4.x | Styling |
| shadcn/ui | Latest | UI component library |
| TanStack Query | 5.90.12 | Data fetching & caching |
| React Hook Form | 7.68.0 | Form handling |
| Zod | 4.2.1 | Schema validation |
| Recharts | 3.6.0 | Charts & analytics |
| Axios | 1.13.2 | HTTP client |
| date-fns | 4.1.0 | Date utilities |
| qrcode.react | 4.2.0 | QR code generation |

---

## Solution Structure

```
rascor-management/
├── src/
│   ├── Core/                                    # Shared across all modules
│   │   ├── Rascor.Core.Domain/                 # Shared entities, base classes
│   │   │   ├── Common/                         # BaseEntity, TenantEntity
│   │   │   └── Entities/                       # Tenant, User, Role, Permission, Site, Employee, Company, Contact
│   │   ├── Rascor.Core.Application/            # Shared interfaces, models, DTOs
│   │   │   ├── DTOs/                           # Auth DTOs, Core entity DTOs
│   │   │   ├── Interfaces/                     # ICurrentUserService, ICoreDbContext, IAuthService
│   │   │   └── Models/                         # PaginatedList, Result
│   │   └── Rascor.Core.Infrastructure/         # Shared EF configurations, Identity, Seeding
│   │       ├── Data/Configurations/            # Entity configurations for Core entities
│   │       ├── Identity/                       # AuthService, Permissions, PermissionHandler
│   │       └── Persistence/                    # DataSeeder, StockManagementSeeder
│   │
│   ├── Modules/
│   │   ├── StockManagement/                    # Stock Management Module
│   │   │   ├── Rascor.Modules.StockManagement.Domain/
│   │   │   │   └── Entities/                   # 15 entities (Category, Product, BayLocation, etc.)
│   │   │   ├── Rascor.Modules.StockManagement.Application/
│   │   │   │   ├── Common/Interfaces/          # IStockManagementDbContext
│   │   │   │   └── Features/                   # Service classes per feature
│   │   │   └── Rascor.Modules.StockManagement.Infrastructure/
│   │   │       └── Data/                       # ApplicationDbContext, Configurations
│   │   │
│   │   ├── Proposals/                          # Proposals Module
│   │       ├── Rascor.Modules.Proposals.Domain/
│   │       │   └── Entities/                   # 7 entities (Proposal, ProposalSection, ProposalLineItem, etc.)
│   │       ├── Rascor.Modules.Proposals.Application/
│   │       │   ├── Common/Interfaces/          # IProposalsDbContext
│   │       │   ├── DTOs/                       # Proposal DTOs, Kit DTOs, Report DTOs
│   │       │   └── Services/                   # ProposalService, ProductKitService, ProposalReportService
│   │       └── Rascor.Modules.Proposals.Infrastructure/
│   │           └── Data/                       # ProposalsDbContext, Configurations
│   │   │
│   │   └── SiteAttendance/                     # Site Attendance Module
│   │       ├── Rascor.Modules.SiteAttendance.Domain/
│   │       │   ├── Entities/                   # 7 entities (AttendanceEvent, AttendanceSummary, etc.)
│   │       │   └── Enums/                      # EventType, TriggerMethod, AttendanceStatus, etc.
│   │       ├── Rascor.Modules.SiteAttendance.Application/
│   │       │   ├── Commands/                   # CQRS commands (RecordEvent, CreateSPA, etc.)
│   │       │   ├── Queries/                    # CQRS queries (GetKPIs, GetPerformance, etc.)
│   │       │   ├── DTOs/                       # Data transfer objects
│   │       │   └── Services/                   # Time calculation, geofencing, notifications
│   │       └── Rascor.Modules.SiteAttendance.Infrastructure/
│   │           ├── Persistence/                # SiteAttendanceDbContext, Configurations
│   │           ├── Repositories/               # Repository implementations
│   │           ├── Services/                   # Service implementations
│   │           └── Jobs/                       # Hangfire background jobs
│   │
│   └── Rascor.API/                             # Single API entry point
│       ├── Controllers/                        # 22 API controllers
│       └── Program.cs                          # Service registration
│
└── web/                                         # Next.js Frontend
    └── src/
        ├── app/                                # App Router pages
        │   ├── login/                          # Login page
        │   └── (authenticated)/                # Protected routes
        │       ├── dashboard/                  # Module selector + charts
        │       ├── stock/                      # Stock Management pages
        │       ├── proposals/                  # Proposals pages
        │       ├── site-attendance/            # Site Attendance pages
        │       ├── admin/                      # Admin pages (sites, employees, etc.)
        │       └── profile/                    # User profile
        ├── components/
        │   ├── ui/                             # shadcn/ui components (23 components)
        │   ├── shared/                         # DataTable, DeleteConfirmationDialog
        │   └── layout/                         # TopNav, etc.
        ├── lib/
        │   ├── api/                            # Axios client with interceptors
        │   └── auth/                           # Auth context, hooks, utilities
        └── types/                              # TypeScript type definitions
```

---

## Backend API Endpoints

### Authentication (`/api/auth`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/login` | Authenticate and get JWT tokens | No |
| POST | `/register` | Register new user | No |
| POST | `/refresh-token` | Refresh expired access token | No |
| POST | `/revoke-token` | Logout (revoke refresh token) | Yes |
| GET | `/me` | Get current user info + permissions | Yes |

### Core Module

#### Users (`/api/users`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List users (paginated) | Core.ManageUsers |
| GET | `/{id}` | Get user by ID | Core.ManageUsers |
| POST | `/` | Create user | Core.ManageUsers |
| PUT | `/{id}` | Update user | Core.ManageUsers |
| DELETE | `/{id}` | Delete user | Core.ManageUsers |
| PUT | `/{id}/toggle-active` | Toggle user active status | Core.ManageUsers |
| PUT | `/{id}/roles` | Update user roles | Core.ManageUsers |

#### Roles (`/api/roles`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List all roles | Core.ManageRoles |
| GET | `/{id}` | Get role by ID | Core.ManageRoles |
| GET | `/permissions` | List all permissions | Core.ManageRoles |

#### Sites (`/api/sites`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List sites (paginated) | Core.ManageSites |
| GET | `/{id}` | Get site by ID | Core.ManageSites |
| POST | `/` | Create site | Core.ManageSites |
| PUT | `/{id}` | Update site | Core.ManageSites |
| DELETE | `/{id}` | Delete site (soft) | Core.ManageSites |

#### Employees (`/api/employees`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List employees (paginated) | Core.ManageEmployees |
| GET | `/{id}` | Get employee by ID | Core.ManageEmployees |
| POST | `/` | Create employee | Core.ManageEmployees |
| PUT | `/{id}` | Update employee | Core.ManageEmployees |
| DELETE | `/{id}` | Delete employee (soft) | Core.ManageEmployees |

#### Companies (`/api/companies`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List companies (paginated) | Core.ManageCompanies |
| GET | `/{id}` | Get company by ID | Core.ManageCompanies |
| POST | `/` | Create company | Core.ManageCompanies |
| PUT | `/{id}` | Update company | Core.ManageCompanies |
| DELETE | `/{id}` | Delete company (soft) | Core.ManageCompanies |

#### Contacts (`/api/contacts`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List contacts (paginated) | Core.ManageCompanies |
| GET | `/{id}` | Get contact by ID | Core.ManageCompanies |
| POST | `/` | Create contact | Core.ManageCompanies |
| PUT | `/{id}` | Update contact | Core.ManageCompanies |
| DELETE | `/{id}` | Delete contact (soft) | Core.ManageCompanies |

### Stock Management Module

#### Categories (`/api/categories`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List categories | StockManagement.View |
| GET | `/{id}` | Get category by ID | StockManagement.View |
| POST | `/` | Create category | StockManagement.ManageProducts |
| PUT | `/{id}` | Update category | StockManagement.ManageProducts |
| DELETE | `/{id}` | Delete category | StockManagement.ManageProducts |

#### Products (`/api/products`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List products (paginated) | StockManagement.View |
| GET | `/{id}` | Get product by ID | StockManagement.View |
| POST | `/` | Create product | StockManagement.ManageProducts |
| PUT | `/{id}` | Update product | StockManagement.ManageProducts |
| DELETE | `/{id}` | Delete product | StockManagement.ManageProducts |

#### Suppliers (`/api/suppliers`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List suppliers (paginated) | StockManagement.View |
| GET | `/{id}` | Get supplier by ID | StockManagement.View |
| POST | `/` | Create supplier | StockManagement.ManageSuppliers |
| PUT | `/{id}` | Update supplier | StockManagement.ManageSuppliers |
| DELETE | `/{id}` | Delete supplier | StockManagement.ManageSuppliers |

#### Stock Locations (`/api/stock-locations`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List stock locations | StockManagement.View |
| GET | `/{id}` | Get location by ID | StockManagement.View |
| POST | `/` | Create location | StockManagement.Admin |
| PUT | `/{id}` | Update location | StockManagement.Admin |
| DELETE | `/{id}` | Delete location | StockManagement.Admin |

#### Bay Locations (`/api/bay-locations`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List bay locations | StockManagement.View |
| GET | `/{id}` | Get bay location by ID | StockManagement.View |
| GET | `/by-location/{stockLocationId}` | Get bays for a stock location | StockManagement.View |
| POST | `/` | Create bay location | StockManagement.Admin |
| PUT | `/{id}` | Update bay location | StockManagement.Admin |
| DELETE | `/{id}` | Delete bay location | StockManagement.Admin |

#### Stock Levels (`/api/stock-levels`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List stock levels (paginated) | StockManagement.View |
| GET | `/location/{locationId}` | Get levels by location | StockManagement.View |
| GET | `/product/{productId}` | Get levels by product | StockManagement.View |
| GET | `/low-stock` | Get low stock items | StockManagement.View |

#### Stock Transactions (`/api/stock-transactions`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List transactions (paginated) | StockManagement.View |
| GET | `/{id}` | Get transaction by ID | StockManagement.View |
| GET | `/product/{productId}` | Get transactions for product | StockManagement.View |
| GET | `/location/{locationId}` | Get transactions for location | StockManagement.View |

#### Stock Orders (`/api/stock-orders`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List orders (paginated) | StockManagement.View |
| GET | `/{id}` | Get order by ID | StockManagement.View |
| POST | `/` | Create order | StockManagement.CreateOrders |
| PUT | `/{id}` | Update order | StockManagement.CreateOrders |
| DELETE | `/{id}` | Delete order | StockManagement.CreateOrders |
| POST | `/{id}/submit` | Submit for approval | StockManagement.CreateOrders |
| POST | `/{id}/approve` | Approve order | StockManagement.ApproveOrders |
| POST | `/{id}/reject` | Reject order | StockManagement.ApproveOrders |
| POST | `/{id}/cancel` | Cancel order | StockManagement.ApproveOrders |
| POST | `/{id}/ready` | Mark ready for collection | StockManagement.ReceiveGoods |
| POST | `/{id}/collect` | Mark as collected | StockManagement.ReceiveGoods |

#### Purchase Orders (`/api/purchase-orders`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List POs (paginated) | StockManagement.View |
| GET | `/{id}` | Get PO by ID | StockManagement.View |
| POST | `/` | Create PO | StockManagement.ManageProducts |
| PUT | `/{id}` | Update PO | StockManagement.ManageProducts |
| DELETE | `/{id}` | Delete PO | StockManagement.ManageProducts |
| POST | `/{id}/confirm` | Confirm PO | StockManagement.ManageProducts |
| POST | `/{id}/cancel` | Cancel PO | StockManagement.ManageProducts |

#### Goods Receipts (`/api/goods-receipts`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List GRNs (paginated) | StockManagement.View |
| GET | `/{id}` | Get GRN by ID | StockManagement.View |
| POST | `/` | Create GRN | StockManagement.ReceiveGoods |
| PUT | `/{id}` | Update GRN | StockManagement.ReceiveGoods |
| DELETE | `/{id}` | Delete GRN | StockManagement.ReceiveGoods |
| POST | `/{id}/complete` | Complete GRN (updates stock) | StockManagement.ReceiveGoods |

#### Stocktakes (`/api/stocktakes`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List stocktakes (paginated) | StockManagement.Stocktake |
| GET | `/{id}` | Get stocktake by ID | StockManagement.Stocktake |
| POST | `/` | Create stocktake | StockManagement.Stocktake |
| PUT | `/{id}` | Update stocktake | StockManagement.Stocktake |
| DELETE | `/{id}` | Delete stocktake | StockManagement.Stocktake |
| POST | `/{id}/start` | Start stocktake | StockManagement.Stocktake |
| POST | `/{id}/complete` | Complete stocktake | StockManagement.Stocktake |

#### Stock Reports (`/api/stock/reports`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/summary` | Stock summary by location | StockManagement.View |
| GET | `/value` | Stock value report | StockManagement.ViewCostings |
| GET | `/movements` | Stock movements report | StockManagement.View |
| GET | `/valuation` | Stock valuation report (by location/category) | StockManagement.ViewCostings |
| GET | `/products-by-month` | Top products by value per month | StockManagement.View |
| GET | `/products-by-site` | Top products by value per site | StockManagement.View |
| GET | `/products-by-week` | Top products by value per week | StockManagement.View |

### Proposals Module

#### Product Kits (`/api/product-kits`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List product kits (paginated) | Proposals.View |
| GET | `/{id}` | Get kit by ID with items | Proposals.View |
| POST | `/` | Create kit | Proposals.Create |
| PUT | `/{id}` | Update kit | Proposals.Edit |
| DELETE | `/{id}` | Delete kit | Proposals.Delete |

#### Proposals (`/api/proposals`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List proposals (paginated, filterable) | Proposals.View |
| GET | `/{id}` | Get proposal by ID with all details | Proposals.View |
| POST | `/` | Create proposal | Proposals.Create |
| PUT | `/{id}` | Update proposal | Proposals.Edit |
| DELETE | `/{id}` | Delete proposal (draft only) | Proposals.Delete |
| POST | `/{id}/submit` | Submit for approval | Proposals.Submit |
| POST | `/{id}/approve` | Approve proposal | Proposals.Approve |
| POST | `/{id}/reject` | Reject proposal | Proposals.Approve |
| POST | `/{id}/win` | Mark as won | Proposals.Edit |
| POST | `/{id}/lose` | Mark as lost | Proposals.Edit |
| POST | `/{id}/cancel` | Cancel proposal | Proposals.Approve |
| POST | `/{id}/revise` | Create new revision | Proposals.Edit |
| GET | `/{id}/pdf` | Generate PDF (client version) | Proposals.View |
| GET | `/{id}/pdf/internal` | Generate PDF (internal with costings) | Proposals.ViewCostings |
| POST | `/{id}/convert-to-stock-order` | Convert won proposal to stock order | Proposals.Edit |

#### Proposal Reports (`/api/proposals/reports`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/pipeline` | Pipeline value by status | Proposals.View |
| GET | `/conversion` | Win/loss conversion rates | Proposals.View |
| GET | `/by-status` | Count by status | Proposals.View |
| GET | `/by-company` | Proposals grouped by company | Proposals.View |
| GET | `/win-loss` | Win/loss analysis | Proposals.View |
| GET | `/monthly-trends` | Monthly submission/won trends | Proposals.View |

### Site Attendance Module

#### Attendance Events (`/api/site-attendance/events`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| POST | `/` | Record attendance event (for mobile app) | Authenticated |
| POST | `/batch` | Batch record events (offline sync support) | Authenticated |
| GET | `/` | List events with filters (employee, site, date, type) | Authenticated |
| GET | `/{id}` | Get event by ID | Authenticated |

#### Attendance Summaries (`/api/site-attendance/summaries`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List summaries with filters | SiteAttendance.View |
| GET | `/employee/{employeeId}` | Get summaries for employee | SiteAttendance.View |
| GET | `/site/{siteId}` | Get summaries for site | SiteAttendance.View |

#### Dashboard (`/api/site-attendance/dashboard`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/kpis` | Dashboard KPIs (utilization, variance, counts) | SiteAttendance.View |
| GET | `/employee-performance` | Employee performance breakdown | SiteAttendance.View |

#### Site Photo Attendance (`/api/site-attendance/spa`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| POST | `/` | Create SPA record | SiteAttendance.MarkAttendance |
| POST | `/{id}/image` | Upload image for SPA | SiteAttendance.MarkAttendance |
| PUT | `/{id}` | Update SPA record | SiteAttendance.MarkAttendance |
| GET | `/` | List SPA records with filters | SiteAttendance.View |
| GET | `/{id}` | Get SPA by ID | SiteAttendance.View |

#### Attendance Settings (`/api/site-attendance/settings`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | Get tenant attendance settings | SiteAttendance.View |
| PUT | `/` | Update attendance settings | SiteAttendance.Admin |

#### Bank Holidays (`/api/site-attendance/bank-holidays`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/` | List bank holidays (optionally by year) | SiteAttendance.View |
| POST | `/` | Create bank holiday | SiteAttendance.Admin |
| PUT | `/{id}` | Update bank holiday | SiteAttendance.Admin |
| DELETE | `/{id}` | Delete bank holiday | SiteAttendance.Admin |

#### Device Registrations (`/api/site-attendance/devices`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| POST | `/register` | Register device for push notifications | SiteAttendance.MarkAttendance |
| GET | `/` | List registered devices | SiteAttendance.Admin |
| PUT | `/{id}` | Update device registration | SiteAttendance.Admin |
| DELETE | `/{id}` | Deactivate device (soft delete) | SiteAttendance.Admin |

### RAMS Module

#### RAMS AI Suggestions (`/api/rams/ai`)
| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| POST | `/suggest-controls` | Get AI-powered control measure suggestions | Authenticated |
| POST | `/accept-suggestion` | Mark suggestion as accepted (for analytics) | Authenticated |

---

## Frontend Pages

### Public Pages
| Path | Description |
|------|-------------|
| `/login` | Login page with "Keep me logged in" option |

### Authenticated Pages

#### Dashboard
| Path | Description |
|------|-------------|
| `/dashboard` | Module selector with charts (orders, stock levels, low stock) |

#### Stock Management (`/stock/*`)
| Path | Description |
|------|-------------|
| `/stock` | Stock module home with quick stats and analytics charts |
| `/stock/categories` | List categories |
| `/stock/categories/new` | Create category |
| `/stock/categories/[id]/edit` | Edit category |
| `/stock/products` | List products with search & filter |
| `/stock/products/new` | Create product |
| `/stock/products/[id]/edit` | Edit product |
| `/stock/suppliers` | List suppliers |
| `/stock/suppliers/new` | Create supplier |
| `/stock/suppliers/[id]/edit` | Edit supplier |
| `/stock/bay-locations` | List bay locations (warehouse bins/shelves) |
| `/stock/bay-locations/new` | Create bay location |
| `/stock/bay-locations/[id]/edit` | Edit bay location |
| `/stock/levels` | Stock levels with low stock highlighting |
| `/stock/orders` | List stock orders with status filters |
| `/stock/orders/new` | Create stock order |
| `/stock/orders/[id]` | View order details with workflow actions |
| `/stock/orders/[id]/edit` | Edit stock order |
| `/stock/orders/[id]/print` | Printable order docket (A4, warehouse use) |
| `/stock/purchase-orders` | List purchase orders |
| `/stock/purchase-orders/new` | Create PO |
| `/stock/purchase-orders/[id]` | View PO details |
| `/stock/purchase-orders/[id]/edit` | Edit PO |
| `/stock/goods-receipts` | List goods receipts |
| `/stock/goods-receipts/new` | Create GRN |
| `/stock/goods-receipts/[id]` | View GRN details |
| `/stock/stocktakes` | List stocktakes |
| `/stock/stocktakes/new` | Create stocktake |
| `/stock/stocktakes/[id]` | View/conduct stocktake |
| `/stock/stocktakes/[id]/print` | Printable count sheets with QR codes |
| `/stock/stocktakes/[id]/count/[lineId]` | Mobile count entry (QR code target) |
| `/stock/reports` | Reports landing page |
| `/stock/reports/valuation` | Stock valuation report with filters |

#### Proposals (`/proposals/*`)
| Path | Description |
|------|-------------|
| `/proposals` | Proposals module home with analytics dashboard |
| `/proposals/list` | List proposals with status filters and search |
| `/proposals/new` | Create new proposal |
| `/proposals/[id]` | View proposal with sections, items, workflow actions |
| `/proposals/[id]/edit` | Edit proposal |
| `/proposals/[id]/pdf` | PDF preview (client version) |
| `/proposals/kits` | List product kits |
| `/proposals/kits/new` | Create product kit |
| `/proposals/kits/[id]/edit` | Edit product kit |
| `/proposals/reports` | Reports landing page |

#### Site Attendance (`/site-attendance/*`)
| Path | Description |
|------|-------------|
| `/site-attendance` | Executive dashboard with KPIs and employee performance |
| `/site-attendance/events` | List attendance events with filters |
| `/site-attendance/summaries` | Daily attendance summaries |
| `/site-attendance/bank-holidays` | Bank holiday management |
| `/site-attendance/settings` | Attendance configuration settings |

#### Admin (`/admin/*`)
| Path | Description |
|------|-------------|
| `/admin` | Admin module home |
| `/admin/sites` | List sites |
| `/admin/sites/new` | Create site |
| `/admin/sites/[id]/edit` | Edit site |
| `/admin/employees` | List employees |
| `/admin/employees/new` | Create employee |
| `/admin/employees/[id]/edit` | Edit employee |
| `/admin/companies` | List companies |
| `/admin/companies/new` | Create company |
| `/admin/companies/[id]` | View company with contacts |
| `/admin/companies/[id]/edit` | Edit company |
| `/admin/users` | List users |
| `/admin/users/new` | Create user |
| `/admin/users/[id]/edit` | Edit user |

#### User
| Path | Description |
|------|-------------|
| `/profile` | User profile and password change |

---

## Authentication & Authorization

### JWT Bearer Authentication
- Access tokens expire in 60 minutes
- Refresh tokens expire in 7 days
- Automatic token refresh on 401 responses
- "Keep me logged in" uses localStorage (persistent) vs sessionStorage (session only)

### Token Claims
```
sub         = User ID
email       = Email address
given_name  = First name
family_name = Last name
tenant_id   = Tenant ID
role        = Role names (array)
permission  = Permission names (array)
```

### Permissions

#### Stock Management Module
| Permission | Description |
|------------|-------------|
| `StockManagement.View` | View stock data |
| `StockManagement.CreateOrders` | Create stock orders |
| `StockManagement.ApproveOrders` | Approve/reject stock orders |
| `StockManagement.ViewCostings` | View cost and pricing information |
| `StockManagement.ManageProducts` | Create/edit/delete products, categories |
| `StockManagement.ManageSuppliers` | Create/edit/delete suppliers |
| `StockManagement.ReceiveGoods` | Receive goods, create GRNs |
| `StockManagement.Stocktake` | Perform stocktakes |
| `StockManagement.Admin` | Full stock management administration |

#### Core Module
| Permission | Description |
|------------|-------------|
| `Core.ManageSites` | Manage sites |
| `Core.ManageEmployees` | Manage employees |
| `Core.ManageCompanies` | Manage companies and contacts |
| `Core.ManageUsers` | Manage user accounts |
| `Core.ManageRoles` | Manage roles and permissions |
| `Core.Admin` | Full core system administration |

#### Proposals Module
| Permission | Description |
|------------|-------------|
| `Proposals.View` | View proposals list and details |
| `Proposals.Create` | Create new proposals |
| `Proposals.Edit` | Edit proposals, mark won/lost |
| `Proposals.Delete` | Delete draft proposals |
| `Proposals.Submit` | Submit proposals for approval |
| `Proposals.Approve` | Approve or reject proposals |
| `Proposals.ViewCostings` | View cost prices and margins |
| `Proposals.Admin` | Full proposals administration |

#### Site Attendance Module
| Permission | Description |
|------------|-------------|
| `SiteAttendance.View` | View attendance records, summaries, KPIs |
| `SiteAttendance.MarkAttendance` | Record events, create SPA, register devices |
| `SiteAttendance.Admin` | Full attendance administration (settings, bank holidays)

### Roles and Default Permissions

| Role | Permissions |
|------|-------------|
| **Admin** | All permissions |
| **Finance** | *.View, StockManagement.ViewCostings, Proposals.View, Proposals.ViewCostings |
| **OfficeStaff** | Proposals.View, Proposals.Create, Proposals.Edit, Proposals.Submit, StockManagement.View, StockManagement.CreateOrders |
| **SiteManager** | Proposals.View, SiteAttendance.*, StockManagement.View, StockManagement.CreateOrders |
| **WarehouseStaff** | StockManagement.* (except Admin and ViewCostings) |

---

## Seeded Data

### Test Credentials

| Role | Email | Password | Home Page |
|------|-------|----------|-----------|
| Admin | admin@rascor.ie | Admin123! | /dashboard |
| Warehouse | warehouse@rascor.ie | Warehouse123! | /stock |
| Site Manager | sitemanager@rascor.ie | SiteManager123! | /stock/orders |
| Office Staff | office@rascor.ie | Office123! | /dashboard |
| Finance | finance@rascor.ie | Finance123! | /dashboard |

### Default Tenant
- **Name:** RASCOR
- **ID:** `11111111-1111-1111-1111-111111111111`

### Test Sites
7 sites seeded: Quantum Build (Dublin), South West Gate (Cork), Marmalade Lane (Galway), Rathbourne Crossing (Dublin), Castleforbes Prem Inn (Dublin), Eden (Limerick), Ford (Waterford)

### Test Stock Data
- Categories: Building Materials, Electrical, Plumbing, Safety Equipment, Tools, Fixings & Fasteners
- Products: ~50 products with realistic pricing and reorder levels
- Suppliers: 5 suppliers with contact details
- Stock Locations: Main Warehouse + site stores

---

## Business Workflows

### Stock Order Workflow
```
Draft → PendingApproval → Approved → AwaitingPick → ReadyForCollection → Collected
                       ↘ Rejected
                       ↘ Cancelled (at any stage)
```

| State | Action | Stock Effect |
|-------|--------|--------------|
| Draft | Create order | None |
| PendingApproval | Submit order | None |
| Approved | Approve order | QuantityReserved increases |
| ReadyForCollection | Mark ready | None |
| Approved/ReadyForCollection | Print Docket | Opens printable order docket |
| Collected | Mark collected | QuantityOnHand decreases, QuantityReserved decreases |
| Cancelled | Cancel order | QuantityReserved released (if was reserved) |

### Purchase Order Workflow
```
Draft → Confirmed → PartiallyReceived → FullyReceived
                 ↘ Cancelled
```

### Goods Receipt Workflow
1. Create GRN (optionally linked to PO)
2. Add line items with received quantities
3. Optionally specify:
   - Bay location for storage
   - Batch/lot number for traceability
   - Expiry date for perishable items
   - Rejected quantities and reasons
4. Complete GRN:
   - Increases `StockLevel.QuantityOnHand`
   - Decreases `StockLevel.QuantityOnOrder` (if from PO)
   - Updates PO line status
   - Creates `StockTransaction` records

### Stocktake Workflow
```
Draft → InProgress → Completed
```

1. Create stocktake for a location
2. Start stocktake (captures current system quantities)
3. Print QR code count sheets (optional but recommended)
4. Enter counted quantities for each product:
   - Via stocktake detail page (desktop)
   - Via QR code scanning (mobile)
5. Complete stocktake:
   - Calculates variances
   - Optionally record variance reasons
   - Creates adjustment transactions
   - Updates stock levels
   - Records last count date

**QR Code Count Sheet Workflow:**
1. Navigate to stocktake detail page
2. Click "Print Count Sheets"
3. Print A4 sheets (one per line item)
4. Warehouse staff scan QR codes with mobile device
5. Mobile page loads with product details pre-filled
6. Enter counted quantity and submit
7. System updates stocktake in real-time

### Proposal Workflow
```
Draft → PendingApproval → Approved → Won
                       ↘ Rejected   ↘ Lost
                       ↘ Cancelled (at any stage)
```

| State | Action | Effect |
|-------|--------|--------|
| Draft | Create/edit proposal | Can add sections, items, contacts |
| Draft | Submit | Moves to PendingApproval |
| PendingApproval | Approve | Moves to Approved, ready to send to client |
| PendingApproval | Reject | Moves to Rejected with reason |
| Approved | Win | Marks as won, enables Convert to Stock Order |
| Approved | Lose | Marks as lost with reason |
| Approved | Revise | Creates new version (v2, v3, etc.) |
| Won | Convert to Stock Order | Creates stock order from proposal items |
| Any | Cancel | Cancels proposal |

**Proposal Structure:**
- **Header**: Reference, company, site, project name, dates, validity
- **Sections**: Group related items (can be created from Product Kits)
- **Line Items**: Products or ad-hoc items with quantity, unit price, discount
- **Contacts**: Client contacts with roles (Decision Maker, Technical, etc.)
- **Totals**: Subtotal, VAT (23%), Grand Total, Margin calculation

**Convert to Stock Order:**
1. Navigate to won proposal
2. Click "Convert to Stock Order"
3. Select destination site
4. Preview items to be ordered
5. Confirm to create stock order
6. Stock order created in Draft status

### Site Attendance Workflow

**Event Processing Flow:**
```
Mobile Device → GPS Event → Geofence Check → Record Event → Noise Filtering → Daily Summary
```

**Event Types:**
- `Enter` - Employee enters site geofence
- `Exit` - Employee exits site geofence

**Trigger Methods:**
- `Automatic` - GPS geofence triggered automatically
- `Manual` - User manually checked in/out

**Attendance Status (calculated at summary level):**
- `Excellent` - >= 90% utilization (target hours vs actual)
- `Good` - 75-90% utilization
- `BelowTarget` - < 75% utilization
- `Absent` - No attendance recorded
- `Incomplete` - Entry but no exit, or vice versa

**Daily Processing (Background Job at 1:00 AM):**
1. Get all unprocessed events for previous day
2. Filter out noise events (rapid entry/exit within threshold)
3. Group events by employee and site
4. Calculate time on site (sum of entry-exit pairs)
5. Create/update AttendanceSummary records
6. Check for SPA compliance
7. Mark events as processed

**Noise Filtering:**
- Events within `NoiseThresholdMeters` (default 150m) of each other within short time are marked as noise
- Prevents false entries/exits from GPS jitter

**SPA (Site Photo Attendance) Compliance:**
- Photo proof of site attendance for RAMS compliance
- Links to AttendanceSummary via `HasSpa` flag
- Grace period for late SPA submission (configurable)

---

## Reusable Frontend Components

### shadcn/ui Components (23)
accordion, alert-dialog, avatar, badge, button, calendar, card, checkbox, command, dialog, dropdown-menu, form, input, label, popover, select, separator, sheet, skeleton, sonner, table, tabs, textarea

### Shared Components
| Component | Location | Description |
|-----------|----------|-------------|
| DataTable | `components/shared/data-table.tsx` | Paginated table with sorting, search |
| DeleteConfirmationDialog | `components/shared/delete-confirmation-dialog.tsx` | Confirm delete with toast |
| TopNav | `components/layout/top-nav.tsx` | Header with user dropdown, logo links to role home |

### Form Pattern
```tsx
// All forms use React Hook Form + Zod
const schema = z.object({ ... });
type FormData = z.infer<typeof schema>;

const form = useForm<FormData>({
  resolver: zodResolver(schema),
  defaultValues: { ... }
});
```

### Query Pattern
```tsx
// All data fetching uses TanStack Query
const { data, isLoading, error } = useQuery({
  queryKey: ['products', page, search],
  queryFn: () => apiClient.get('/products', { params: { page, search } })
});

// Mutations with cache invalidation
const mutation = useMutation({
  mutationFn: (data) => apiClient.post('/products', data),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['products'] });
    toast.success('Product created');
  }
});
```

---

## Running the Application

### Prerequisites
- .NET 9 SDK
- Node.js 20+
- PostgreSQL

### Backend (API)
```bash
cd src/Rascor.API
dotnet run
# Runs on http://localhost:5222
# Swagger: http://localhost:5222/swagger
```

### Frontend
```bash
cd web
npm install
npm run dev
# Runs on http://localhost:3000
```

### Database
- Host: localhost (127.0.0.1)
- Port: 5432
- Database: rascor_stock
- Username: postgres

### EF Migrations
```bash
cd src/Rascor.API
dotnet ef migrations add MigrationName --project ../Modules/StockManagement/Rascor.Modules.StockManagement.Infrastructure
dotnet ef database update --project ../Modules/StockManagement/Rascor.Modules.StockManagement.Infrastructure
```

---

## Project Status

### Completed
- [x] Modular monolith architecture
- [x] Multi-tenancy with tenant isolation
- [x] JWT authentication with refresh tokens
- [x] Permission-based authorization
- [x] Core domain entities (Tenant, User, Role, Permission, Site, Employee, Company, Contact)
- [x] Stock Management module (15 entities, full CRUD)
- [x] Bay Location tracking for warehouse bins/shelves
- [x] Stock Order workflow with stock reservations
- [x] Purchase Order workflow
- [x] Goods Receipt workflow with stock updates, batch tracking, expiry dates
- [x] Stocktake workflow with adjustments and variance tracking
- [x] QR code count sheets for mobile stocktaking
- [x] Stock valuation reporting (Finance-restricted)
- [x] Stock analytics dashboard with charts
- [x] Dashboard with module selector and charts
- [x] Admin UI (Sites, Employees, Companies, Users)
- [x] Stock Management UI (all pages including bay locations and reports)
- [x] Stock Order print docket (A4 printable for warehouse)
- [x] User profile page
- [x] Test data seeding
- [x] Role-based home page routing
- [x] **Proposals module:**
  - [x] Proposals with sections and line items (full CRUD)
  - [x] Product Kits with kit items
  - [x] Workflow: Submit, Approve, Reject, Win, Lose, Cancel
  - [x] Versioning/Revisions
  - [x] Kit expansion (auto-populate sections from kits)
  - [x] Auto-calculation of totals, VAT, margins
  - [x] PDF generation (client and internal versions)
  - [x] Reports: Pipeline, Conversion, Status, Company, Win/Loss, Monthly Trends
  - [x] Convert to Stock Order (won proposals)
- [x] **Proposals Frontend:**
  - [x] Product Kits admin pages
  - [x] Proposals list with filters and status badges
  - [x] Proposal form (create/edit)
  - [x] Proposal view with sections, items, contacts, workflow actions
  - [x] Convert to Stock Order dialog with preview
  - [x] Analytics dashboard with charts
- [x] **Site Attendance module:**
  - [x] GPS-based entry/exit tracking (automatic and manual)
  - [x] Time on site calculation with noise filtering
  - [x] Daily attendance processing (Hangfire background job)
  - [x] Executive dashboard with KPIs
  - [x] Employee performance breakdown
  - [x] Configurable working days (Saturday/Sunday inclusion)
  - [x] Bank holiday management
  - [x] SPA (Site Photo Attendance) compliance
  - [x] Device registration for push notifications
  - [x] Batch event submission (offline sync support)
- [x] **Site Attendance Frontend:**
  - [x] Executive dashboard with KPI cards and performance table
  - [x] Attendance events list with filters
  - [x] Attendance summaries view
  - [x] Bank holidays management
  - [x] Settings configuration page

### Pending / Future
- [ ] Seed data for Proposals module
- [ ] End-to-end testing for Proposals
- [ ] RAMS module (Risk Assessment Method Statements)
- [ ] Toolbox Talks module
- [ ] PDF/Excel export for reports (GRN, PO)
- [ ] Low Stock Report (UI exists as placeholder)
- [ ] Stock Movement Report (UI exists as placeholder)
- [ ] Email notifications (order approvals, low stock alerts)
- [ ] Mobile app (MAUI)
- [ ] Advanced analytics (trends, forecasting)
- [ ] Docker containerization
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Production deployment

---

## Coding Conventions

### C# / .NET
- Use `record` types for DTOs
- Async all the way - suffix with `Async`
- Use primary constructors for simple classes
- Nullable reference types enabled
- File-scoped namespaces

### Entity Framework
- Configure entities in separate `EntityConfiguration` classes
- Use `HasQueryFilter` for tenant and soft delete filtering
- Migrations named descriptively

### Frontend
- Use Server Components by default, Client Components when needed
- API calls through TanStack Query with cache invalidation
- Forms with React Hook Form + Zod
- Toast notifications with Sonner

---

## Notes for Claude Code

1. **Multi-tenancy is critical** - every query filters by TenantId
2. **Soft deletes everywhere** - set `IsDeleted = true`, never hard delete
3. **Audit fields are automatic** - SaveChanges override handles CreatedAt/UpdatedAt
4. **Permission-based auth** - use `[Authorize(Policy = "Permission.Name")]`
5. **Single DbContext** - ApplicationDbContext implements both ICoreDbContext and IStockManagementDbContext
6. **Keep controllers thin** - business logic in Application layer services
7. **Follow established patterns** - check existing code before creating new
8. **Bay Locations** - Optional granular tracking within stock locations (Aisle A, Shelf 1, Bin 3)
9. **Product fields** - CostPrice and SellPrice added; legacy BaseRate field remains for backward compatibility
10. **QR code stocktaking** - Use qrcode.react for generating count sheet QR codes

---

## Database Migrations

**Migration History:**
1. `20251216112816_InitialModularMonolith` - Initial schema
2. `20251216125141_AddIdentityAndPermissions` - ASP.NET Identity integration
3. `20251216194530_AddTenantEntity` - Multi-tenancy support
4. `20251219095609_AddMissingFields` - Product pricing (CostPrice, SellPrice, ProductType), GRN enhancements (batch, expiry, rejection tracking), Stocktake variance reasons
5. `20251219125924_AddBayLocations` - BayLocation entity and integration with StockLevel, GoodsReceiptLine, StocktakeLine
6. `20251221215252_InitialSiteAttendance` - Site Attendance module (AttendanceEvent, AttendanceSummary, SitePhotoAttendance, DeviceRegistration, BankHoliday, AttendanceSettings, AttendanceNotification)

---

## Stock Management Entities (15 Total)

1. **Category** - Product categories
2. **Product** - Products with pricing, reorder levels, product type
3. **Supplier** - Supplier companies
4. **StockLocation** - Warehouses and site stores
5. **BayLocation** - Sub-locations within stock locations (bins, shelves, aisles)
6. **StockLevel** - Current stock quantities by product/location/bay
7. **StockOrder** - Site orders for stock
8. **StockOrderLine** - Line items for stock orders
9. **PurchaseOrder** - Purchase orders to suppliers
10. **PurchaseOrderLine** - Line items for purchase orders
11. **GoodsReceipt** - Goods received notes (GRN)
12. **GoodsReceiptLine** - Line items for GRNs with batch/expiry/bay tracking
13. **Stocktake** - Stocktake sessions
14. **StocktakeLine** - Count records with variance reasons and bay location snapshot
15. **StockTransaction** - Audit trail of all stock movements

**Key Field Additions:**

**Product:**
- `CostPrice` (decimal?) - Purchase cost per unit
- `SellPrice` (decimal?) - Selling price per unit
- `ProductType` (string?) - Main Product, Ancillary Product, Tool, Consumable

**GoodsReceiptLine:**
- `BayLocationId` (Guid?) - Where goods are stored
- `BatchNumber` (string?) - Batch/lot tracking
- `ExpiryDate` (DateTime?) - For perishable items
- `QuantityRejected` (decimal) - Rejected quantity
- `RejectionReason` (string?) - Damaged, Wrong Item, Expired, Quality Issue, Other

**StocktakeLine:**
- `BayLocationId` (Guid?) - Bay at time of count
- `BayCode` (string?) - Denormalized for display
- `VarianceReason` (string?) - Damaged, Missing, Found, Data Entry Error, Theft, Other

**GoodsReceipt:**
- `DeliveryNoteRef` (string?) - Supplier delivery note reference

**StockLevel:**
- `BayLocationId` (Guid?) - Primary bay for this product (replaces legacy BinLocation string)

---

## Proposals Module Entities (7 Total)

1. **Proposal** - Main proposal header with company, site, status, totals
2. **ProposalSection** - Groups of line items within a proposal
3. **ProposalLineItem** - Individual items (product-linked or ad-hoc)
4. **ProposalContact** - Client contacts associated with proposal
5. **ProductKit** - Pre-defined bundles for quick section creation
6. **ProductKitItem** - Products and quantities within a kit

**Key Entity Fields:**

**Proposal:**
- `Reference` (string) - Auto-generated reference (PRO-001, PRO-002, etc.)
- `Version` (int) - Revision number (1, 2, 3, etc.)
- `ParentProposalId` (Guid?) - Links to original proposal for revisions
- `Status` (ProposalStatus) - Draft, PendingApproval, Approved, Rejected, Won, Lost, Cancelled
- `CompanyId` (Guid) - Client company
- `SiteId` (Guid?) - Optional site reference
- `ProjectName` (string?) - Project/job name
- `ValidUntil` (DateTime?) - Proposal validity date
- `SubTotal` (decimal) - Sum of line items
- `VatRate` (decimal) - VAT percentage (default 23%)
- `VatAmount` (decimal) - Calculated VAT
- `GrandTotal` (decimal) - SubTotal + VatAmount
- `TotalCost` (decimal) - Sum of cost prices (for margin calculation)
- `MarginPercentage` (decimal) - Calculated profit margin
- `WonDate` / `LostDate` (DateTime?) - Outcome dates
- `LostReason` (string?) - Reason if lost

**ProposalSection:**
- `Name` (string) - Section name
- `Description` (string?) - Optional description
- `SortOrder` (int) - Display order
- `ProductKitId` (Guid?) - Source kit if expanded from kit

**ProposalLineItem:**
- `ProductId` (Guid?) - Optional link to product
- `Description` (string) - Item description
- `Quantity` (decimal) - Quantity
- `UnitPrice` (decimal) - Price per unit
- `CostPrice` (decimal) - Cost per unit (from product or manual)
- `DiscountPercentage` (decimal) - Line discount
- `LineTotal` (decimal) - Calculated total
- `SortOrder` (int) - Display order

**ProposalContact:**
- `ContactId` (Guid) - Link to Contact entity
- `Role` (string?) - Decision Maker, Technical Contact, etc.
- `IsPrimary` (bool) - Primary contact flag

**ProductKit:**
- `Name` (string) - Kit name
- `Description` (string?) - Kit description
- `IsActive` (bool) - Active/inactive status

**ProductKitItem:**
- `ProductId` (Guid) - Link to product
- `Quantity` (decimal) - Default quantity
- `SortOrder` (int) - Display order

---

## Proposals Module Features Summary

### Proposals Features
- **Create proposals** for clients with project details
- **Sections** to group related items (can expand from Product Kits)
- **Line items** linked to products or ad-hoc entries
- **Multiple contacts** per proposal with roles
- **Pricing** with discount %, VAT calculation, margin tracking
- **Workflow** from Draft to Won/Lost with approval steps
- **Versioning** to create revisions while preserving history
- **PDF generation** with professional template (client and internal versions)
- **Analytics** dashboard with pipeline, conversion rates, trends
- **Convert to Stock Order** for won proposals

### Product Kits Features
- **Pre-defined bundles** of related products
- **Default quantities** per kit item
- **Quick section creation** - expand kit into proposal section
- **Total cost/price** calculated from current product prices

---

## Site Attendance Module Entities (7 Total)

1. **AttendanceEvent** - Raw GPS entry/exit events from mobile devices
2. **AttendanceSummary** - Daily aggregated attendance per employee per site
3. **SitePhotoAttendance** - SPA/RAMS compliance photo records
4. **DeviceRegistration** - Registered mobile devices for GPS tracking and push notifications
5. **BankHoliday** - Excluded dates from working day calculations
6. **AttendanceSettings** - Tenant-level attendance configuration
7. **AttendanceNotification** - Notification log for attendance-related alerts

**Key Entity Fields:**

**AttendanceEvent:**
- `EmployeeId` (Guid) - Employee who triggered the event
- `SiteId` (Guid) - Site where the event occurred
- `EventType` (EventType) - Enter or Exit
- `Timestamp` (DateTime) - When the event occurred
- `Latitude` / `Longitude` (decimal?) - GPS coordinates
- `TriggerMethod` (TriggerMethod) - Automatic (GPS geofence) or Manual
- `DeviceRegistrationId` (Guid?) - Device that recorded the event
- `IsNoise` (bool) - Flagged as noise (false positive from GPS jitter)
- `NoiseDistance` (decimal?) - Distance threshold that flagged as noise
- `Processed` (bool) - Has been processed into summary

**AttendanceSummary:**
- `EmployeeId` (Guid) - Employee
- `SiteId` (Guid) - Site
- `Date` (DateOnly) - Date of the summary
- `FirstEntry` (DateTime?) - First entry timestamp
- `LastExit` (DateTime?) - Last exit timestamp
- `TimeOnSiteMinutes` (int) - Total time on site in minutes
- `ExpectedHours` (decimal) - Expected working hours (from settings)
- `UtilizationPercent` (decimal) - (ActualHours / ExpectedHours) * 100
- `Status` (AttendanceStatus) - Excellent, Good, BelowTarget, Absent, Incomplete
- `EntryCount` / `ExitCount` (int) - Number of entry/exit events
- `HasSpa` (bool) - Whether SPA record exists for this day

**SitePhotoAttendance:**
- `EmployeeId` (Guid) - Employee
- `SiteId` (Guid) - Site
- `EventDate` (DateOnly) - Date of attendance
- `WeatherConditions` (string?) - Weather notes
- `ImageUrl` (string?) - URL to uploaded photo
- `DistanceToSite` (decimal?) - Distance from site when photo taken
- `Latitude` / `Longitude` (decimal?) - GPS coordinates
- `Notes` (string?) - Additional notes

**DeviceRegistration:**
- `EmployeeId` (Guid?) - Assigned employee (optional)
- `DeviceIdentifier` (string) - Unique device ID
- `DeviceName` (string?) - Friendly device name
- `Platform` (string?) - iOS, Android
- `PushToken` (string?) - Push notification token
- `RegisteredAt` (DateTime) - Registration timestamp
- `LastActiveAt` (DateTime?) - Last activity
- `IsActive` (bool) - Active status

**BankHoliday:**
- `Date` (DateOnly) - Holiday date
- `Name` (string?) - Holiday name (Christmas, St. Patrick's Day, etc.)

**AttendanceSettings:**
- `ExpectedHoursPerDay` (decimal) - Target hours (default 7.5)
- `WorkStartTime` (TimeOnly) - Work start time (default 08:00)
- `LateThresholdMinutes` (int) - Minutes after start to be considered late (default 30)
- `IncludeSaturday` / `IncludeSunday` (bool) - Include in working days
- `GeofenceRadiusMeters` (int) - Site boundary radius (default 100)
- `NoiseThresholdMeters` (int) - Distance for noise filtering (default 150)
- `SpaGracePeriodMinutes` (int) - Grace period for SPA submission (default 5)
- `EnablePushNotifications` / `EnableEmailNotifications` / `EnableSmsNotifications` (bool)
- `NotificationTitle` / `NotificationMessage` (string) - Notification templates

**AttendanceNotification:**
- `EmployeeId` (Guid) - Recipient employee
- `NotificationType` (NotificationType) - Push, Email, Sms
- `Reason` (NotificationReason) - MissingSpa, LateArrival, EarlyDeparture, NoCheckOut, DeviceRegistered
- `Message` (string?) - Notification content
- `SentAt` (DateTime) - When sent
- `Delivered` (bool) - Delivery status
- `ErrorMessage` (string?) - Error if failed
- `RelatedEventId` (Guid?) - Related attendance event

---

## Site Attendance Module Features Summary

### Core Features
- **GPS-based tracking** - Automatic entry/exit detection using geofencing
- **Manual check-in/out** - Alternative for poor GPS reception areas
- **Time calculation** - Accurate time on site with noise filtering
- **Daily processing** - Hangfire job runs at 1:00 AM to process previous day
- **Multi-tenant** - Full tenant isolation for all data

### Dashboard & Reporting
- **Executive KPIs** - Overall utilization, variance, employee counts by status
- **Employee performance** - Individual breakdown with hours, utilization, variance
- **Site filtering** - View data for specific sites

### Configuration
- **Working hours** - Configurable expected hours per day
- **Working days** - Include/exclude Saturday and Sunday
- **Bank holidays** - Exclude from working day calculations
- **Geofence settings** - Configurable radius for site boundary
- **Noise filtering** - Configurable threshold to filter GPS jitter

### Compliance
- **SPA records** - Photo proof of site attendance
- **Image upload** - Store compliance photos
- **GPS verification** - Distance from site recorded

### Mobile Support
- **Device registration** - Register devices for push notifications
- **Offline sync** - Batch event submission for offline operation
- **Push notifications** - Alerts for missing SPA, late arrival, etc.

---

## RAMS AI Features

### AI-Powered Control Measure Suggestions

The RAMS module includes AI-powered control measure suggestions to assist safety officers in creating comprehensive risk assessments.

**Backend Components:**
- **McpAuditLog Entity** - Tracks all AI requests, responses, and acceptance rates for analytics
- **IRamsAiService** - Service interface for AI suggestions
- **RamsAiService** - Implementation that:
  1. Searches the library for matching hazards based on keywords
  2. Searches for relevant control measures from the library
  3. Searches for relevant legislation references
  4. Searches for relevant SOPs
  5. Optionally calls Claude API for AI-generated suggestions when library matches are sparse
  6. Logs all requests for usage tracking and improvement

**API Endpoints:**
- `POST /api/rams/ai/suggest-controls` - Get suggestions based on task/hazard
- `POST /api/rams/ai/accept-suggestion` - Mark suggestion as accepted/rejected

**Configuration:**
```json
{
  "Anthropic": {
    "ApiKey": ""  // Set via environment variable or secrets
  },
  "Rams": {
    "AiEnabled": true  // Toggle AI features on/off
  }
}
```

**Frontend Hooks:**
- `useSuggestControls()` - Mutation hook to request AI suggestions
- `useAcceptSuggestion()` - Mutation hook to mark suggestions as accepted

---

*Last Updated: January 10, 2026 (RAMS AI Suggestions Added)*
*Architecture: Modular Monolith with Clean Architecture*
