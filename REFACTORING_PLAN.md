# ApplicationDbContext Refactoring Plan

## Move from StockManagement.Infrastructure to Core.Infrastructure

---

## 1. Target Architecture

### Current State (Problematic)
```
                    ┌─────────────────────────────────────────────────────────┐
                    │       StockManagement.Infrastructure                     │
                    │         (Contains ApplicationDbContext)                  │
                    │                                                         │
                    │  References ALL module Infrastructures:                 │
                    │  - Proposals.Infrastructure                             │
                    │  - ToolboxTalks.Infrastructure                          │
                    │  - Rams.Infrastructure                                  │
                    └─────────────────────────────────────────────────────────┘
                                           ▲
                                           │
              ┌────────────────────────────┼────────────────────────────┐
              │                            │                            │
    Proposals.Infra              ToolboxTalks.Infra              Rams.Infra
         │                              │                            │
         └──────────────────────────────┼────────────────────────────┘
                                        │
                                        ▼
                              Core.Infrastructure
                        (References module Domains ⚠️)
```

### Target State (Clean)
```
                              ┌──────────────────────────────────────┐
                              │       Core.Infrastructure            │
                              │     (Contains ApplicationDbContext)  │
                              │                                      │
                              │  References ALL module Applications  │
                              │  & Infrastructures (for configs)     │
                              └──────────────────────────────────────┘
                                               ▲
                                               │
       ┌───────────────────┬───────────────────┼───────────────────┬───────────────────┐
       │                   │                   │                   │                   │
       ▼                   ▼                   ▼                   ▼                   ▼
  StockMgmt.Infra    Proposals.Infra    ToolboxTalks.Infra    Rams.Infra    SiteAttendance.Infra
       │                   │                   │                   │                   │
       │                   │                   │                   │                   │
       └───────────────────┴───────────────────┴───────────────────┴───────────────────┘
                                               │
                                               ▼
                                        No cross-module
                                          references
```

---

## 2. File Movement Plan

### a) Files to MOVE

| Source | Destination |
|--------|-------------|
| `src/Modules/StockManagement/Rascor.Modules.StockManagement.Infrastructure/Data/ApplicationDbContext.cs` | `src/Core/Rascor.Core.Infrastructure/Data/ApplicationDbContext.cs` |

### b) Migrations to MOVE (18 migration pairs + 1 snapshot = 35 files)

| # | Migration File | Action |
|---|----------------|--------|
| 1 | `20251216112816_InitialModularMonolith.cs` | MOVE |
| 2 | `20251216112816_InitialModularMonolith.Designer.cs` | MOVE |
| 3 | `20251216125141_AddIdentityAndPermissions.cs` | MOVE |
| 4 | `20251216125141_AddIdentityAndPermissions.Designer.cs` | MOVE |
| 5 | `20251216194530_AddTenantEntity.cs` | MOVE |
| 6 | `20251216194530_AddTenantEntity.Designer.cs` | MOVE |
| 7 | `20251219095609_AddMissingFields.cs` | MOVE |
| 8 | `20251219095609_AddMissingFields.Designer.cs` | MOVE |
| 9 | `20251219125924_AddBayLocations.cs` | MOVE |
| 10 | `20251219125924_AddBayLocations.Designer.cs` | MOVE |
| 11 | `20251219200722_AddSourceLocationToStockOrder.cs` | MOVE |
| 12 | `20251219200722_AddSourceLocationToStockOrder.Designer.cs` | MOVE |
| 13 | `20251219205318_AddProductImage.cs` | MOVE |
| 14 | `20251219205318_AddProductImage.Designer.cs` | MOVE |
| 15 | `20251219222400_InitialProposalsModule.cs` | MOVE |
| 16 | `20251219222400_InitialProposalsModule.Designer.cs` | MOVE |
| 17 | `20251219223003_AddProductKits.cs` | MOVE |
| 18 | `20251219223003_AddProductKits.Designer.cs` | MOVE |
| 19 | `20251220175806_AddSourceProposalToStockOrder.cs` | MOVE |
| 20 | `20251220175806_AddSourceProposalToStockOrder.Designer.cs` | MOVE |
| 21 | `20251221221901_AddSiteGeolocationFields.cs` | MOVE |
| 22 | `20251221221901_AddSiteGeolocationFields.Designer.cs` | MOVE |
| 23 | `20260105111408_AddToolboxTalksModule.cs` | MOVE |
| 24 | `20260105111408_AddToolboxTalksModule.Designer.cs` | MOVE |
| 25 | `20260105182034_ToolboxTalksModelFixes.cs` | MOVE |
| 26 | `20260105182034_ToolboxTalksModelFixes.Designer.cs` | MOVE |
| 27 | `20260110192714_AddRamsNotificationLog.cs` | MOVE |
| 28 | `20260110192714_AddRamsNotificationLog.Designer.cs` | MOVE |
| 29 | `20260112140142_AddGeoTrackerIDToEmployee.cs` | MOVE |
| 30 | `20260112140142_AddGeoTrackerIDToEmployee.Designer.cs` | MOVE |
| 31 | `20260115175251_AddPurchaseOrderLineUnitType.cs` | MOVE |
| 32 | `20260115175251_AddPurchaseOrderLineUnitType.Designer.cs` | MOVE |
| 33 | `20260116203306_AddSubtitleProcessing.cs` | MOVE |
| 34 | `20260116203306_AddSubtitleProcessing.Designer.cs` | MOVE |
| 35 | `ApplicationDbContextModelSnapshot.cs` | MOVE |

