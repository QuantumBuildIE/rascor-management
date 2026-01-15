# Database Migration & Deployment Guide

This document outlines the database migration strategy for RASCOR across all environments.

## Overview

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                         Migration Strategy by Environment                      │
├────────────────────┬────────────────────┬────────────────────────────────────┤
│    Development     │      CI/E2E        │           Production               │
├────────────────────┼────────────────────┼────────────────────────────────────┤
│ Auto-migrate on    │ Explicit step in   │ Railway release command            │
│ app startup        │ GitHub Actions     │ BEFORE app deployment              │
│                    │ workflow           │                                    │
├────────────────────┼────────────────────┼────────────────────────────────────┤
│ Safe: Local DB     │ Safe: Ephemeral    │ Controlled: Review required        │
│ No review needed   │ test database      │ Rollback plan ready                │
└────────────────────┴────────────────────┴────────────────────────────────────┘
```

## Environment Behavior

### Development (Local)
- **Automatic migrations** on startup for developer convenience
- Logs pending migrations before applying
- Safe because it's a local database with no production impact

### Testing (CI/E2E)
- Migrations run as **explicit workflow step** before API starts
- Provides clear visibility in CI logs
- If migrations fail, the job fails before wasting time on E2E setup

### Production
- **NO automatic migrations**
- App will **fail to start** if pending migrations exist
- Migrations must be applied via release command

---

## Production Deployment Procedure

### Pre-Deployment Checklist

- [ ] Review pending migrations in the PR/commit
- [ ] Verify migration has been tested in development
- [ ] Check for destructive operations (column drops, table deletes)
- [ ] Ensure rollback migration exists if needed
- [ ] Verify database backup is recent (Railway auto-backups)

### Step 1: Review Pending Migrations

Before deploying, check what migrations will be applied:

```bash
# From your local machine with production connection string
dotnet ef migrations list --project src/Rascor.Infrastructure --startup-project src/Rascor.API
```

### Step 2: Railway Release Command

Configure Railway to run migrations before the app starts.

**railway.json** (add to project root):
```json
{
  "$schema": "https://railway.app/railway.schema.json",
  "build": {
    "builder": "DOCKERFILE"
  },
  "deploy": {
    "startCommand": "dotnet Rascor.API.dll",
    "releaseCommand": "dotnet ef database update --project Rascor.Infrastructure.dll --startup-project Rascor.API.dll --no-build"
  }
}
```

**Alternative: Dockerfile with migration script**

If the EF CLI doesn't work in Railway, add a migration step to your Dockerfile:

```dockerfile
# In your Dockerfile, create a startup script
COPY scripts/startup.sh /app/startup.sh
RUN chmod +x /app/startup.sh
ENTRYPOINT ["/app/startup.sh"]
```

**scripts/startup.sh:**
```bash
#!/bin/bash
set -e

echo "=== Running database migrations ==="
dotnet ef database update --no-build || {
    echo "Migration failed!"
    exit 1
}

echo "=== Starting application ==="
exec dotnet Rascor.API.dll
```

### Step 3: Deploy and Monitor

1. Push to main/trigger deployment
2. Watch Railway logs for migration output
3. Verify the app starts successfully
4. Run a quick smoke test on production

---

## Rollback Procedures

### Scenario 1: Migration Failed, App Not Started

**Impact:** Low - app didn't start, old version still running (if using rolling deploy)

**Action:**
1. Check Railway logs for migration error
2. Fix the migration locally
3. Re-deploy with corrected migration

### Scenario 2: Migration Succeeded, App Has Bug

**Impact:** Medium - new code has issues

**Action:**
1. Rollback the deployment in Railway to previous version
2. If data is compatible, the old code should work
3. If schema changed, may need to create a reverse migration

### Scenario 3: Migration Caused Data Loss

**Impact:** High - data has been lost or corrupted

**Action:**
1. **STOP** - Don't deploy more changes
2. Restore from Railway backup (Railway keeps 7 days of backups)
3. Investigate root cause
4. Create a corrected migration

### Creating a Rollback Migration

If you need to undo a migration:

```bash
# Create a new migration that reverses the changes
dotnet ef migrations add Rollback_MigrationName --project src/Rascor.Infrastructure --startup-project src/Rascor.API

# Edit the generated migration to reverse the previous changes
# Then apply it
dotnet ef database update --project src/Rascor.Infrastructure --startup-project src/Rascor.API
```

---

## Best Practices for Migrations

### DO ✅

- **Small, incremental changes** - One logical change per migration
- **Non-destructive changes first** - Add columns before removing old ones
- **Test locally** - Run migration against a copy of production data
- **Add indexes in separate migration** - Large tables can lock during index creation
- **Use transactions** - Ensure migrations are atomic

### DON'T ❌

- **Drop columns immediately** - Mark as deprecated, remove in later release
- **Modify production data in migrations** - Use separate data scripts
- **Skip testing** - Always test with realistic data volumes
- **Ignore failed migrations** - Fix them, don't work around them

### Safe Column Removal Pattern

Instead of immediately dropping a column:

**Migration 1 (Release N):**
```csharp
// Add new column (if replacing)
migrationBuilder.AddColumn<string>("NewColumn", "TableName");
```

**Application Code (Release N):**
```csharp
// Write to both columns, read from new
```

**Migration 2 (Release N+1):**
```csharp
// Drop old column after confirming new one works
migrationBuilder.DropColumn("OldColumn", "TableName");
```

---

## Emergency Procedures

### Database Connection Issues

```bash
# Check Railway database status
railway status

# Get connection info
railway variables

# Connect directly to debug
railway connect postgres
```

### Manual Migration (Emergency Only)

If automated migration fails and you need to apply manually:

```bash
# Generate SQL script
dotnet ef migrations script --idempotent --project src/Rascor.Infrastructure --startup-project src/Rascor.API -o migration.sql

# Review the script carefully
cat migration.sql

# Apply via psql (Railway provides connection string)
psql $DATABASE_URL -f migration.sql
```

### Restore from Backup

Railway automatically backs up databases. To restore:

1. Go to Railway Dashboard → Database → Backups
2. Select the backup point
3. Click "Restore"
4. Railway will restore to a new database instance
5. Update your app's connection string to point to restored database

---

## Monitoring & Verification

### After Migration

1. **Check logs** for migration completion message
2. **Verify tables** exist:
   ```sql
   SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';
   ```
3. **Check row counts** for critical tables
4. **Run application smoke test**

### Health Check Endpoint

The `/health` endpoint verifies database connectivity:
- Returns `200` if database is accessible
- Returns `503` if database check fails

Use this for:
- Load balancer health checks
- Deployment readiness verification
- Monitoring alerts

---

## Summary

| Environment | Auto-Migrate | Explicit Step | Fail-Safe |
|-------------|--------------|---------------|-----------|
| Development | ✅ Yes | - | Logs pending migrations |
| Testing/CI | - | ✅ Before API | Job fails if migration fails |
| Production | ❌ No | ✅ Release command | App refuses to start if migrations pending |

This strategy ensures:
- **Development** is convenient (auto-migrate)
- **CI** is reliable (explicit, logged)
- **Production** is safe (controlled, reviewed)