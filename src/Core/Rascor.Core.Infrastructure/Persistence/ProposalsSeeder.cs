using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Core.Domain.Entities;
using Rascor.Modules.Proposals.Domain.Entities;
using Rascor.Modules.StockManagement.Domain.Entities;

namespace Rascor.Core.Infrastructure.Persistence;

/// <summary>
/// Seeds test data for Proposals module
/// </summary>
public static class ProposalsSeeder
{
    private static readonly Guid TenantId = DataSeeder.DefaultTenantId;

    // Predefined GUIDs for companies
    private static readonly Guid RiversideCompanyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid DcgCompanyId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid LbsCompanyId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid CoastalCompanyId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    // Predefined GUIDs for contacts
    private static readonly Guid JohnMurphyContactId = Guid.Parse("11111111-aaaa-aaaa-aaaa-111111111111");
    private static readonly Guid SarahOBrienContactId = Guid.Parse("22222222-bbbb-bbbb-bbbb-222222222222");
    private static readonly Guid TomKellyContactId = Guid.Parse("33333333-cccc-cccc-cccc-333333333333");
    private static readonly Guid MichaelWalshContactId = Guid.Parse("44444444-dddd-dddd-dddd-444444444444");
    private static readonly Guid EmmaByrneContactId = Guid.Parse("55555555-eeee-eeee-eeee-555555555555");
    private static readonly Guid DavidQuinnContactId = Guid.Parse("66666666-ffff-ffff-ffff-666666666666");

    /// <summary>
    /// Seed proposals test data if database is empty
    /// </summary>
    public static async Task SeedAsync(DbContext context, ILogger logger)
    {
        // Check if proposals data already exists
        var hasProposals = await context.Set<Proposal>().IgnoreQueryFilters().AnyAsync();
        if (hasProposals)
        {
            logger.LogInformation("Proposals data already exists, skipping");
            return;
        }

        logger.LogInformation("Seeding proposals test data...");

        // Seed in order to respect foreign keys
        var companies = await SeedCompaniesAsync(context, logger);
        var contacts = await SeedContactsAsync(context, companies, logger);

        // Get existing products and kits for proposals
        var products = await context.Set<Product>().IgnoreQueryFilters().ToListAsync();
        var productKits = await context.Set<ProductKit>().IgnoreQueryFilters().ToListAsync();

        if (products.Count == 0)
        {
            logger.LogWarning("No products found - skipping proposals seeding");
            return;
        }

        await SeedProposalsAsync(context, companies, contacts, products, productKits, logger);

        logger.LogInformation("Proposals test data seeding completed");
    }

    private static async Task<List<Company>> SeedCompaniesAsync(DbContext context, ILogger logger)
    {
        // Check if companies already exist
        var existingCompanies = await context.Set<Company>().IgnoreQueryFilters()
            .Where(c => c.CompanyCode.StartsWith("RIVER") || c.CompanyCode.StartsWith("DCG") ||
                        c.CompanyCode.StartsWith("LBS") || c.CompanyCode.StartsWith("COAST"))
            .ToListAsync();

        if (existingCompanies.Count >= 4)
        {
            logger.LogInformation("Client companies already exist, skipping");
            return existingCompanies;
        }

        var companies = new List<Company>
        {
            CreateCompany(RiversideCompanyId, "RIVER-001", "Riverside Developments Ltd", "Client",
                "123 Quay Street", null, "Dublin", "Dublin", "D01 R1V3", "Ireland",
                "+353 1 234 5678", "info@riverside.ie", "IE1234567A"),

            CreateCompany(DcgCompanyId, "DCG-001", "Dublin Construction Group", "Client",
                "45 Grafton Street", "Suite 200", "Dublin", "Dublin", "D02 DC99", "Ireland",
                "+353 1 345 6789", "contracts@dcg.ie", "IE2345678B"),

            CreateCompany(LbsCompanyId, "LBS-001", "Leinster Building Services", "Client",
                "78 Georgian Lane", null, "Kilkenny", "Kilkenny", "R95 LB55", "Ireland",
                "+353 56 456 7890", "info@lbs.ie", "IE3456789C"),

            CreateCompany(CoastalCompanyId, "COAST-001", "Coastal Properties", "Client",
                "12 Marina Drive", null, "Malahide", "Dublin", "K36 CP12", "Ireland",
                "+353 1 567 8901", "hello@coastalprops.ie", "IE4567890D")
        };

        await context.Set<Company>().AddRangeAsync(companies);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} client companies", companies.Count);