**Source folder:** `src/Modules/StockManagement/Rascor.Modules.StockManagement.Infrastructure/Migrations/`
**Destination folder:** `src/Core/Rascor.Core.Infrastructure/Migrations/` (CREATE)

### c) Files/Folders to DELETE after verification
- `src/Modules/StockManagement/Rascor.Modules.StockManagement.Infrastructure/Migrations/` (entire folder after move)
- `src/Modules/StockManagement/Rascor.Modules.StockManagement.Infrastructure/Data/Migrations/` (empty folder)

---

## 3. Core.Infrastructure.csproj Changes

### Current State
```xml
<ItemGroup>
  <ProjectReference Include="..\Rascor.Core.Application\Rascor.Core.Application.csproj" />
  <ProjectReference Include="..\..\Modules\StockManagement\Rascor.Modules.StockManagement.Domain\Rascor.Modules.StockManagement.Domain.csproj" />
  <ProjectReference Include="..\..\Modules\Proposals\Rascor.Modules.Proposals.Domain\Rascor.Modules.Proposals.Domain.csproj" />
</ItemGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.*" />
  <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.*" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.*">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.*" />
</ItemGroup>
```

### Target State
```xml
<ItemGroup>
  <!-- Core Application (keep) -->
  <ProjectReference Include="..\Rascor.Core.Application\Rascor.Core.Application.csproj" />

  <!-- Module Applications (for DbContext interfaces) -->
  <ProjectReference Include="..\..\Modules\StockManagement\Rascor.Modules.StockManagement.Application\Rascor.Modules.StockManagement.Application.csproj" />
  <ProjectReference Include="..\..\Modules\Proposals\Rascor.Modules.Proposals.Application\Rascor.Modules.Proposals.Application.csproj" />
  <ProjectReference Include="..\..\Modules\ToolboxTalks\Rascor.Modules.ToolboxTalks.Application\Rascor.Modules.ToolboxTalks.Application.csproj" />
  <ProjectReference Include="..\..\Modules\Rams\Rascor.Modules.Rams.Application\Rascor.Modules.Rams.Application.csproj" />

  <!-- Module Infrastructures (for entity configurations) -->
  <ProjectReference Include="..\..\Modules\StockManagement\Rascor.Modules.StockManagement.Infrastructure\Rascor.Modules.StockManagement.Infrastructure.csproj" />
  <ProjectReference Include="..\..\Modules\Proposals\Rascor.Modules.Proposals.Infrastructure\Rascor.Modules.Proposals.Infrastructure.csproj" />
  <ProjectReference Include="..\..\Modules\ToolboxTalks\Rascor.Modules.ToolboxTalks.Infrastructure\Rascor.Modules.ToolboxTalks.Infrastructure.csproj" />
  <ProjectReference Include="..\..\Modules\Rams\Rascor.Modules.Rams.Infrastructure\Rascor.Modules.Rams.Infrastructure.csproj" />
</ItemGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.*" />
  <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.*" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.*">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.*" />
</ItemGroup>
```

**Changes Summary:**
- REMOVE: `Rascor.Modules.StockManagement.Domain` reference
- REMOVE: `Rascor.Modules.Proposals.Domain` reference
- ADD: 4 Module Application references (for interfaces)
- ADD: 4 Module Infrastructure references (for entity configurations)

