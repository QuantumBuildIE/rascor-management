using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rascor.Modules.StockManagement.Application.Common.Interfaces;
using Rascor.Modules.Proposals.Application.Common.Interfaces;
using Rascor.Modules.ToolboxTalks.Application.Common.Interfaces;
using Rascor.Modules.Rams.Application.Common.Interfaces;
using Rascor.Core.Application.Interfaces;
using Rascor.Core.Domain.Common;
using Rascor.Core.Domain.Entities;
using Rascor.Core.Infrastructure.Data.Configurations;
using Rascor.Modules.StockManagement.Domain.Entities;
using Rascor.Modules.Proposals.Domain.Entities;
using Rascor.Modules.ToolboxTalks.Domain.Entities;
using Rascor.Modules.Rams.Domain.Entities;
using Rascor.Modules.StockManagement.Infrastructure.Data.Configurations;
using Rascor.Modules.StockManagement.Infrastructure.Persistence.Configurations;
using Rascor.Modules.Proposals.Infrastructure.Data.Configurations;
using Rascor.Modules.ToolboxTalks.Infrastructure.Persistence.Configurations;
using Rascor.Modules.Rams.Infrastructure.Configurations;

namespace Rascor.Core.Infrastructure.Data;

/// <summary>
/// Main database context for the RASCOR Management System
/// Implements multi-tenancy, soft deletes, audit trail, and Identity
/// </summary>
public class ApplicationDbContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>, IStockManagementDbContext, IProposalsDbContext, IToolboxTalksDbContext, IRamsDbContext, ICoreDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Current tenant ID for query filtering
    /// </summary>
    public Guid TenantId => _currentUserService?.TenantId ?? Guid.Empty;

    /// <summary>
    /// Current user ID for audit fields
    /// </summary>
    public string CurrentUserId => string.IsNullOrEmpty(_currentUserService?.UserId) ? "system" : _currentUserService.UserId;

    // Core DbSets
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<FloatUnmatchedItem> FloatUnmatchedItems => Set<FloatUnmatchedItem>();
    public DbSet<SpaNotificationAudit> SpaNotificationAudits => Set<SpaNotificationAudit>();

    // Identity/Authorization DbSets
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Stock Management DbSets
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductKit> ProductKits => Set<ProductKit>();
    public DbSet<ProductKitItem> ProductKitItems => Set<ProductKitItem>();
    public DbSet<StockLocation> StockLocations => Set<StockLocation>();
    public DbSet<BayLocation> BayLocations => Set<BayLocation>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptLine> GoodsReceiptLines => Set<GoodsReceiptLine>();
    public DbSet<StockOrder> StockOrders => Set<StockOrder>();
    public DbSet<StockOrderLine> StockOrderLines => Set<StockOrderLine>();
    public DbSet<Stocktake> Stocktakes => Set<Stocktake>();
    public DbSet<StocktakeLine> StocktakeLines => Set<StocktakeLine>();

    // Proposals DbSets
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<ProposalSection> ProposalSections => Set<ProposalSection>();
    public DbSet<ProposalLineItem> ProposalLineItems => Set<ProposalLineItem>();
    public DbSet<ProposalContact> ProposalContacts => Set<ProposalContact>();

    // Toolbox Talks DbSets
    public DbSet<ToolboxTalk> ToolboxTalks => Set<ToolboxTalk>();
    public DbSet<ToolboxTalkSection> ToolboxTalkSections => Set<ToolboxTalkSection>();
    public DbSet<ToolboxTalkQuestion> ToolboxTalkQuestions => Set<ToolboxTalkQuestion>();
    public DbSet<ToolboxTalkSchedule> ToolboxTalkSchedules => Set<ToolboxTalkSchedule>();
    public DbSet<ToolboxTalkScheduleAssignment> ToolboxTalkScheduleAssignments => Set<ToolboxTalkScheduleAssignment>();
    public DbSet<ScheduledTalk> ScheduledTalks => Set<ScheduledTalk>();
    public DbSet<ScheduledTalkSectionProgress> ScheduledTalkSectionProgress => Set<ScheduledTalkSectionProgress>();
    public DbSet<ScheduledTalkQuizAttempt> ScheduledTalkQuizAttempts => Set<ScheduledTalkQuizAttempt>();
    public DbSet<ScheduledTalkCompletion> ScheduledTalkCompletions => Set<ScheduledTalkCompletion>();
    public DbSet<ToolboxTalkSettings> ToolboxTalkSettings => Set<ToolboxTalkSettings>();
    public DbSet<ToolboxTalkTranslation> ToolboxTalkTranslations => Set<ToolboxTalkTranslation>();
    public DbSet<ToolboxTalkVideoTranslation> ToolboxTalkVideoTranslations => Set<ToolboxTalkVideoTranslation>();
    public DbSet<ToolboxTalkCourse> ToolboxTalkCourses => Set<ToolboxTalkCourse>();
    public DbSet<ToolboxTalkCourseItem> ToolboxTalkCourseItems => Set<ToolboxTalkCourseItem>();
    public DbSet<ToolboxTalkCourseTranslation> ToolboxTalkCourseTranslations => Set<ToolboxTalkCourseTranslation>();
    public DbSet<ToolboxTalkCourseAssignment> ToolboxTalkCourseAssignments => Set<ToolboxTalkCourseAssignment>();
    public DbSet<SubtitleProcessingJob> SubtitleProcessingJobs => Set<SubtitleProcessingJob>();
    public DbSet<SubtitleTranslation> SubtitleTranslations => Set<SubtitleTranslation>();

    // RAMS DbSets
    public DbSet<RamsDocument> RamsDocuments => Set<RamsDocument>();
    public DbSet<RiskAssessment> RamsRiskAssessments => Set<RiskAssessment>();
    public DbSet<MethodStep> RamsMethodSteps => Set<MethodStep>();
    public DbSet<HazardLibrary> RamsHazardLibrary => Set<HazardLibrary>();
    public DbSet<ControlMeasureLibrary> RamsControlMeasureLibrary => Set<ControlMeasureLibrary>();
    public DbSet<LegislationReference> RamsLegislationReferences => Set<LegislationReference>();
    public DbSet<SopReference> RamsSopReferences => Set<SopReference>();
    public DbSet<HazardControlLink> RamsHazardControlLinks => Set<HazardControlLink>();
    public DbSet<McpAuditLog> RamsMcpAuditLogs => Set<McpAuditLog>();
    public DbSet<RamsNotificationLog> RamsNotificationLogs => Set<RamsNotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Identity tables with custom names
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.RefreshToken).HasMaxLength(500);
            entity.Ignore(e => e.FullName);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            entity.HasMany(e => e.RolePermissions)
                .WithOne(e => e.Role)
                .HasForeignKey(rp => rp.RoleId)
                .IsRequired();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
        });

        modelBuilder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("UserClaims");
        });

        modelBuilder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("UserLogins");
        });

        modelBuilder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("RoleClaims");
        });

        modelBuilder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("UserTokens");
        });

        // Configure Permission entity
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Module).HasMaxLength(100).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();

            entity.HasMany(e => e.RolePermissions)
                .WithOne(e => e.Permission)
                .HasForeignKey(rp => rp.PermissionId)
                .IsRequired();
        });

        // Configure RolePermission join entity
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(e => new { e.RoleId, e.PermissionId });
        });

        // Apply Core entity configurations
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new SiteConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyConfiguration());
        modelBuilder.ApplyConfiguration(new ContactConfiguration());
        modelBuilder.ApplyConfiguration(new FloatUnmatchedItemConfiguration());
        modelBuilder.ApplyConfiguration(new SpaNotificationAuditConfiguration());

        // Apply Stock Management entity configurations
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new SupplierConfiguration());
        modelBuilder.ApplyConfiguration(new StockLocationConfiguration());
        modelBuilder.ApplyConfiguration(new BayLocationConfiguration());
        modelBuilder.ApplyConfiguration(new StockLevelConfiguration());
        modelBuilder.ApplyConfiguration(new StockTransactionConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseOrderConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseOrderLineConfiguration());
        modelBuilder.ApplyConfiguration(new GoodsReceiptConfiguration());
        modelBuilder.ApplyConfiguration(new GoodsReceiptLineConfiguration());
        modelBuilder.ApplyConfiguration(new StockOrderConfiguration());
        modelBuilder.ApplyConfiguration(new StockOrderLineConfiguration());
        modelBuilder.ApplyConfiguration(new StocktakeConfiguration());
        modelBuilder.ApplyConfiguration(new StocktakeLineConfiguration());
        modelBuilder.ApplyConfiguration(new ProductKitConfiguration());
        modelBuilder.ApplyConfiguration(new ProductKitItemConfiguration());

        // Apply Proposals entity configurations
        modelBuilder.ApplyConfiguration(new ProposalConfiguration());
        modelBuilder.ApplyConfiguration(new ProposalSectionConfiguration());
        modelBuilder.ApplyConfiguration(new ProposalLineItemConfiguration());
        modelBuilder.ApplyConfiguration(new ProposalContactConfiguration());

        // Apply Toolbox Talks entity configurations
        modelBuilder.ApplyConfiguration(new ToolboxTalkConfiguration());
        modelBuilder.ApplyConfiguration(new ToolboxTalkSectionConfiguration());
        modelBuilder.ApplyConfiguration(new ToolboxTalkQuestionConfiguration());
        modelBuilder.ApplyConfiguration(new ToolboxTalkScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new ToolboxTalkScheduleAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduledTalkConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduledTalkSectionProgressConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduledTalkQuizAttemptConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduledTalkCompletionConfiguration());
        modelBuilder.ApplyConfiguration(new ToolboxTalkSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new ToolboxTalkTranslationConfiguration());
        modelBuilder.ApplyConfiguration(new ToolboxTalkVideoTranslationConfiguration());
        modelBuilder.ApplyConfiguration(new ToolboxTalkCourseConfiguration());
        modelBuilder.ApplyConfiguration(new ToolboxTalkCourseItemConfiguration());
        modelBuilder.ApplyConfiguration(new ToolboxTalkCourseTranslationConfiguration());
        modelBuilder.ApplyConfiguration(new ToolboxTalkCourseAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new SubtitleProcessingJobConfiguration());
        modelBuilder.ApplyConfiguration(new SubtitleTranslationConfiguration());

        // Apply RAMS entity configurations
        modelBuilder.ApplyConfiguration(new RamsDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new RiskAssessmentConfiguration());
        modelBuilder.ApplyConfiguration(new MethodStepConfiguration());
        modelBuilder.ApplyConfiguration(new HazardLibraryConfiguration());
        modelBuilder.ApplyConfiguration(new ControlMeasureLibraryConfiguration());
        modelBuilder.ApplyConfiguration(new LegislationReferenceConfiguration());
        modelBuilder.ApplyConfiguration(new SopReferenceConfiguration());
        modelBuilder.ApplyConfiguration(new HazardControlLinkConfiguration());
        modelBuilder.ApplyConfiguration(new McpAuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new RamsNotificationLogConfiguration());

        // Apply global query filters - Core entities
        modelBuilder.Entity<Site>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<Company>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<Contact>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<FloatUnmatchedItem>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<SpaNotificationAudit>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);

        // Apply global query filters - Stock Management entities
        modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<Supplier>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<ProductKit>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<ProductKitItem>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<StockLocation>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<BayLocation>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<StockLevel>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<StockTransaction>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<PurchaseOrder>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<PurchaseOrderLine>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<GoodsReceipt>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<GoodsReceiptLine>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<StockOrder>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<StockOrderLine>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<Stocktake>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<StocktakeLine>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);

        // Apply global query filters - Proposals entities
        modelBuilder.Entity<Proposal>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<ProposalSection>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<ProposalLineItem>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);
        modelBuilder.Entity<ProposalContact>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == TenantId);

        // Note: RAMS query filters are defined in entity configurations
        // TenantEntity-based: RamsDocument, RiskAssessment, MethodStep, HazardLibrary, ControlMeasureLibrary, LegislationReference, SopReference
        // BaseEntity-based (not tenant-scoped): HazardControlLink

        // Note: Toolbox Talks query filters are defined in entity configurations
        // TenantEntity-based: ToolboxTalk, ToolboxTalkCourse, ToolboxTalkSchedule, ScheduledTalk, ToolboxTalkTranslation, ToolboxTalkVideoTranslation, SubtitleProcessingJob
        // BaseEntity-based (not tenant-scoped): ToolboxTalkSection, ToolboxTalkQuestion, ToolboxTalkCourseItem, ToolboxTalkCourseTranslation,
        //   ToolboxTalkScheduleAssignment, ScheduledTalkSectionProgress, ScheduledTalkQuizAttempt, ScheduledTalkCompletion, ToolboxTalkSettings, SubtitleTranslation

        // Apply query filter for Permission (not tenant-scoped, global)
        modelBuilder.Entity<Permission>().HasQueryFilter(e => !e.IsDeleted);

        // Apply query filter for Tenant (not tenant-scoped, global)
        modelBuilder.Entity<Tenant>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set audit fields before saving
        SetAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        // Automatically set audit fields before saving
        SetAuditFields();
        return base.SaveChanges();
    }

    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = CurrentUserId;
                    entry.Entity.IsDeleted = false;

                    // Set TenantId for new tenant entities
                    if (entry.Entity is TenantEntity tenantEntity && tenantEntity.TenantId == Guid.Empty)
                    {
                        tenantEntity.TenantId = TenantId;
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = CurrentUserId;

                    // Prevent modification of CreatedAt and CreatedBy
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;

                    // Prevent modification of TenantId
                    if (entry.Entity is TenantEntity)
                    {
                        entry.Property(nameof(TenantEntity.TenantId)).IsModified = false;
                    }
                    break;

                case EntityState.Deleted:
                    // Implement soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = CurrentUserId;
                    break;
            }
        }

        // Handle User entity audit fields separately (doesn't inherit from BaseEntity)
        var userEntries = ChangeTracker.Entries<User>();
        foreach (var entry in userEntries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = CurrentUserId;
                    if (entry.Entity.TenantId == Guid.Empty)
                    {
                        entry.Entity.TenantId = TenantId;
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = CurrentUserId;
                    entry.Property(nameof(User.CreatedAt)).IsModified = false;
                    entry.Property(nameof(User.CreatedBy)).IsModified = false;
                    entry.Property(nameof(User.TenantId)).IsModified = false;
                    break;
            }
        }

        // Handle Role entity audit fields separately
        var roleEntries = ChangeTracker.Entries<Role>();
        foreach (var entry in roleEntries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = CurrentUserId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = CurrentUserId;
                    entry.Property(nameof(Role.CreatedAt)).IsModified = false;
                    entry.Property(nameof(Role.CreatedBy)).IsModified = false;
                    break;
            }
        }
    }
}