        return companies;
    }

    private static async Task<List<Contact>> SeedContactsAsync(DbContext context, List<Company> companies, ILogger logger)
    {
        // Check if contacts already exist
        var existingContacts = await context.Set<Contact>().IgnoreQueryFilters()
            .Where(c => c.Email != null && (c.Email.Contains("riverside") || c.Email.Contains("dcg") ||
                        c.Email.Contains("lbs") || c.Email.Contains("coastal")))
            .ToListAsync();

        if (existingContacts.Count >= 6)
        {
            logger.LogInformation("Client contacts already exist, skipping");
            return existingContacts;
        }

        var companyDict = companies.ToDictionary(c => c.CompanyCode, c => c.Id);

        var contacts = new List<Contact>
        {
            CreateContact(JohnMurphyContactId, "John", "Murphy", "Quantity Surveyor",
                "john.murphy@riverside.ie", "+353 1 234 5678", "+353 87 123 4567",
                companyDict["RIVER-001"], true),

            CreateContact(SarahOBrienContactId, "Sarah", "O'Brien", "Contracts Manager",
                "sarah.obrien@dcg.ie", "+353 1 345 6789", "+353 87 234 5678",
                companyDict["DCG-001"], true),

            CreateContact(TomKellyContactId, "Tom", "Kelly", "Site Manager",
                "tom.kelly@dcg.ie", "+353 1 345 6790", "+353 87 345 6789",
                companyDict["DCG-001"], false),

            CreateContact(MichaelWalshContactId, "Michael", "Walsh", "Project Manager",
                "m.walsh@lbs.ie", "+353 56 456 7890", "+353 87 456 7890",
                companyDict["LBS-001"], true),

            CreateContact(EmmaByrneContactId, "Emma", "Byrne", "Director",
                "emma@coastalprops.ie", "+353 1 567 8901", "+353 87 567 8901",
                companyDict["COAST-001"], true),

            CreateContact(DavidQuinnContactId, "David", "Quinn", "Site Supervisor",
                "david.quinn@coastalprops.ie", "+353 1 567 8902", "+353 87 678 9012",
                companyDict["COAST-001"], false)
        };

        await context.Set<Contact>().AddRangeAsync(contacts);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} client contacts", contacts.Count);

        return contacts;
    }

    private static async Task SeedProposalsAsync(
        DbContext context,
        List<Company> companies,
        List<Contact> contacts,
        List<Product> products,
        List<ProductKit> productKits,
        ILogger logger)
    {
        var now = DateTime.UtcNow;
        var productDict = products.ToDictionary(p => p.ProductCode, p => p);
        var kitDict = productKits.ToDictionary(k => k.KitCode, k => k);
        var contactDict = contacts.ToDictionary(c => c.Id, c => c);
        var companyDict = companies.ToDictionary(c => c.CompanyCode, c => c);

        var proposals = new List<Proposal>();
        var proposalSections = new List<ProposalSection>();
        var proposalLineItems = new List<ProposalLineItem>();
        var proposalContacts = new List<ProposalContact>();

        // ================================================================================
        // Proposal 1: Draft - Riverside Apartments Block A Basement
        // ================================================================================
        var proposal1 = CreateProposal(
            "PROP-2024-0001",
            companyDict["RIVER-001"],
            contactDict[JohnMurphyContactId],
            "Riverside Apartments - Block A Basement",
            "123 River Road, Dublin 4",
            "Basement waterproofing for new residential development - Block A",
            now.AddDays(-5),
            now.AddDays(25),
            ProposalStatus.Draft,
            0m);

        proposals.Add(proposal1);

        // Section 1: Basement Waterproofing (from Kit)
        var section1_1 = CreateSection(proposal1.Id, "Basement Waterproofing", "Complete basement tanking system", 1,
            kitDict.ContainsKey("KIT-TANK-001") ? kitDict["KIT-TANK-001"].Id : null);
        proposalSections.Add(section1_1);

        // Add kit items
        if (productDict.ContainsKey("WPF002"))
        {
            proposalLineItems.Add(CreateLineItem(section1_1.Id, productDict["WPF002"], 2, 162.50m, 125.00m, 1, "Tanking slurry - 2 units for full coverage"));
        }
        if (productDict.ContainsKey("ADH001"))
        {
            proposalLineItems.Add(CreateLineItem(section1_1.Id, productDict["ADH001"], 1, 58.50m, 45.00m, 2, "Primer"));
        }
        if (productDict.ContainsKey("SEA001"))
        {
            proposalLineItems.Add(CreateLineItem(section1_1.Id, productDict["SEA001"], 1, 24.05m, 18.50m, 3, "Reinforcement sealant for corners"));
        }

        // Section 2: Additional Materials
        var section1_2 = CreateSection(proposal1.Id, "Additional Materials", null, 2, null);
        proposalSections.Add(section1_2);
        proposalLineItems.Add(CreateAdHocLineItem(section1_2.Id, "Site Survey and Assessment", "Each", 1, 500.00m, 300.00m, 1, null));

        // Contact
        proposalContacts.Add(CreateProposalContact(proposal1.Id, JohnMurphyContactId, "John Murphy", "john.murphy@riverside.ie", "+353 87 123 4567", "Quantity Surveyor", true));

        // ================================================================================
        // Proposal 2: Submitted - DCG Office Renovation
        // ================================================================================
        var proposal2 = CreateProposal(
            "PROP-2024-0002",
            companyDict["DCG-001"],
            contactDict[SarahOBrienContactId],
            "DCG Office Renovation - Damp Treatment",
            "45 Grafton Street, Dublin 2",
            "Damp proofing treatment for office basement and ground floor",
            now.AddDays(-10),
            now.AddDays(20),
            ProposalStatus.Submitted,
            5m);
        proposal2.SubmittedDate = now.AddDays(-8);

        proposals.Add(proposal2);

        // Section 1: Damp Proofing Works (from Kit - doubled quantities)
        var section2_1 = CreateSection(proposal2.Id, "Damp Proofing Works", "DPC injection treatment throughout affected areas", 1,
            kitDict.ContainsKey("KIT-DPI-001") ? kitDict["KIT-DPI-001"].Id : null);
        proposalSections.Add(section2_1);

        if (productDict.ContainsKey("WPF002"))
        {
            proposalLineItems.Add(CreateLineItem(section2_1.Id, productDict["WPF002"], 2, 162.50m, 125.00m, 1, "DPC injection cream"));
        }
        if (productDict.ContainsKey("TLS002"))
        {
            proposalLineItems.Add(CreateLineItem(section2_1.Id, productDict["TLS002"], 2, 84.50m, 65.00m, 2, "Drill bit set for injection holes"));
        }
        if (productDict.ContainsKey("SAF001"))
        {
            proposalLineItems.Add(CreateLineItem(section2_1.Id, productDict["SAF001"], 2, 11.05m, 8.50m, 3, "Safety equipment"));
        }

        // Section 2: Wall Preparation
        var section2_2 = CreateSection(proposal2.Id, "Wall Preparation", null, 2, null);
        proposalSections.Add(section2_2);

        if (productDict.ContainsKey("ADH001"))
        {
            proposalLineItems.Add(CreateLineItem(section2_2.Id, productDict["ADH001"], 3, 58.50m, 45.00m, 1, "Surface preparation"));
        }
        if (productDict.ContainsKey("SEA002"))
        {
            proposalLineItems.Add(CreateLineItem(section2_2.Id, productDict["SEA002"], 4, 20.79m, 15.99m, 2, "Fire retardant sealant"));
        }

        // Contacts
        proposalContacts.Add(CreateProposalContact(proposal2.Id, SarahOBrienContactId, "Sarah O'Brien", "sarah.obrien@dcg.ie", "+353 87 234 5678", "Contracts Manager", true));
        proposalContacts.Add(CreateProposalContact(proposal2.Id, TomKellyContactId, "Tom Kelly", "tom.kelly@dcg.ie", "+353 87 345 6789", "Site Manager", false));

        // ================================================================================
        // Proposal 3: Approved - Heritage House Full Waterproofing
        // ================================================================================
        var proposal3 = CreateProposal(
            "PROP-2024-0003",
            companyDict["LBS-001"],
            contactDict[MichaelWalshContactId],
            "Heritage House - Full Waterproofing",
            "78 Georgian Lane, Kilkenny",
            "Comprehensive waterproofing solution for heritage property basement and external walls",
            now.AddDays(-20),
            now.AddDays(10),
            ProposalStatus.Approved,
            10m);
        proposal3.SubmittedDate = now.AddDays(-18);
        proposal3.ApprovedDate = now.AddDays(-15);
        proposal3.ApprovedBy = "admin@rascor.ie";

        proposals.Add(proposal3);

        // Section 1: External Waterproofing (from Kit)
        var section3_1 = CreateSection(proposal3.Id, "External Waterproofing", "Membrane system for external walls", 1,
            kitDict.ContainsKey("KIT-WRAP-001") ? kitDict["KIT-WRAP-001"].Id : null);
        proposalSections.Add(section3_1);

        if (productDict.ContainsKey("WPF001"))
        {
            proposalLineItems.Add(CreateLineItem(section3_1.Id, productDict["WPF001"], 3, 110.50m, 85.00m, 1, "Main waterproofing membrane"));
        }
        if (productDict.ContainsKey("ADH001"))
        {
            proposalLineItems.Add(CreateLineItem(section3_1.Id, productDict["ADH001"], 4, 58.50m, 45.00m, 2, "Adhesive paste"));
        }
        if (productDict.ContainsKey("SEA001"))
        {
            proposalLineItems.Add(CreateLineItem(section3_1.Id, productDict["SEA001"], 2, 24.05m, 18.50m, 3, "Corner and joint sealant"));
        }

        // Section 2: Internal Tanking (from Kit)
        var section3_2 = CreateSection(proposal3.Id, "Internal Tanking", "Basement tanking system", 2,
            kitDict.ContainsKey("KIT-TANK-001") ? kitDict["KIT-TANK-001"].Id : null);
        proposalSections.Add(section3_2);

        if (productDict.ContainsKey("WPF002"))
        {
            proposalLineItems.Add(CreateLineItem(section3_2.Id, productDict["WPF002"], 4, 162.50m, 125.00m, 1, "Tanking slurry"));
        }
        if (productDict.ContainsKey("WPF003"))
        {
            proposalLineItems.Add(CreateLineItem(section3_2.Id, productDict["WPF003"], 2, 188.50m, 145.00m, 2, "Radon barrier"));
        }

        // Section 3: Labour and Installation
        var section3_3 = CreateSection(proposal3.Id, "Labour and Installation", null, 3, null);
        proposalSections.Add(section3_3);
        proposalLineItems.Add(CreateAdHocLineItem(section3_3.Id, "Skilled Labour - Waterproofing Specialist", "Hours", 40, 65.00m, 45.00m, 1, null));
        proposalLineItems.Add(CreateAdHocLineItem(section3_3.Id, "General Labour", "Hours", 80, 35.00m, 22.00m, 2, null));

        // Contact
        proposalContacts.Add(CreateProposalContact(proposal3.Id, MichaelWalshContactId, "Michael Walsh", "m.walsh@lbs.ie", "+353 87 456 7890", "Project Manager", true));

        // ================================================================================
        // Proposal 4: Won - Coastal Properties Basement Conversion
        // ================================================================================
        var proposal4 = CreateProposal(
            "PROP-2024-0004",
            companyDict["COAST-001"],
            contactDict[EmmaByrneContactId],
            "Seaside Villa - Basement Conversion",
            "12 Coastal Drive, Malahide",
            "Full basement waterproofing for conversion to habitable space",
            now.AddDays(-45),
            now.AddDays(-15),
            ProposalStatus.Won,
            7.5m);
        proposal4.SubmittedDate = now.AddDays(-43);
        proposal4.ApprovedDate = now.AddDays(-40);
        proposal4.ApprovedBy = "admin@rascor.ie";
        proposal4.WonDate = now.AddDays(-30);
        proposal4.WonLostReason = "Competitive pricing and previous good work";

        proposals.Add(proposal4);

        // Section 1: Full Basement Waterproofing
        var section4_1 = CreateSection(proposal4.Id, "Full Basement Waterproofing", null, 1, null);
        proposalSections.Add(section4_1);

        if (productDict.ContainsKey("WPF001"))
        {
            proposalLineItems.Add(CreateLineItem(section4_1.Id, productDict["WPF001"], 5, 110.50m, 85.00m, 1, "Waterproofing membrane"));
        }
        if (productDict.ContainsKey("WPF002"))
        {
            proposalLineItems.Add(CreateLineItem(section4_1.Id, productDict["WPF002"], 3, 162.50m, 125.00m, 2, "Tanking slurry"));
        }
        if (productDict.ContainsKey("ADH001"))
        {
            proposalLineItems.Add(CreateLineItem(section4_1.Id, productDict["ADH001"], 5, 58.50m, 45.00m, 3, "Primer and adhesive"));
        }
        if (productDict.ContainsKey("SEA001"))
        {
            proposalLineItems.Add(CreateLineItem(section4_1.Id, productDict["SEA001"], 4, 24.05m, 18.50m, 4, "Joint sealant"));
        }

        // Section 2: Drainage System
        var section4_2 = CreateSection(proposal4.Id, "Drainage System", null, 2, null);
        proposalSections.Add(section4_2);

        if (productDict.ContainsKey("DRN001"))
        {
            proposalLineItems.Add(CreateLineItem(section4_2.Id, productDict["DRN001"], 10, 29.25m, 22.50m, 1, "110mm Underground Drainage Pipe"));
        }
        if (productDict.ContainsKey("DRN002"))
        {
            proposalLineItems.Add(CreateLineItem(section4_2.Id, productDict["DRN002"], 6, 45.50m, 35.00m, 2, "Drainage Channel"));
        }

        // Section 3: Finishing Works
        var section4_3 = CreateSection(proposal4.Id, "Finishing Works", null, 3, null);
        proposalSections.Add(section4_3);

        if (productDict.ContainsKey("INS001"))
        {
            proposalLineItems.Add(CreateLineItem(section4_3.Id, productDict["INS001"], 20, 36.40m, 28.00m, 1, "Insulation boards"));
        }
        if (productDict.ContainsKey("FIX001"))
        {
            proposalLineItems.Add(CreateLineItem(section4_3.Id, productDict["FIX001"], 4, 24.05m, 18.50m, 2, "Anchor bolts"));
        }
        proposalLineItems.Add(CreateAdHocLineItem(section4_3.Id, "Installation Labour", "Hours", 60, 55.00m, 38.00m, 3, null));

        // Contacts
        proposalContacts.Add(CreateProposalContact(proposal4.Id, EmmaByrneContactId, "Emma Byrne", "emma@coastalprops.ie", "+353 87 567 8901", "Director", true));
        proposalContacts.Add(CreateProposalContact(proposal4.Id, DavidQuinnContactId, "David Quinn", "david.quinn@coastalprops.ie", "+353 87 678 9012", "Site Supervisor", false));

        // ================================================================================
        // Proposal 5: Lost - DCG Warehouse Floor Treatment
        // ================================================================================
        var proposal5 = CreateProposal(
            "PROP-2024-0005",
            companyDict["DCG-001"],
            contactDict[SarahOBrienContactId],
            "DCG Warehouse - Floor Treatment",
            "Industrial Estate, Dublin 12",
            "Floor sealing and treatment for warehouse facility",
            now.AddDays(-60),
            now.AddDays(-30),
            ProposalStatus.Lost,
            0m);
        proposal5.SubmittedDate = now.AddDays(-58);
        proposal5.LostDate = now.AddDays(-35);
        proposal5.WonLostReason = "Client chose cheaper competitor";

        proposals.Add(proposal5);

        // Section 1: Floor Sealing
        var section5_1 = CreateSection(proposal5.Id, "Floor Sealing", "Industrial floor sealing system", 1, null);
        proposalSections.Add(section5_1);

        if (productDict.ContainsKey("ADH001"))
        {
            proposalLineItems.Add(CreateLineItem(section5_1.Id, productDict["ADH001"], 10, 58.50m, 45.00m, 1, "Floor primer"));
        }
        if (productDict.ContainsKey("CON001"))
        {
            proposalLineItems.Add(CreateLineItem(section5_1.Id, productDict["CON001"], 50, 8.45m, 6.50m, 2, "Concrete mix for repairs"));
        }
        proposalLineItems.Add(CreateAdHocLineItem(section5_1.Id, "Floor Sealer (Premium)", "m²", 500, 8.50m, 5.00m, 3, null));
        proposalLineItems.Add(CreateAdHocLineItem(section5_1.Id, "Application Labour", "Hours", 24, 45.00m, 32.00m, 4, null));

        // Contact
        proposalContacts.Add(CreateProposalContact(proposal5.Id, SarahOBrienContactId, "Sarah O'Brien", "sarah.obrien@dcg.ie", "+353 87 234 5678", "Contracts Manager", true));

        // ================================================================================
        // Proposal 6: Expired - Riverside Phase 2 Initial Quote
        // ================================================================================
        var proposal6 = CreateProposal(
            "PROP-2024-0006",
            companyDict["RIVER-001"],
            contactDict[JohnMurphyContactId],
            "Riverside Phase 2 - Initial Quote",
            "125 River Road, Dublin 4",
            "Preliminary waterproofing quote for Phase 2 development",
            now.AddDays(-90),
            now.AddDays(-60),
            ProposalStatus.Expired,
            5m);
        proposal6.SubmittedDate = now.AddDays(-88);

        proposals.Add(proposal6);

        // Section 1: Preliminary Works
        var section6_1 = CreateSection(proposal6.Id, "Preliminary Works", null, 1, null);
        proposalSections.Add(section6_1);

        if (productDict.ContainsKey("WPF001"))
        {
            proposalLineItems.Add(CreateLineItem(section6_1.Id, productDict["WPF001"], 2, 110.50m, 85.00m, 1, "Waterproofing membrane"));
        }
        if (productDict.ContainsKey("ADH001"))
        {
            proposalLineItems.Add(CreateLineItem(section6_1.Id, productDict["ADH001"], 2, 58.50m, 45.00m, 2, "Adhesive"));
        }
        proposalLineItems.Add(CreateAdHocLineItem(section6_1.Id, "Site Survey", "Each", 1, 400.00m, 250.00m, 3, null));

        // Contact
        proposalContacts.Add(CreateProposalContact(proposal6.Id, JohnMurphyContactId, "John Murphy", "john.murphy@riverside.ie", "+353 87 123 4567", "Quantity Surveyor", true));

        // ================================================================================
        // Proposal 7: Draft (Revision) - Heritage House v2
        // ================================================================================
        var proposal7 = CreateProposal(
            "PROP-2024-0007",
            companyDict["LBS-001"],
            contactDict[MichaelWalshContactId],
            "Heritage House - Full Waterproofing (Revised)",
            "78 Georgian Lane, Kilkenny",
            "Revision of PROP-2024-0003 with updated pricing per client request",
            now.AddDays(-5),
            now.AddDays(25),
            ProposalStatus.Draft,
            12m);
        proposal7.Version = 2;
        proposal7.ParentProposalId = proposal3.Id;

        proposals.Add(proposal7);

        // Copy sections from Proposal 3 with slightly adjusted prices
        var section7_1 = CreateSection(proposal7.Id, "External Waterproofing", "Membrane system for external walls (revised pricing)", 1,
            kitDict.ContainsKey("KIT-WRAP-001") ? kitDict["KIT-WRAP-001"].Id : null);
        proposalSections.Add(section7_1);

        if (productDict.ContainsKey("WPF001"))
        {
            proposalLineItems.Add(CreateLineItem(section7_1.Id, productDict["WPF001"], 3, 105.00m, 85.00m, 1, "Main waterproofing membrane - revised"));
        }
        if (productDict.ContainsKey("ADH001"))
        {
            proposalLineItems.Add(CreateLineItem(section7_1.Id, productDict["ADH001"], 4, 55.00m, 45.00m, 2, "Adhesive paste - revised"));
        }
        if (productDict.ContainsKey("SEA001"))
        {
            proposalLineItems.Add(CreateLineItem(section7_1.Id, productDict["SEA001"], 2, 22.00m, 18.50m, 3, "Corner and joint sealant - revised"));
        }

        var section7_2 = CreateSection(proposal7.Id, "Internal Tanking", "Basement tanking system (revised pricing)", 2,
            kitDict.ContainsKey("KIT-TANK-001") ? kitDict["KIT-TANK-001"].Id : null);
        proposalSections.Add(section7_2);

        if (productDict.ContainsKey("WPF002"))
        {
            proposalLineItems.Add(CreateLineItem(section7_2.Id, productDict["WPF002"], 4, 155.00m, 125.00m, 1, "Tanking slurry - revised"));
        }
        if (productDict.ContainsKey("WPF003"))
        {
            proposalLineItems.Add(CreateLineItem(section7_2.Id, productDict["WPF003"], 2, 180.00m, 145.00m, 2, "Radon barrier - revised"));
        }

        var section7_3 = CreateSection(proposal7.Id, "Labour and Installation", "Updated labour rates", 3, null);
        proposalSections.Add(section7_3);
        proposalLineItems.Add(CreateAdHocLineItem(section7_3.Id, "Skilled Labour - Waterproofing Specialist", "Hours", 40, 62.00m, 45.00m, 1, "Revised rate"));
        proposalLineItems.Add(CreateAdHocLineItem(section7_3.Id, "General Labour", "Hours", 80, 33.00m, 22.00m, 2, "Revised rate"));

        // Contact
        proposalContacts.Add(CreateProposalContact(proposal7.Id, MichaelWalshContactId, "Michael Walsh", "m.walsh@lbs.ie", "+353 87 456 7890", "Project Manager", true));

        // ================================================================================
        // Save all entities
        // ================================================================================
        await context.Set<Proposal>().AddRangeAsync(proposals);
        await context.SaveChangesAsync();

        await context.Set<ProposalSection>().AddRangeAsync(proposalSections);
        await context.SaveChangesAsync();

        await context.Set<ProposalLineItem>().AddRangeAsync(proposalLineItems);
        await context.SaveChangesAsync();

        await context.Set<ProposalContact>().AddRangeAsync(proposalContacts);
        await context.SaveChangesAsync();

        // Calculate totals for all proposals
        await CalculateProposalTotalsAsync(context, proposals, proposalSections, proposalLineItems);

        logger.LogInformation("Seeded {Count} proposals with {SectionCount} sections and {LineCount} line items",
            proposals.Count, proposalSections.Count, proposalLineItems.Count);
    }

    private static async Task CalculateProposalTotalsAsync(
        DbContext context,
        List<Proposal> proposals,
        List<ProposalSection> sections,
        List<ProposalLineItem> lineItems)
    {
        foreach (var proposal in proposals)
        {
            var proposalSections = sections.Where(s => s.ProposalId == proposal.Id).ToList();
            decimal subtotal = 0;
            decimal totalCost = 0;

            foreach (var section in proposalSections)
            {
                var sectionItems = lineItems.Where(li => li.ProposalSectionId == section.Id).ToList();

                section.SectionTotal = sectionItems.Sum(li => li.LineTotal);
                section.SectionCost = sectionItems.Sum(li => li.LineCost);
                section.SectionMargin = section.SectionTotal - section.SectionCost;

                subtotal += section.SectionTotal;
                totalCost += section.SectionCost;
            }

            proposal.Subtotal = subtotal;
            proposal.TotalCost = totalCost;
            proposal.DiscountAmount = subtotal * (proposal.DiscountPercent / 100);
            proposal.NetTotal = subtotal - proposal.DiscountAmount;
            proposal.VatAmount = proposal.NetTotal * (proposal.VatRate / 100);
            proposal.GrandTotal = proposal.NetTotal + proposal.VatAmount;
            proposal.TotalMargin = proposal.NetTotal - totalCost;
            proposal.MarginPercent = proposal.NetTotal > 0 ? (proposal.TotalMargin / proposal.NetTotal) * 100 : 0;
        }

        await context.SaveChangesAsync();
    }

    #region Helper Methods

    private static Company CreateCompany(Guid id, string code, string name, string type,
        string? addressLine1, string? addressLine2, string? city, string? county, string? postalCode, string? country,
        string? phone, string? email, string? vatNumber) => new()
    {
        Id = id,
        TenantId = TenantId,
        CompanyCode = code,
        CompanyName = name,
        CompanyType = type,
        AddressLine1 = addressLine1,
        AddressLine2 = addressLine2,
        City = city,
        County = county,
        PostalCode = postalCode,
        Country = country,
        Phone = phone,
        Email = email,
        VatNumber = vatNumber,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "system"
    };

    private static Contact CreateContact(Guid id, string firstName, string lastName, string? jobTitle,
        string? email, string? phone, string? mobile, Guid companyId, bool isPrimary) => new()
    {
        Id = id,
        TenantId = TenantId,
        FirstName = firstName,
        LastName = lastName,
        JobTitle = jobTitle,
        Email = email,
        Phone = phone,
        Mobile = mobile,
        CompanyId = companyId,
        IsPrimaryContact = isPrimary,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "system"
    };

    private static Proposal CreateProposal(
        string proposalNumber,
        Company company,
        Contact? primaryContact,
        string projectName,
        string? projectAddress,
        string? projectDescription,
        DateTime proposalDate,
        DateTime validUntilDate,
        ProposalStatus status,
        decimal discountPercent) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = TenantId,
        ProposalNumber = proposalNumber,
        Version = 1,
        CompanyId = company.Id,
        CompanyName = company.CompanyName,
        PrimaryContactId = primaryContact?.Id,
        PrimaryContactName = primaryContact != null ? $"{primaryContact.FirstName} {primaryContact.LastName}" : null,
        ProjectName = projectName,
        ProjectAddress = projectAddress,
        ProjectDescription = projectDescription,
        ProposalDate = proposalDate,
        ValidUntilDate = validUntilDate,
        Status = status,
        Currency = "EUR",
        VatRate = 23m,
        DiscountPercent = discountPercent,
        PaymentTerms = "30 days from invoice date",
        TermsAndConditions = "Standard terms and conditions apply. Quote valid for 30 days.",
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "system"
    };

    private static ProposalSection CreateSection(
        Guid proposalId,
        string sectionName,
        string? description,
        int sortOrder,
        Guid? sourceKitId) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = TenantId,
        ProposalId = proposalId,
        SectionName = sectionName,
        Description = description,
        SortOrder = sortOrder,
        SourceKitId = sourceKitId,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "system"
    };

    private static ProposalLineItem CreateLineItem(
        Guid sectionId,
        Product product,
        decimal quantity,
        decimal unitPrice,
        decimal unitCost,
        int sortOrder,
        string? notes)
    {
        var lineTotal = quantity * unitPrice;
        var lineCost = quantity * unitCost;
        var lineMargin = lineTotal - lineCost;
        var marginPercent = lineTotal > 0 ? (lineMargin / lineTotal) * 100 : 0;

        return new ProposalLineItem
        {
            Id = Guid.NewGuid(),
            TenantId = TenantId,
            ProposalSectionId = sectionId,
            ProductId = product.Id,
            ProductCode = product.ProductCode,
            Description = product.ProductName,
            Quantity = quantity,
            Unit = product.UnitType ?? "Each",
            UnitCost = unitCost,
            UnitPrice = unitPrice,
            LineTotal = lineTotal,
            LineCost = lineCost,
            LineMargin = lineMargin,
            MarginPercent = marginPercent,
            SortOrder = sortOrder,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };
    }

    private static ProposalLineItem CreateAdHocLineItem(
        Guid sectionId,
        string description,
        string unit,
        decimal quantity,
        decimal unitPrice,
        decimal unitCost,
        int sortOrder,
        string? notes)
    {
        var lineTotal = quantity * unitPrice;
        var lineCost = quantity * unitCost;
        var lineMargin = lineTotal - lineCost;
        var marginPercent = lineTotal > 0 ? (lineMargin / lineTotal) * 100 : 0;

        return new ProposalLineItem
        {
            Id = Guid.NewGuid(),
            TenantId = TenantId,
            ProposalSectionId = sectionId,
            ProductId = null,
            ProductCode = null,
            Description = description,
            Quantity = quantity,
            Unit = unit,
            UnitCost = unitCost,
            UnitPrice = unitPrice,
            LineTotal = lineTotal,
            LineCost = lineCost,
            LineMargin = lineMargin,
            MarginPercent = marginPercent,
            SortOrder = sortOrder,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };
    }

    private static ProposalContact CreateProposalContact(
        Guid proposalId,
        Guid? contactId,
        string contactName,
        string? email,
        string? phone,
        string role,
        bool isPrimary) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = TenantId,
        ProposalId = proposalId,
        ContactId = contactId,
        ContactName = contactName,
        Email = email,
        Phone = phone,
        Role = role,
        IsPrimary = isPrimary,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "system"
    };

    #endregion
}