---

## 4. StockManagement.Infrastructure.csproj Changes

### Current State
```xml
<ItemGroup>
  <ProjectReference Include="..\Rascor.Modules.StockManagement.Application\Rascor.Modules.StockManagement.Application.csproj" />
  <ProjectReference Include="..\..\..\Core\Rascor.Core.Infrastructure\Rascor.Core.Infrastructure.csproj" />
  <ProjectReference Include="..\..\Proposals\Rascor.Modules.Proposals.Infrastructure\Rascor.Modules.Proposals.Infrastructure.csproj" />
  <ProjectReference Include="..\..\ToolboxTalks\Rascor.Modules.ToolboxTalks.Application\Rascor.Modules.ToolboxTalks.Application.csproj" />
  <ProjectReference Include="..\..\ToolboxTalks\Rascor.Modules.ToolboxTalks.Infrastructure\Rascor.Modules.ToolboxTalks.Infrastructure.csproj" />
  <ProjectReference Include="..\..\Rams\Rascor.Modules.Rams.Application\Rascor.Modules.Rams.Application.csproj" />
  <ProjectReference Include="..\..\Rams\Rascor.Modules.Rams.Infrastructure\Rascor.Modules.Rams.Infrastructure.csproj" />
</ItemGroup>
```

### Target State
```xml
<ItemGroup>
  <ProjectReference Include="..\Rascor.Modules.StockManagement.Application\Rascor.Modules.StockManagement.Application.csproj" />
  <!-- Core.Infrastructure now contains ApplicationDbContext -->
  <!-- This module only needs its own Application layer -->
</ItemGroup>
```

**Changes Summary:**
- REMOVE: `Rascor.Core.Infrastructure` reference (will cause circular ref)
- REMOVE: `Rascor.Modules.Proposals.Infrastructure` reference
- REMOVE: `Rascor.Modules.ToolboxTalks.Application` reference
- REMOVE: `Rascor.Modules.ToolboxTalks.Infrastructure` reference
- REMOVE: `Rascor.Modules.Rams.Application` reference
- REMOVE: `Rascor.Modules.Rams.Infrastructure` reference
- KEEP: `Rascor.Modules.StockManagement.Application` reference

---

## 5. Other Module.Infrastructure.csproj Changes

### Proposals.Infrastructure (NO CHANGES NEEDED)
```xml
<!-- Current - Already correct -->
<ItemGroup>
  <ProjectReference Include="..\Rascor.Modules.Proposals.Application\Rascor.Modules.Proposals.Application.csproj" />
  <ProjectReference Include="..\..\..\Core\Rascor.Core.Infrastructure\Rascor.Core.Infrastructure.csproj" />
</ItemGroup>
```
**Note:** Will need to REMOVE Core.Infrastructure reference to avoid circular dependency.

### ToolboxTalks.Infrastructure (NO CHANGES NEEDED)
```xml
<!-- Current - Already correct -->
<ItemGroup>
  <ProjectReference Include="..\Rascor.Modules.ToolboxTalks.Application\Rascor.Modules.ToolboxTalks.Application.csproj" />
  <ProjectReference Include="..\Rascor.Modules.ToolboxTalks.Domain\Rascor.Modules.ToolboxTalks.Domain.csproj" />
  <ProjectReference Include="..\..\..\Core\Rascor.Core.Infrastructure\Rascor.Core.Infrastructure.csproj" />
</ItemGroup>
```
**Note:** Will need to REMOVE Core.Infrastructure reference to avoid circular dependency.

### Rams.Infrastructure (NO CHANGES NEEDED)
```xml
<!-- Current - Already correct -->
<ItemGroup>
  <ProjectReference Include="..\Rascor.Modules.Rams.Application\Rascor.Modules.Rams.Application.csproj" />
  <ProjectReference Include="..\Rascor.Modules.Rams.Domain\Rascor.Modules.Rams.Domain.csproj" />
  <ProjectReference Include="..\..\..\Core\Rascor.Core.Infrastructure\Rascor.Core.Infrastructure.csproj" />
</ItemGroup>
```
**Note:** Will need to REMOVE Core.Infrastructure reference to avoid circular dependency.

---

## 6. CRITICAL: Circular Dependency Resolution

### The Problem
If Core.Infrastructure references Module.Infrastructures, and Module.Infrastructures reference Core.Infrastructure, we have a **circular dependency**.

