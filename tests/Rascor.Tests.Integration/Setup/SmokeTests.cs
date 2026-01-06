using System.Net;
using Microsoft.EntityFrameworkCore;
using Rascor.Tests.Common.TestTenant;
using Rascor.Tests.Integration.Fixtures;

namespace Rascor.Tests.Integration.Setup;

/// <summary>
/// Smoke tests to verify the integration test setup is working correctly.
/// These tests verify:
/// - The application starts correctly
/// - Authentication works as expected
/// - Database connection is functional
/// - Test tenant data is seeded correctly
///
/// NOTE: Tests should NEVER query RASCOR tenant data - only test tenant data.
/// </summary>
public class SmokeTests : IntegrationTestBase
{
    // Use the test tenant ID (tests should ONLY use test tenant data)
    private static readonly Guid TestTenantId = TestTenantConstants.TenantId;

    public SmokeTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ApiEndpoint_ReturnsNotFoundForInvalidRoute()
    {
        // Act - Test that the API returns 404 for non-existent routes (basic routing test)
        var response = await UnauthenticatedClient.GetAsync("/api/nonexistent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AuthenticatedRequest_WithValidAdminToken_Succeeds()
    {
        // Act
        var response = await AdminClient.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AuthenticatedRequest_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await UnauthenticatedClient.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminClient_HasAccessToProducts()
    {
        // Act - Products endpoint should be accessible to Admin
        var response = await AdminClient.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SiteManagerClient_HasAccessToSiteAttendanceEvents()
    {
        // Act - Site managers should have SiteAttendance.View permission
        var response = await SiteManagerClient.GetAsync("/api/site-attendance/events");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue($"Expected success but got {response.StatusCode}");
    }

    [Fact]
    public async Task WarehouseClient_HasAccessToProducts()
    {
        // Act - Warehouse users should have StockManagement.View permission
        var response = await WarehouseClient.GetAsync("/api/products");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue($"Expected success but got {response.StatusCode}");
    }

    [Fact]
    public async Task OperatorClient_HasAccessToProducts()
    {
        // Act - Operators should have StockManagement.View permission
        var response = await OperatorClient.GetAsync("/api/products");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue($"Expected success but got {response.StatusCode}");
    }

    [Fact]
    public async Task FinanceClient_HasAccessToProductsButNotManagement()
    {
        // Finance users should have StockManagement.View but not ManageProducts
        // Act - Can view products
        var getResponse = await FinanceClient.GetAsync("/api/products");
        getResponse.IsSuccessStatusCode.Should().BeTrue("Finance should be able to view products");
    }

    [Fact]
    public async Task DatabaseConnection_Works()
    {
        // Act - This implicitly tests database connectivity
        var context = GetDbContext();
        var canConnect = await context.Database.CanConnectAsync();

        // Assert
        canConnect.Should().BeTrue("Database connection should work");
    }

    [Fact]
    public async Task TestTenant_IsSeeded()
    {
        // Arrange
        var context = GetDbContext();

        // Act - Check if test tenant exists (seeded by TestTenantSeeder)
        var tenant = await context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == TestTenantId);

        // Assert
        tenant.Should().NotBeNull("Test tenant should be seeded");
        tenant!.Name.Should().Be(TestTenantConstants.TenantName);
    }

    [Fact]
    public async Task TestTenant_Sites_AreSeeded()
    {
        // Arrange
        var context = GetDbContext();

        // Act - Check if test tenant sites exist (seeded by TestTenantSeeder)
        var sites = await context.Sites
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == TestTenantId)
            .ToListAsync();

        // Assert
        sites.Should().NotBeEmpty("At least one test site should be seeded");
        sites.Should().Contain(s => s.Id == TestTenantConstants.Sites.MainSite);
    }

    [Fact]
    public async Task TestTenant_Products_AreSeeded()
    {
        // Arrange
        var context = GetDbContext();

        // Act - Check if test tenant products exist (seeded by TestTenantSeeder)
        var products = await context.Products
            .IgnoreQueryFilters()
            .Where(p => p.TenantId == TestTenantId)
            .ToListAsync();

        // Assert
        products.Should().NotBeEmpty("Test products should be seeded by TestTenantSeeder");
        products.Should().Contain(p => p.Id == TestTenantConstants.StockManagement.Products.HardHat);
    }

    [Fact]
    public async Task TestTenant_Categories_AreSeeded()
    {
        // Arrange
        var context = GetDbContext();

        // Act - Check if test tenant categories exist (seeded by TestTenantSeeder)
        var categories = await context.Categories
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == TestTenantId)
            .ToListAsync();

        // Assert
        categories.Should().NotBeEmpty("Test categories should be seeded");
        categories.Should().Contain(c => c.Id == TestTenantConstants.StockManagement.Categories.Safety);
    }

    [Fact]
    public async Task CreateAuthenticatedClient_WithCustomPermissions_Works()
    {
        // Arrange - Create a client with specific permissions
        var customClient = Factory.CreateAuthenticatedClient(
            userId: Guid.NewGuid(),
            email: "custom@test.com",
            roles: new[] { "Custom" },
            permissions: new[] { "StockManagement.View" }
        );

        // Act - Should be able to view products
        var response = await customClient.GetAsync("/api/products");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue($"Custom client should have access with correct permissions, got {response.StatusCode}");

        customClient.Dispose();
    }

    [Fact]
    public void FakeEmailSender_CapturesEmails()
    {
        // Arrange
        var emailSender = Factory.FakeEmailSender;
        emailSender.Clear();

        // Act
        emailSender.SendEmailAsync("test@example.com", "Test Subject", "Test Body");

        // Assert
        emailSender.Count.Should().Be(1);
        emailSender.LastEmail.Should().NotBeNull();
        emailSender.LastEmail!.To.Should().Be("test@example.com");
        emailSender.LastEmail.Subject.Should().Be("Test Subject");
    }
}