### The Solution: Two-Phase Architecture

**Phase 1: Core.Infrastructure references Module Infrastructures**
- Core.Infrastructure will reference ALL module Infrastructure projects
- Module Infrastructure projects will NOT reference Core.Infrastructure

**Phase 2: Update Module Infrastructures**
Each module Infrastructure project needs:
```xml
<ItemGroup>
  <ProjectReference Include="..\Module.Application\Module.Application.csproj" />
  <ProjectReference Include="..\Module.Domain\Module.Domain.csproj" />
  <!-- NO reference to Core.Infrastructure -->
</ItemGroup>
```

### Updated Module.Infrastructure References

#### StockManagement.Infrastructure
```xml
<ItemGroup>
  <ProjectReference Include="..\Rascor.Modules.StockManagement.Application\Rascor.Modules.StockManagement.Application.csproj" />
</ItemGroup>
```

#### Proposals.Infrastructure
```xml
<ItemGroup>
  <ProjectReference Include="..\Rascor.Modules.Proposals.Application\Rascor.Modules.Proposals.Application.csproj" />
</ItemGroup>
```

#### ToolboxTalks.Infrastructure
```xml
<ItemGroup>
  <ProjectReference Include="..\Rascor.Modules.ToolboxTalks.Application\Rascor.Modules.ToolboxTalks.Application.csproj" />
  <ProjectReference Include="..\Rascor.Modules.ToolboxTalks.Domain\Rascor.Modules.ToolboxTalks.Domain.csproj" />
</ItemGroup>
```

#### Rams.Infrastructure
```xml
<ItemGroup>
  <ProjectReference Include="..\Rascor.Modules.Rams.Application\Rascor.Modules.Rams.Application.csproj" />
  <ProjectReference Include="..\Rascor.Modules.Rams.Domain\Rascor.Modules.Rams.Domain.csproj" />
</ItemGroup>
```

---

## 7. Namespace Changes

### ApplicationDbContext.cs
```diff
- namespace Rascor.Modules.StockManagement.Infrastructure.Data;
+ namespace Rascor.Core.Infrastructure.Data;
```

### All Migration Files (35 files)
```diff
- namespace Rascor.Modules.StockManagement.Infrastructure.Migrations
+ namespace Rascor.Core.Infrastructure.Migrations
```

Each `.Designer.cs` file also contains:
```diff
- [DbContext(typeof(Rascor.Modules.StockManagement.Infrastructure.Data.ApplicationDbContext))]
+ [DbContext(typeof(Rascor.Core.Infrastructure.Data.ApplicationDbContext))]
```

---

## 8. Using Statement Updates

### Files That Need Updates

| File | Change Required |
|------|-----------------|
| `src/Rascor.API/Program.cs` (Line 13) | `using Rascor.Modules.StockManagement.Infrastructure.Data;` → `using Rascor.Core.Infrastructure.Data;` |
| All 18 migration `.Designer.cs` files | Update DbContext attribute and using statements |
| `ApplicationDbContextModelSnapshot.cs` | Update namespace and DbContext attribute |

### Program.cs Changes (Line 13)
```diff
- using Rascor.Modules.StockManagement.Infrastructure.Data;
+ using Rascor.Core.Infrastructure.Data;
```

---

## 9. Entity Configuration Loading Analysis

### Current OnModelCreating Approach (ApplicationDbContext.cs lines 111-283)

The current configuration loading uses **three methods**:

1. **Explicit ApplyConfiguration calls for Core entities** (lines 201-205):
   ```csharp
   modelBuilder.ApplyConfiguration(new TenantConfiguration());
   modelBuilder.ApplyConfiguration(new SiteConfiguration());
   modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
   modelBuilder.ApplyConfiguration(new CompanyConfiguration());
   modelBuilder.ApplyConfiguration(new ContactConfiguration());
   ```
   **Location:** `Rascor.Core.Infrastructure/Data/Configurations/`
   **Status:** ✅ Will work - same project after move

2. **Assembly scanning for StockManagement configs** (line 208):
   ```csharp
   modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
   ```
   **Current behavior:** Loads all IEntityTypeConfiguration from StockManagement.Infrastructure assembly
   **After move:** ⚠️ Will load from Core.Infrastructure assembly - NEED TO FIX

3. **Explicit ApplyConfiguration calls for ToolboxTalks** (lines 211-224):
   ```csharp
   modelBuilder.ApplyConfiguration(new ToolboxTalkConfiguration());
   // ... 13 more configurations
   ```
   **Location:** `Rascor.Modules.ToolboxTalks.Infrastructure/Persistence/Configurations/`
   **Status:** ✅ Will work - explicit imports

4. **Explicit ApplyConfiguration calls for RAMS** (lines 227-236):
   ```csharp
   modelBuilder.ApplyConfiguration(new RamsDocumentConfiguration());
   // ... 9 more configurations
   ```
   **Location:** `Rascor.Modules.Rams.Infrastructure/Configurations/`
   **Status:** ✅ Will work - explicit imports

### REQUIRED FIX: Update Assembly Scanning

After the move, line 208 needs to change to explicitly load from all module assemblies:

```csharp
// Apply Stock Management entity configurations
modelBuilder.ApplyConfigurationsFromAssembly(
    typeof(Rascor.Modules.StockManagement.Infrastructure.Data.Configurations.CategoryConfiguration).Assembly);

// Apply Proposals entity configurations (currently not loaded - needs adding)
modelBuilder.ApplyConfigurationsFromAssembly(
    typeof(Rascor.Modules.Proposals.Infrastructure.Data.Configurations.ProposalConfiguration).Assembly);
```

### Entity Configurations by Module

| Module | Configuration Location | Count | Loading Method |
|--------|------------------------|-------|----------------|
| Core | `Core.Infrastructure/Data/Configurations/` | 5 | Explicit |
| StockManagement | `StockManagement.Infrastructure/Data/Configurations/` | 16 | Assembly Scan |
| StockManagement | `StockManagement.Infrastructure/Persistence/Configurations/` | 2 | Assembly Scan |
| Proposals | `Proposals.Infrastructure/Data/Configurations/` | 4 | **NOT LOADED** ⚠️ |
| ToolboxTalks | `ToolboxTalks.Infrastructure/Persistence/Configurations/` | 14 | Explicit |
| Rams | `Rams.Infrastructure/Configurations/` | 10 | Explicit |
| SiteAttendance | `SiteAttendance.Infrastructure/Persistence/Configurations/` | 8 | N/A (separate DbContext) |

**Finding:** Proposals configurations are currently NOT being loaded! This is a pre-existing bug.

---

## 10. EF Core Migration Considerations

### Migration History Table
- Table: `__EFMigrationsHistory`
- Stores migration IDs (timestamps + names)
- The migration IDs will NOT change - they're stored as string values

### After Moving - Verify Migrations
```bash
cd src/Rascor.API
dotnet ef migrations list --project ../Core/Rascor.Core.Infrastructure
```

Expected output: All 18 migrations should be listed.

### Future Migrations Command
```bash
# Old command (won't work after move)
dotnet ef migrations add NewMigration --project ../Modules/StockManagement/Rascor.Modules.StockManagement.Infrastructure

# New command
dotnet ef migrations add NewMigration --project ../Core/Rascor.Core.Infrastructure
```

---

## 11. Execution Order (Step-by-Step)

### Phase 1: Preparation
| Step | Action | Verify |
|------|--------|--------|
| 1.1 | Create git commit/backup point | `git status` shows clean state |
| 1.2 | Ensure solution builds | `dotnet build` succeeds |
| 1.3 | Create `Migrations/` folder in Core.Infrastructure | Folder exists |
| 1.4 | Create `Data/` folder in Core.Infrastructure if missing | Folder exists |

### **STOP POINT 1: Backup Verified**

### Phase 2: Move Files
| Step | Action | Verify |
|------|--------|--------|
| 2.1 | Move `ApplicationDbContext.cs` to Core.Infrastructure/Data/ | File exists in new location |
| 2.2 | Move all 35 migration files to Core.Infrastructure/Migrations/ | All files exist |
| 2.3 | Update namespace in ApplicationDbContext.cs | Namespace is `Rascor.Core.Infrastructure.Data` |
| 2.4 | Update namespaces in all migration files | Namespace is `Rascor.Core.Infrastructure.Migrations` |
| 2.5 | Update DbContext attributes in .Designer.cs files | All reference new namespace |

### **STOP POINT 2: Files Moved and Namespaces Updated**

### Phase 3: Update Project References
| Step | Action | Verify |
|------|--------|--------|
| 3.1 | Update Core.Infrastructure.csproj | Has all module Application + Infrastructure refs |
| 3.2 | Update StockManagement.Infrastructure.csproj | Only has StockManagement.Application ref |
| 3.3 | Update Proposals.Infrastructure.csproj | Remove Core.Infrastructure ref |
| 3.4 | Update ToolboxTalks.Infrastructure.csproj | Remove Core.Infrastructure ref |
| 3.5 | Update Rams.Infrastructure.csproj | Remove Core.Infrastructure ref |

### **STOP POINT 3: Project References Updated**

### Phase 4: Update Code References
| Step | Action | Verify |
|------|--------|--------|
| 4.1 | Update Program.cs using statement | Uses `Rascor.Core.Infrastructure.Data` |
| 4.2 | Update ApplicationDbContext.cs assembly scanning | Loads configs from all module assemblies |
| 4.3 | Add Proposals configurations loading | ProposalConfiguration etc. are loaded |

### **STOP POINT 4: Code References Updated**

### Phase 5: Build and Test
| Step | Action | Verify |
|------|--------|--------|
| 5.1 | Build solution | `dotnet build` succeeds with no errors |
| 5.2 | List migrations | `dotnet ef migrations list` shows all 18 |
| 5.3 | Run application | Application starts without errors |
| 5.4 | Test database operations | CRUD operations work |

### **STOP POINT 5: Build Verified**

### Phase 6: Cleanup
| Step | Action | Verify |
|------|--------|--------|
| 6.1 | Delete old Migrations folder in StockManagement.Infrastructure | Folder removed |
| 6.2 | Delete empty Data/Migrations folder | Folder removed |
| 6.3 | Final build verification | Solution builds clean |
| 6.4 | Git commit with meaningful message | Changes committed |

---

## 12. Risk Assessment

### High Risk
| Risk | Mitigation |
|------|------------|
| **Circular dependency after move** | Carefully remove Core.Infrastructure refs from module Infrastructures BEFORE adding reverse refs |
| **Missing entity configurations** | Explicitly load each module's configurations; fix pre-existing Proposals bug |
| **Migration namespace mismatch** | EF uses MigrationId (timestamp+name), not namespace - should be safe |

### Medium Risk
| Risk | Mitigation |
|------|------------|
| **Build order issues** | Update all .csproj files in correct order |
| **Runtime DI failures** | Test DbContext interface resolution after changes |
| **Missing using statements** | Use IDE to find all compilation errors |

### Low Risk
| Risk | Mitigation |
|------|------------|
| **Database migration history** | Migration IDs are immutable strings - no database impact |
| **Swagger/API breaks** | Controllers don't directly reference DbContext |

---

## 13. Rollback Plan

### If Build Fails
```bash
git checkout .
git clean -fd
```

### If Runtime Errors Occur
```bash
git revert HEAD
```

### Database Concerns
- No database schema changes in this refactoring
- Migration history table unchanged
- If needed: `dotnet ef database update 0` then `dotnet ef database update`

---

## 14. Pre-Existing Bug Found

**Issue:** Proposals entity configurations are NOT being loaded!

**Current code (line 208):**
```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
```

This only loads configurations from StockManagement.Infrastructure assembly.

**Proposals configurations exist but are not loaded:**
- `ProposalConfiguration.cs`
- `ProposalSectionConfiguration.cs`
- `ProposalLineItemConfiguration.cs`
- `ProposalContactConfiguration.cs`

**Fix Required (add after line 208):**
```csharp
// Apply Proposals entity configurations
modelBuilder.ApplyConfigurationsFromAssembly(
    typeof(Rascor.Modules.Proposals.Infrastructure.Data.Configurations.ProposalConfiguration).Assembly);
```

---

## 15. Summary of Changes

| Category | Files Changed | Lines Changed (est.) |
|----------|--------------|---------------------|
| Moved Files | 36 | N/A |
| Namespace Updates | 36 | ~70 |
| .csproj Updates | 5 | ~40 |
| Program.cs | 1 | 1 |
| ApplicationDbContext.cs | 1 | ~5 |
| **Total** | **~40 files** | **~120 lines** |

---

## Approval Checklist

- [ ] Architecture diagram reviewed
- [ ] File movement plan approved
- [ ] Project reference changes approved
- [ ] Namespace changes understood
- [ ] Entity configuration loading fix approved
- [ ] Execution order agreed
- [ ] Risk assessment accepted
- [ ] Rollback plan understood

**Ready to proceed?** Reply with approval to begin implementation.
