using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rascor.Modules.Rams.Domain.Entities;
using Rascor.Modules.Rams.Domain.Enums;

namespace Rascor.Modules.Rams.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds library data for the RAMS module including hazards, control measures,
/// legislation references, and SOPs common to construction work.
/// </summary>
public static class RamsLibrarySeedData
{
    /// <summary>
    /// Default tenant ID for RASCOR (must match Core.Infrastructure.Persistence.DataSeeder)
    /// </summary>
    public static readonly Guid DefaultTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>
    /// Seed all RAMS library data
    /// </summary>
    public static async Task SeedAsync(DbContext context, ILogger logger)
    {
        await SeedHazardsAsync(context, logger);
        await SeedControlMeasuresAsync(context, logger);
        await SeedLegislationAsync(context, logger);
        await SeedSopsAsync(context, logger);
        await SeedHazardControlLinksAsync(context, logger);
    }

    private static async Task SeedHazardsAsync(DbContext context, ILogger logger)
    {
        if (await context.Set<HazardLibrary>().IgnoreQueryFilters().AnyAsync(h => h.TenantId == DefaultTenantId))
        {
            logger.LogInformation("RAMS hazards already exist, skipping");
            return;
        }

        var hazards = new List<HazardLibrary>
        {
            // Working at Height Hazards
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-001",
                Name = "Fall from Height",
                Description = "Risk of falling from elevated work areas including scaffolding, ladders, roofs, or open edges",
                Category = HazardCategory.WorkingAtHeight,
                Keywords = "fall,height,scaffold,ladder,roof,edge,elevated,platform,tower",
                DefaultLikelihood = 3,
                DefaultSeverity = 5,
                TypicalWhoAtRisk = "Employees, Contractors, Visitors",
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-002",
                Name = "Falling Objects",
                Description = "Risk of being struck by tools, materials, or debris falling from height",
                Category = HazardCategory.WorkingAtHeight,
                Keywords = "falling,objects,tools,materials,debris,dropped,overhead",
                DefaultLikelihood = 3,
                DefaultSeverity = 4,
                TypicalWhoAtRisk = "Employees, Contractors, Public",
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-003",
                Name = "Ladder Instability",
                Description = "Risk of ladder slipping, tipping, or collapsing during use",
                Category = HazardCategory.WorkingAtHeight,
                Keywords = "ladder,unstable,slip,tip,collapse,footing",
                DefaultLikelihood = 3,
                DefaultSeverity = 4,
                TypicalWhoAtRisk = "Employees using ladders",
                IsActive = true,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Manual Handling Hazards
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-010",
                Name = "Manual Handling Injury",
                Description = "Risk of musculoskeletal injury from lifting, carrying, pushing, or pulling heavy or awkward loads",
                Category = HazardCategory.ManualHandling,
                Keywords = "lifting,carrying,pushing,pulling,heavy,load,back,strain,sprain",
                DefaultLikelihood = 4,
                DefaultSeverity = 3,
                TypicalWhoAtRisk = "Employees, Contractors",
                IsActive = true,
                SortOrder = 10,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-011",
                Name = "Repetitive Strain",
                Description = "Risk of injury from repetitive movements or prolonged awkward postures",
                Category = HazardCategory.ManualHandling,
                Keywords = "repetitive,strain,posture,ergonomic,RSI,wrist,arm",
                DefaultLikelihood = 3,
                DefaultSeverity = 2,
                TypicalWhoAtRisk = "Employees",
                IsActive = true,
                SortOrder = 11,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Electrical Hazards
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-020",
                Name = "Electric Shock",
                Description = "Risk of electric shock from contact with live electrical conductors or equipment",
                Category = HazardCategory.Electrical,
                Keywords = "electric,shock,live,wire,cable,voltage,electrocution",
                DefaultLikelihood = 2,
                DefaultSeverity = 5,
                TypicalWhoAtRisk = "Electricians, All site personnel",
                IsActive = true,
                SortOrder = 20,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-021",
                Name = "Underground Services",
                Description = "Risk of striking underground electrical cables, gas pipes, or water mains during excavation",
                Category = HazardCategory.Electrical,
                Keywords = "underground,services,cables,pipes,excavation,digging,buried",
                DefaultLikelihood = 3,
                DefaultSeverity = 5,
                TypicalWhoAtRisk = "Excavation workers, All site personnel",
                IsActive = true,
                SortOrder = 21,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-022",
                Name = "Overhead Power Lines",
                Description = "Risk of contact with overhead power lines when using cranes, MEWP, or tall equipment",
                Category = HazardCategory.Electrical,
                Keywords = "overhead,power,lines,crane,MEWP,flashover",
                DefaultLikelihood = 2,
                DefaultSeverity = 5,
                TypicalWhoAtRisk = "Crane operators, MEWP operators, All nearby personnel",
                IsActive = true,
                SortOrder = 22,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Machinery/Equipment Hazards
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-030",
                Name = "Moving Machinery",
                Description = "Risk of entanglement, crushing, or impact from moving parts of machinery",
                Category = HazardCategory.MachineryEquipment,
                Keywords = "machinery,moving,entanglement,crushing,nip,trap,rotating",
                DefaultLikelihood = 3,
                DefaultSeverity = 4,
                TypicalWhoAtRisk = "Machine operators, Nearby workers",
                IsActive = true,
                SortOrder = 30,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-031",
                Name = "Angle Grinder Injury",
                Description = "Risk of cuts, abrasions, or eye injury from angle grinder disc breakage or kickback",
                Category = HazardCategory.MachineryEquipment,
                Keywords = "angle,grinder,cutting,disc,kickback,sparks,abrasive",
                DefaultLikelihood = 3,
                DefaultSeverity = 4,
                TypicalWhoAtRisk = "Operators, Nearby workers",
                IsActive = true,
                SortOrder = 31,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-032",
                Name = "Plant/Vehicle Strike",
                Description = "Risk of being struck by moving vehicles, plant, or mobile equipment",
                Category = HazardCategory.MachineryEquipment,
                Keywords = "vehicle,plant,strike,reversing,forklift,excavator,truck",
                DefaultLikelihood = 3,
                DefaultSeverity = 5,
                TypicalWhoAtRisk = "Pedestrians, All site personnel",
                IsActive = true,
                SortOrder = 32,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Physical Hazards
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-040",
                Name = "Slips, Trips, and Falls",
                Description = "Risk of injury from slipping on wet/contaminated surfaces or tripping over obstacles",
                Category = HazardCategory.Physical,
                Keywords = "slip,trip,fall,wet,floor,obstacle,cable,uneven",
                DefaultLikelihood = 4,
                DefaultSeverity = 2,
                TypicalWhoAtRisk = "All site personnel, Visitors",
                IsActive = true,
                SortOrder = 40,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-041",
                Name = "Noise Exposure",
                Description = "Risk of hearing damage from exposure to high noise levels",
                Category = HazardCategory.Physical,
                Keywords = "noise,hearing,decibel,loud,ear,damage",
                DefaultLikelihood = 4,
                DefaultSeverity = 3,
                TypicalWhoAtRisk = "Employees in noisy areas",
                IsActive = true,
                SortOrder = 41,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-042",
                Name = "Hand-Arm Vibration",
                Description = "Risk of vibration-related injury from prolonged use of vibrating tools",
                Category = HazardCategory.Physical,
                Keywords = "vibration,HAVS,hand,arm,tool,white finger",
                DefaultLikelihood = 3,
                DefaultSeverity = 3,
                TypicalWhoAtRisk = "Power tool operators",
                IsActive = true,
                SortOrder = 42,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-043",
                Name = "Struck by Object",
                Description = "Risk of being struck by flying, swinging, or rolling objects",
                Category = HazardCategory.Physical,
                Keywords = "struck,hit,flying,swing,impact,projectile",
                DefaultLikelihood = 3,
                DefaultSeverity = 3,
                TypicalWhoAtRisk = "All site personnel",
                IsActive = true,
                SortOrder = 43,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Fire Hazards
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-050",
                Name = "Fire from Hot Works",
                Description = "Risk of fire from welding, cutting, grinding, or other hot work activities",
                Category = HazardCategory.Fire,
                Keywords = "fire,hot,work,welding,cutting,sparks,ignition",
                DefaultLikelihood = 3,
                DefaultSeverity = 4,
                TypicalWhoAtRisk = "Hot work operatives, Nearby workers",
                IsActive = true,
                SortOrder = 50,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-051",
                Name = "Fire - Flammable Materials",
                Description = "Risk of fire from storage or use of flammable liquids, gases, or materials",
                Category = HazardCategory.Fire,
                Keywords = "fire,flammable,liquid,gas,fuel,petrol,diesel,storage",
                DefaultLikelihood = 2,
                DefaultSeverity = 4,
                TypicalWhoAtRisk = "All site personnel",
                IsActive = true,
                SortOrder = 51,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Chemical Hazards
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-060",
                Name = "Hazardous Substances",
                Description = "Risk of injury or illness from contact with, inhalation of, or ingestion of hazardous substances",
                Category = HazardCategory.Chemical,
                Keywords = "chemical,hazardous,COSHH,substance,toxic,corrosive",
                DefaultLikelihood = 3,
                DefaultSeverity = 4,
                TypicalWhoAtRisk = "Workers handling chemicals",
                IsActive = true,
                SortOrder = 60,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-061",
                Name = "Silica Dust Exposure",
                Description = "Risk of silicosis from inhalation of respirable crystalline silica during cutting, drilling, or grinding",
                Category = HazardCategory.Chemical,
                Keywords = "silica,dust,RCS,cutting,drilling,grinding,concrete,stone",
                DefaultLikelihood = 4,
                DefaultSeverity = 4,
                TypicalWhoAtRisk = "Cutting/drilling operatives, Nearby workers",
                IsActive = true,
                SortOrder = 61,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-062",
                Name = "Asbestos Exposure",
                Description = "Risk of asbestos-related disease from disturbing asbestos-containing materials",
                Category = HazardCategory.Chemical,
                Keywords = "asbestos,ACM,mesothelioma,refurbishment,demolition",
                DefaultLikelihood = 2,
                DefaultSeverity = 5,
                TypicalWhoAtRisk = "All workers in refurbishment/demolition",
                IsActive = true,
                SortOrder = 62,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-063",
                Name = "Cement Burns",
                Description = "Risk of skin burns and dermatitis from contact with wet cement or concrete",
                Category = HazardCategory.Chemical,
                Keywords = "cement,concrete,burn,dermatitis,alkaline,wet",
                DefaultLikelihood = 3,
                DefaultSeverity = 3,
                TypicalWhoAtRisk = "Concrete workers, Bricklayers",
                IsActive = true,
                SortOrder = 63,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Environmental Hazards
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-070",
                Name = "Excavation Collapse",
                Description = "Risk of being trapped or buried by collapse of excavation walls",
                Category = HazardCategory.Environmental,
                Keywords = "excavation,collapse,trench,buried,cave-in,shoring",
                DefaultLikelihood = 2,
                DefaultSeverity = 5,
                TypicalWhoAtRisk = "Excavation workers, Nearby personnel",
                IsActive = true,
                SortOrder = 70,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-071",
                Name = "Confined Space",
                Description = "Risk of asphyxiation, poisoning, or drowning in confined spaces",
                Category = HazardCategory.Environmental,
                Keywords = "confined,space,asphyxiation,oxygen,toxic,atmosphere",
                DefaultLikelihood = 2,
                DefaultSeverity = 5,
                TypicalWhoAtRisk = "Workers entering confined spaces",
                IsActive = true,
                SortOrder = 71,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-072",
                Name = "Adverse Weather",
                Description = "Risk of injury from working in extreme heat, cold, wind, or lightning",
                Category = HazardCategory.Environmental,
                Keywords = "weather,heat,cold,wind,lightning,storm,rain",
                DefaultLikelihood = 3,
                DefaultSeverity = 3,
                TypicalWhoAtRisk = "All outdoor workers",
                IsActive = true,
                SortOrder = 72,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Ergonomic Hazards
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-080",
                Name = "Poor Workstation Setup",
                Description = "Risk of musculoskeletal disorders from poorly designed workstations or prolonged static postures",
                Category = HazardCategory.Ergonomic,
                Keywords = "workstation,posture,DSE,VDU,desk,chair,ergonomic",
                DefaultLikelihood = 3,
                DefaultSeverity = 2,
                TypicalWhoAtRisk = "Office workers, Site office staff",
                IsActive = true,
                SortOrder = 80,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Psychological Hazards
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-090",
                Name = "Work-Related Stress",
                Description = "Risk of stress-related illness from excessive workload, time pressure, or workplace conflict",
                Category = HazardCategory.Psychological,
                Keywords = "stress,workload,pressure,deadline,mental,health",
                DefaultLikelihood = 3,
                DefaultSeverity = 3,
                TypicalWhoAtRisk = "All employees",
                IsActive = true,
                SortOrder = 90,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-091",
                Name = "Lone Working",
                Description = "Risk of delayed assistance in case of accident or illness when working alone",
                Category = HazardCategory.Psychological,
                Keywords = "lone,working,alone,isolated,remote,assistance",
                DefaultLikelihood = 3,
                DefaultSeverity = 3,
                TypicalWhoAtRisk = "Lone workers",
                IsActive = true,
                SortOrder = 91,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Biological Hazards
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HAZ-100",
                Name = "Leptospirosis (Weil's Disease)",
                Description = "Risk of infection from contact with water contaminated by rat urine",
                Category = HazardCategory.Biological,
                Keywords = "leptospirosis,weil,rat,urine,water,contaminated",
                DefaultLikelihood = 2,
                DefaultSeverity = 4,
                TypicalWhoAtRisk = "Workers near water, excavation workers",
                IsActive = true,
                SortOrder = 100,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            }
        };

        await context.Set<HazardLibrary>().AddRangeAsync(hazards);
        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} RAMS hazards", hazards.Count);
    }

    private static async Task SeedControlMeasuresAsync(DbContext context, ILogger logger)
    {
        if (await context.Set<ControlMeasureLibrary>().IgnoreQueryFilters().AnyAsync(c => c.TenantId == DefaultTenantId))
        {
            logger.LogInformation("RAMS control measures already exist, skipping");
            return;
        }

        var controls = new List<ControlMeasureLibrary>
        {
            // Engineering Controls - Working at Height
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-001",
                Name = "Edge Protection",
                Description = "Install guardrails, toe boards, and intermediate rails at open edges. Guardrails minimum 950mm high with 470mm gap max between rails.",
                Hierarchy = ControlHierarchy.Engineering,
                ApplicableToCategory = HazardCategory.WorkingAtHeight,
                Keywords = "guardrail,edge,protection,barrier,toeboard,handrail",
                TypicalLikelihoodReduction = 2,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-002",
                Name = "Scaffold with Full Boarding",
                Description = "Use fully boarded scaffold platforms with double guardrails, toe boards, and safe access. Scaffold to be erected and inspected by competent person.",
                Hierarchy = ControlHierarchy.Engineering,
                ApplicableToCategory = HazardCategory.WorkingAtHeight,
                Keywords = "scaffold,platform,boarding,tower,access",
                TypicalLikelihoodReduction = 2,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-003",
                Name = "MEWP (Mobile Elevating Work Platform)",
                Description = "Use cherry picker, scissor lift, or boom lift for work at height. Operator must be trained and platform inspected before use.",
                Hierarchy = ControlHierarchy.Engineering,
                ApplicableToCategory = HazardCategory.WorkingAtHeight,
                Keywords = "MEWP,cherry picker,scissor lift,boom,elevated,platform",
                TypicalLikelihoodReduction = 2,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-004",
                Name = "Safety Netting",
                Description = "Install safety nets below work area to arrest falls. Nets to comply with EN 1263-1 and be installed by competent rigger.",
                Hierarchy = ControlHierarchy.Engineering,
                ApplicableToCategory = HazardCategory.WorkingAtHeight,
                Keywords = "net,safety,fall,arrest,catch",
                TypicalLikelihoodReduction = 0,
                TypicalSeverityReduction = 2,
                IsActive = true,
                SortOrder = 4,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Elimination Controls
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-010",
                Name = "Work at Ground Level",
                Description = "Where possible, prefabricate components at ground level to eliminate the need for work at height.",
                Hierarchy = ControlHierarchy.Elimination,
                ApplicableToCategory = HazardCategory.WorkingAtHeight,
                Keywords = "ground,level,eliminate,prefabricate,assembly",
                TypicalLikelihoodReduction = 3,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 10,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Manual Handling Controls
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-020",
                Name = "Mechanical Lifting Equipment",
                Description = "Use forklift, crane, pallet truck, or hoist to eliminate manual handling where possible. Equipment to be inspected and operated by trained personnel.",
                Hierarchy = ControlHierarchy.Engineering,
                ApplicableToCategory = HazardCategory.ManualHandling,
                Keywords = "forklift,crane,hoist,pallet,mechanical,lifting",
                TypicalLikelihoodReduction = 2,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 20,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-021",
                Name = "Manual Handling Training",
                Description = "All personnel to receive manual handling training covering correct lifting techniques, assessing loads, and when to seek assistance.",
                Hierarchy = ControlHierarchy.Administrative,
                ApplicableToCategory = HazardCategory.ManualHandling,
                Keywords = "training,manual,handling,lifting,technique",
                TypicalLikelihoodReduction = 1,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 21,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-022",
                Name = "Team Lifting",
                Description = "Use two-person or team lifting for heavy or awkward loads. Brief team on coordination before lift.",
                Hierarchy = ControlHierarchy.Administrative,
                ApplicableToCategory = HazardCategory.ManualHandling,
                Keywords = "team,lifting,two,person,coordination",
                TypicalLikelihoodReduction = 1,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 22,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Electrical Controls
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-030",
                Name = "Isolation and Lockout/Tagout",
                Description = "Isolate electrical supply and apply lockout/tagout before any electrical work. Verify dead using approved voltage indicator.",
                Hierarchy = ControlHierarchy.Engineering,
                ApplicableToCategory = HazardCategory.Electrical,
                Keywords = "isolation,lockout,tagout,LOTO,dead,verify",
                TypicalLikelihoodReduction = 2,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 30,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-031",
                Name = "110V Reduced Voltage",
                Description = "Use 110V CTE (Centre Tapped to Earth) tools and equipment on construction sites to reduce shock severity.",
                Hierarchy = ControlHierarchy.Engineering,
                ApplicableToCategory = HazardCategory.Electrical,
                Keywords = "110V,reduced,voltage,CTE,transformer",
                TypicalLikelihoodReduction = 0,
                TypicalSeverityReduction = 2,
                IsActive = true,
                SortOrder = 31,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-032",
                Name = "CAT and Genny Survey",
                Description = "Use Cable Avoidance Tool (CAT) and signal generator before excavation. Mark up located services. Hand dig within 500mm of services.",
                Hierarchy = ControlHierarchy.Engineering,
                ApplicableToCategory = HazardCategory.Electrical,
                Keywords = "CAT,genny,survey,underground,services,scan",
                TypicalLikelihoodReduction = 2,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 32,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Machinery Controls
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-040",
                Name = "Machine Guarding",
                Description = "Ensure all machine guards are in place and functional. Do not operate machinery with guards removed or bypassed.",
                Hierarchy = ControlHierarchy.Engineering,
                ApplicableToCategory = HazardCategory.MachineryEquipment,
                Keywords = "guard,machine,safety,interlock,barrier",
                TypicalLikelihoodReduction = 2,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 40,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-041",
                Name = "Vehicle/Pedestrian Segregation",
                Description = "Implement physical barriers, designated walkways, and separate access routes to keep pedestrians away from moving vehicles and plant.",
                Hierarchy = ControlHierarchy.Engineering,
                ApplicableToCategory = HazardCategory.MachineryEquipment,
                Keywords = "segregation,pedestrian,vehicle,barrier,walkway",
                TypicalLikelihoodReduction = 2,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 41,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-042",
                Name = "Banksman/Spotter",
                Description = "Use trained banksman to direct reversing vehicles and plant. Maintain eye contact and use agreed signals.",
                Hierarchy = ControlHierarchy.Administrative,
                ApplicableToCategory = HazardCategory.MachineryEquipment,
                Keywords = "banksman,spotter,reversing,signals,vehicle",
                TypicalLikelihoodReduction = 1,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 42,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Physical Hazard Controls
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-050",
                Name = "Good Housekeeping",
                Description = "Maintain clean and tidy work areas. Clear walkways of obstacles, manage cables with covers or overhead routes, clean spills immediately.",
                Hierarchy = ControlHierarchy.Administrative,
                ApplicableToCategory = HazardCategory.Physical,
                Keywords = "housekeeping,tidy,clean,clear,walkway,cable",
                TypicalLikelihoodReduction = 2,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 50,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-051",
                Name = "Hearing Protection",
                Description = "Provide and wear appropriate hearing protection (ear defenders or plugs) when noise levels exceed 85 dB(A) action level.",
                Hierarchy = ControlHierarchy.PPE,
                ApplicableToCategory = HazardCategory.Physical,
                Keywords = "hearing,protection,ear,defenders,plugs,noise",
                TypicalLikelihoodReduction = 0,
                TypicalSeverityReduction = 2,
                IsActive = true,
                SortOrder = 51,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Fire Controls
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-060",
                Name = "Hot Work Permit",
                Description = "Obtain hot work permit before welding, cutting, or grinding. Clear combustibles, post fire watch, have extinguisher available.",
                Hierarchy = ControlHierarchy.Administrative,
                ApplicableToCategory = HazardCategory.Fire,
                Keywords = "hot,work,permit,welding,fire,watch",
                TypicalLikelihoodReduction = 2,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 60,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-061",
                Name = "Fire Extinguishers",
                Description = "Provide appropriate fire extinguishers at all hot work locations and in storage areas. Personnel to be trained in use.",
                Hierarchy = ControlHierarchy.Engineering,
                ApplicableToCategory = HazardCategory.Fire,
                Keywords = "fire,extinguisher,firefighting,suppression",
                TypicalLikelihoodReduction = 0,
                TypicalSeverityReduction = 2,
                IsActive = true,
                SortOrder = 61,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Chemical Controls
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-070",
                Name = "COSHH Assessment",
                Description = "Complete COSHH assessment for all hazardous substances. Review SDS, identify controls, brief workers on hazards and precautions.",
                Hierarchy = ControlHierarchy.Administrative,
                ApplicableToCategory = HazardCategory.Chemical,
                Keywords = "COSHH,assessment,SDS,hazardous,substance",
                TypicalLikelihoodReduction = 1,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 70,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-071",
                Name = "Dust Suppression/Extraction",
                Description = "Use water suppression or on-tool extraction when cutting, drilling, or grinding to control silica and other dust.",
                Hierarchy = ControlHierarchy.Engineering,
                ApplicableToCategory = HazardCategory.Chemical,
                Keywords = "dust,suppression,extraction,water,silica,control",
                TypicalLikelihoodReduction = 2,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 71,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-072",
                Name = "Respiratory Protection (FFP3)",
                Description = "Wear FFP3 mask or powered respirator when dust controls are insufficient. Face-fit test required for tight-fitting masks.",
                Hierarchy = ControlHierarchy.PPE,
                ApplicableToCategory = HazardCategory.Chemical,
                Keywords = "respiratory,mask,FFP3,respirator,dust,breathing",
                TypicalLikelihoodReduction = 0,
                TypicalSeverityReduction = 2,
                IsActive = true,
                SortOrder = 72,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Environmental Controls
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-080",
                Name = "Excavation Shoring/Battering",
                Description = "Support excavation sides with shoring, trench boxes, or batter back sides to safe angle. Inspect daily and after rain.",
                Hierarchy = ControlHierarchy.Engineering,
                ApplicableToCategory = HazardCategory.Environmental,
                Keywords = "shoring,trench,box,batter,excavation,support",
                TypicalLikelihoodReduction = 2,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 80,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-081",
                Name = "Confined Space Permit and Rescue Plan",
                Description = "Obtain permit to work, test atmosphere, provide ventilation, have rescue plan and equipment available. Never enter alone.",
                Hierarchy = ControlHierarchy.Administrative,
                ApplicableToCategory = HazardCategory.Environmental,
                Keywords = "confined,space,permit,atmosphere,rescue,ventilation",
                TypicalLikelihoodReduction = 2,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 81,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // PPE - General
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-090",
                Name = "Safety Helmet",
                Description = "Wear appropriate safety helmet (hard hat) at all times in construction areas. Replace if damaged or after impact.",
                Hierarchy = ControlHierarchy.PPE,
                ApplicableToCategory = null,
                Keywords = "helmet,hard,hat,head,protection",
                TypicalLikelihoodReduction = 0,
                TypicalSeverityReduction = 1,
                IsActive = true,
                SortOrder = 90,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-091",
                Name = "Safety Footwear",
                Description = "Wear safety boots with steel toe caps and midsole protection on all construction sites.",
                Hierarchy = ControlHierarchy.PPE,
                ApplicableToCategory = null,
                Keywords = "boots,footwear,steel,toe,cap,safety",
                TypicalLikelihoodReduction = 0,
                TypicalSeverityReduction = 1,
                IsActive = true,
                SortOrder = 91,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-092",
                Name = "Hi-Visibility Clothing",
                Description = "Wear hi-vis vest, jacket, or coveralls meeting EN ISO 20471 Class 2 minimum on all construction sites.",
                Hierarchy = ControlHierarchy.PPE,
                ApplicableToCategory = null,
                Keywords = "hi-vis,high,visibility,vest,jacket,reflective",
                TypicalLikelihoodReduction = 1,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 92,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-093",
                Name = "Safety Glasses/Goggles",
                Description = "Wear appropriate eye protection (safety glasses or goggles) for all cutting, grinding, drilling, and hammering operations.",
                Hierarchy = ControlHierarchy.PPE,
                ApplicableToCategory = null,
                Keywords = "glasses,goggles,eye,protection,safety",
                TypicalLikelihoodReduction = 0,
                TypicalSeverityReduction = 2,
                IsActive = true,
                SortOrder = 93,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-094",
                Name = "Work Gloves",
                Description = "Wear appropriate work gloves for the task - cut-resistant for handling sharp materials, chemical-resistant for hazardous substances.",
                Hierarchy = ControlHierarchy.PPE,
                ApplicableToCategory = null,
                Keywords = "gloves,hand,protection,cut,resistant",
                TypicalLikelihoodReduction = 0,
                TypicalSeverityReduction = 1,
                IsActive = true,
                SortOrder = 94,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-095",
                Name = "Fall Arrest Harness",
                Description = "Wear full body harness with lanyard attached to suitable anchor point when working at height where collective protection is not feasible.",
                Hierarchy = ControlHierarchy.PPE,
                ApplicableToCategory = HazardCategory.WorkingAtHeight,
                Keywords = "harness,fall,arrest,lanyard,anchor",
                TypicalLikelihoodReduction = 0,
                TypicalSeverityReduction = 2,
                IsActive = true,
                SortOrder = 95,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // Administrative - Training and Competence
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-100",
                Name = "Site Induction",
                Description = "All personnel to complete site induction before commencing work. Covers site rules, hazards, emergency procedures, and welfare facilities.",
                Hierarchy = ControlHierarchy.Administrative,
                ApplicableToCategory = null,
                Keywords = "induction,training,site,rules,briefing",
                TypicalLikelihoodReduction = 1,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 100,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-101",
                Name = "Toolbox Talk",
                Description = "Conduct task-specific toolbox talk before high-risk activities. Ensure all workers understand hazards and control measures.",
                Hierarchy = ControlHierarchy.Administrative,
                ApplicableToCategory = null,
                Keywords = "toolbox,talk,briefing,task,specific",
                TypicalLikelihoodReduction = 1,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 101,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CTL-102",
                Name = "Competent Person/Supervision",
                Description = "Ensure work is carried out by or under supervision of competent persons with appropriate training and experience.",
                Hierarchy = ControlHierarchy.Administrative,
                ApplicableToCategory = null,
                Keywords = "competent,person,supervision,trained,qualified",
                TypicalLikelihoodReduction = 1,
                TypicalSeverityReduction = 0,
                IsActive = true,
                SortOrder = 102,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            }
        };

        await context.Set<ControlMeasureLibrary>().AddRangeAsync(controls);
        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} RAMS control measures", controls.Count);
    }

    private static async Task SeedLegislationAsync(DbContext context, ILogger logger)
    {
        if (await context.Set<LegislationReference>().IgnoreQueryFilters().AnyAsync(l => l.TenantId == DefaultTenantId))
        {
            logger.LogInformation("RAMS legislation already exists, skipping");
            return;
        }

        var legislation = new List<LegislationReference>
        {
            // Irish Legislation
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "SHWWA-2005",
                Name = "Safety, Health and Welfare at Work Act 2005",
                ShortName = "SHWWA 2005",
                Description = "Primary Irish health and safety legislation setting out duties of employers, employees, and others. Requires risk assessment and safety statement.",
                Jurisdiction = "Ireland",
                Keywords = "general,duties,employer,employee,safety,statement,risk,assessment",
                DocumentUrl = "https://www.irishstatutebook.ie/eli/2005/act/10/enacted/en/html",
                ApplicableCategories = null,
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "SHWW-CONST-2013",
                Name = "Safety, Health and Welfare at Work (Construction) Regulations 2013",
                ShortName = "Construction Regs 2013",
                Description = "Irish regulations specific to construction work. Covers duties of clients, designers, contractors, and workers. Requires safety file and coordination.",
                Jurisdiction = "Ireland",
                Keywords = "construction,contractor,client,designer,safety,file,PSCS,PSDP",
                DocumentUrl = "https://www.irishstatutebook.ie/eli/2013/si/291/made/en/print",
                ApplicableCategories = null,
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "SHWW-GEN-2007",
                Name = "Safety, Health and Welfare at Work (General Application) Regulations 2007",
                ShortName = "General Application Regs",
                Description = "Irish regulations covering workplace requirements, work equipment, PPE, manual handling, DSE, electricity, work at height, and more.",
                Jurisdiction = "Ireland",
                Keywords = "general,application,equipment,PPE,manual,handling,height,electricity",
                DocumentUrl = "https://www.irishstatutebook.ie/eli/2007/si/299/made/en/print",
                ApplicableCategories = null,
                IsActive = true,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "SHWW-CHEM-2001",
                Name = "Safety, Health and Welfare at Work (Chemical Agents) Regulations 2001",
                ShortName = "Chemical Agents Regs",
                Description = "Irish regulations on controlling exposure to hazardous chemical agents in the workplace. Equivalent to UK COSHH.",
                Jurisdiction = "Ireland",
                Keywords = "chemical,COSHH,hazardous,substance,exposure,OEL",
                DocumentUrl = "https://www.irishstatutebook.ie/eli/2001/si/619/made/en/print",
                ApplicableCategories = "Chemical",
                IsActive = true,
                SortOrder = 4,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // UK Legislation (for reference/cross-border work)
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "HASAWA-1974",
                Name = "Health and Safety at Work etc. Act 1974",
                ShortName = "HASAWA",
                Description = "Primary UK health and safety legislation. Sets out general duties of employers to employees and others, and duties of employees.",
                Jurisdiction = "UK",
                Keywords = "general,duties,employer,employee,UK",
                DocumentUrl = "https://www.legislation.gov.uk/ukpga/1974/37/contents",
                ApplicableCategories = null,
                IsActive = true,
                SortOrder = 10,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CDM-2015",
                Name = "Construction (Design and Management) Regulations 2015",
                ShortName = "CDM 2015",
                Description = "UK regulations for managing health and safety in construction. Covers duties of clients, designers, principal contractors, and workers.",
                Jurisdiction = "UK",
                Keywords = "construction,CDM,client,designer,principal,contractor",
                DocumentUrl = "https://www.legislation.gov.uk/uksi/2015/51/contents",
                ApplicableCategories = null,
                IsActive = true,
                SortOrder = 11,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "WAHR-2005",
                Name = "Work at Height Regulations 2005",
                ShortName = "WAHR 2005",
                Description = "UK regulations on preventing falls from height. Requires hierarchy of controls: avoid, prevent, mitigate.",
                Jurisdiction = "UK",
                Keywords = "height,fall,ladder,scaffold,MEWP,edge,protection",
                DocumentUrl = "https://www.legislation.gov.uk/uksi/2005/735/contents",
                ApplicableCategories = "WorkingAtHeight",
                IsActive = true,
                SortOrder = 12,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "MHOR-1992",
                Name = "Manual Handling Operations Regulations 1992",
                ShortName = "MHOR",
                Description = "UK regulations requiring employers to avoid hazardous manual handling, assess risks, and reduce risk of injury.",
                Jurisdiction = "UK",
                Keywords = "manual,handling,lifting,carrying,pushing,pulling",
                DocumentUrl = "https://www.legislation.gov.uk/uksi/1992/2793/contents",
                ApplicableCategories = "ManualHandling",
                IsActive = true,
                SortOrder = 13,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "COSHH-2002",
                Name = "Control of Substances Hazardous to Health Regulations 2002",
                ShortName = "COSHH",
                Description = "UK regulations on preventing or controlling exposure to hazardous substances. Requires assessment and control measures.",
                Jurisdiction = "UK",
                Keywords = "COSHH,chemical,hazardous,substance,exposure,control",
                DocumentUrl = "https://www.legislation.gov.uk/uksi/2002/2677/contents",
                ApplicableCategories = "Chemical",
                IsActive = true,
                SortOrder = 14,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "PUWER-1998",
                Name = "Provision and Use of Work Equipment Regulations 1998",
                ShortName = "PUWER",
                Description = "UK regulations on safe provision and use of work equipment. Covers suitability, maintenance, inspection, and training.",
                Jurisdiction = "UK",
                Keywords = "equipment,machinery,tool,maintenance,inspection,guard",
                DocumentUrl = "https://www.legislation.gov.uk/uksi/1998/2306/contents",
                ApplicableCategories = "MachineryEquipment",
                IsActive = true,
                SortOrder = 15,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "LOLER-1998",
                Name = "Lifting Operations and Lifting Equipment Regulations 1998",
                ShortName = "LOLER",
                Description = "UK regulations on safe use of lifting equipment. Requires planning, competent persons, and thorough examination.",
                Jurisdiction = "UK",
                Keywords = "lifting,crane,hoist,sling,examination,competent",
                DocumentUrl = "https://www.legislation.gov.uk/uksi/1998/2307/contents",
                ApplicableCategories = "MachineryEquipment,ManualHandling",
                IsActive = true,
                SortOrder = 16,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "EAWR-1989",
                Name = "Electricity at Work Regulations 1989",
                ShortName = "EAWR",
                Description = "UK regulations on electrical safety. Requires safe systems of work, competent persons, and suitable equipment.",
                Jurisdiction = "UK",
                Keywords = "electricity,electrical,shock,isolation,live,work",
                DocumentUrl = "https://www.legislation.gov.uk/uksi/1989/635/contents",
                ApplicableCategories = "Electrical",
                IsActive = true,
                SortOrder = 17,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "NOISE-2005",
                Name = "Control of Noise at Work Regulations 2005",
                ShortName = "Noise Regs",
                Description = "UK regulations on controlling noise exposure. Sets exposure action values and limit values.",
                Jurisdiction = "UK",
                Keywords = "noise,hearing,decibel,exposure,action,limit",
                DocumentUrl = "https://www.legislation.gov.uk/uksi/2005/1643/contents",
                ApplicableCategories = "Physical",
                IsActive = true,
                SortOrder = 18,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                Code = "CONFINED-1997",
                Name = "Confined Spaces Regulations 1997",
                ShortName = "Confined Spaces Regs",
                Description = "UK regulations on working in confined spaces. Requires avoidance, safe system of work, and emergency arrangements.",
                Jurisdiction = "UK",
                Keywords = "confined,space,entry,atmosphere,rescue,ventilation",
                DocumentUrl = "https://www.legislation.gov.uk/uksi/1997/1713/contents",
                ApplicableCategories = "Environmental",
                IsActive = true,
                SortOrder = 19,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            }
        };

        await context.Set<LegislationReference>().AddRangeAsync(legislation);
        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} RAMS legislation references", legislation.Count);
    }

    private static async Task SeedSopsAsync(DbContext context, ILogger logger)
    {
        if (await context.Set<SopReference>().IgnoreQueryFilters().AnyAsync(s => s.TenantId == DefaultTenantId))
        {
            logger.LogInformation("RAMS SOPs already exist, skipping");
            return;
        }

        var sops = new List<SopReference>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                SopId = "SOP-001",
                Topic = "Working at Height",
                Description = "Standard operating procedure for all work at height activities including ladder use, scaffold access, and MEWP operation.",
                TaskKeywords = "height,ladder,scaffold,MEWP,roof,elevated,platform",
                PolicySnippet = "No work at height shall commence without a completed task-specific risk assessment. Edge protection must be in place before work begins.",
                ProcedureDetails = "1. Complete task-specific risk assessment\n2. Ensure edge protection is installed\n3. Inspect access equipment before use\n4. Use fall arrest equipment where required\n5. Do not work in adverse weather conditions\n6. Maintain 3-points of contact on ladders",
                ApplicableLegislation = "SHWW-GEN-2007, WAHR-2005",
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                SopId = "SOP-002",
                Topic = "Manual Handling",
                Description = "Standard operating procedure for safe manual handling of loads on construction sites.",
                TaskKeywords = "lifting,carrying,pushing,pulling,manual,handling,load",
                PolicySnippet = "Mechanical aids shall be used wherever reasonably practicable. No single person lift over 25kg without assessment.",
                ProcedureDetails = "1. Assess the load - weight, size, grip points\n2. Plan the lift - route, destination, obstacles\n3. Use mechanical aids where possible\n4. Get help for heavy or awkward loads\n5. Use correct lifting technique - bend knees, straight back\n6. Don't twist while lifting",
                ApplicableLegislation = "SHWW-GEN-2007, MHOR-1992",
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                SopId = "SOP-003",
                Topic = "Electrical Safety",
                Description = "Standard operating procedure for electrical work and working near electrical installations.",
                TaskKeywords = "electrical,electric,cable,wire,isolation,voltage",
                PolicySnippet = "All electrical work to be carried out by competent electricians. Isolation and lockout/tagout required for all work on electrical systems.",
                ProcedureDetails = "1. Only competent electricians to work on electrical systems\n2. Isolate supply and apply lockout/tagout\n3. Prove dead with approved voltage indicator\n4. Use 110V tools on site\n5. Inspect cables and equipment before use\n6. Report damaged cables immediately",
                ApplicableLegislation = "SHWW-GEN-2007, EAWR-1989",
                IsActive = true,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                SopId = "SOP-004",
                Topic = "Excavation Safety",
                Description = "Standard operating procedure for excavation work including trenching and service detection.",
                TaskKeywords = "excavation,trench,dig,underground,services,shoring",
                PolicySnippet = "All excavations to be supported or battered back. CAT scan required before any ground breaking. Hand dig within 500mm of services.",
                ProcedureDetails = "1. Obtain service drawings and conduct CAT/Genny survey\n2. Mark up all located services\n3. Plan support method - shoring, battering, trench box\n4. Hand dig within 500mm of services\n5. Install edge protection around excavations\n6. Inspect excavation daily and after rain\n7. Provide safe access/egress (ladder within 15m)",
                ApplicableLegislation = "SHWW-CONST-2013",
                IsActive = true,
                SortOrder = 4,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                SopId = "SOP-005",
                Topic = "Hot Work",
                Description = "Standard operating procedure for welding, cutting, grinding, and other hot work activities.",
                TaskKeywords = "welding,cutting,grinding,hot,work,sparks,fire",
                PolicySnippet = "Hot work permit required for all welding, cutting, and grinding operations. Fire extinguisher and fire watch mandatory.",
                ProcedureDetails = "1. Obtain hot work permit from site manager\n2. Clear combustible materials from area (10m radius)\n3. Provide suitable fire extinguisher\n4. Ensure adequate ventilation\n5. Use welding screens to protect others\n6. Post fire watch during and 30 mins after work\n7. Inspect area before leaving",
                ApplicableLegislation = "SHWW-GEN-2007",
                IsActive = true,
                SortOrder = 5,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                SopId = "SOP-006",
                Topic = "Confined Space Entry",
                Description = "Standard operating procedure for entry into confined spaces.",
                TaskKeywords = "confined,space,entry,tank,vessel,sewer,manhole",
                PolicySnippet = "Entry to confined spaces only with permit to work and rescue plan. Atmospheric testing mandatory before and during entry.",
                ProcedureDetails = "1. Can the work be done without entry? If not:\n2. Obtain confined space permit\n3. Test atmosphere - O2 (19.5-23.5%), LEL (<10%), toxics\n4. Ventilate space adequately\n5. Establish rescue plan and have rescue equipment ready\n6. Use attendant/top man at all times\n7. Maintain communication throughout\n8. Continuous or frequent atmospheric monitoring",
                ApplicableLegislation = "SHWW-GEN-2007, CONFINED-1997",
                IsActive = true,
                SortOrder = 6,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                SopId = "SOP-007",
                Topic = "Lifting Operations",
                Description = "Standard operating procedure for crane lifts and other lifting operations.",
                TaskKeywords = "crane,lifting,hoist,sling,rigging,load",
                PolicySnippet = "All lifting operations to be planned by competent person. Lifting equipment to have valid LOLER examination certificate.",
                ProcedureDetails = "1. Appoint competent Appointed Person to plan lift\n2. Prepare lift plan for complex/high-risk lifts\n3. Check lifting equipment has valid examination\n4. Brief all involved - crane operator, slinger, banksman\n5. Establish exclusion zone under lift\n6. Check load weight and select appropriate equipment\n7. Use tag lines to control load\n8. Never walk under suspended loads",
                ApplicableLegislation = "SHWW-GEN-2007, LOLER-1998",
                IsActive = true,
                SortOrder = 7,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                SopId = "SOP-008",
                Topic = "Dust Control (Silica)",
                Description = "Standard operating procedure for controlling exposure to respirable crystalline silica (RCS) dust.",
                TaskKeywords = "dust,silica,RCS,cutting,drilling,grinding,concrete,stone",
                PolicySnippet = "Water suppression or on-tool extraction mandatory for all cutting, drilling, or grinding of silica-containing materials.",
                ProcedureDetails = "1. Use wet cutting/drilling where possible\n2. Use tools with on-tool extraction (H-class vacuum)\n3. Dampen down work area to suppress dust\n4. Wear RPE (FFP3 minimum) when controls insufficient\n5. Face-fit test required for tight-fitting RPE\n6. Clean up using H-class vacuum, not dry sweeping\n7. Maintain good personal hygiene - wash before eating",
                ApplicableLegislation = "SHWW-CHEM-2001, COSHH-2002",
                IsActive = true,
                SortOrder = 8,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                SopId = "SOP-009",
                Topic = "Angle Grinder Use",
                Description = "Standard operating procedure for safe use of angle grinders and abrasive wheels.",
                TaskKeywords = "angle,grinder,cutting,disc,abrasive,wheel",
                PolicySnippet = "Only trained operatives to use angle grinders. Guard must be fitted and disc appropriate for the task.",
                ProcedureDetails = "1. Only trained operatives to use angle grinders\n2. Inspect grinder and disc before use\n3. Ensure guard is correctly fitted\n4. Use correct disc for the task (cutting vs grinding)\n5. Check disc speed rating matches grinder\n6. Wear safety glasses, face shield, and gloves\n7. Secure workpiece before cutting\n8. Allow disc to reach full speed before cutting\n9. Do not force the disc\n10. Store discs flat and dry",
                ApplicableLegislation = "SHWW-GEN-2007, PUWER-1998",
                IsActive = true,
                SortOrder = 9,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = DefaultTenantId,
                SopId = "SOP-010",
                Topic = "Site Traffic Management",
                Description = "Standard operating procedure for managing vehicles and pedestrians on construction sites.",
                TaskKeywords = "vehicle,traffic,pedestrian,reversing,plant,delivery",
                PolicySnippet = "Segregation of vehicles and pedestrians is mandatory. All reversing to be controlled by banksman or reversing cameras/sensors.",
                ProcedureDetails = "1. Establish separate vehicle and pedestrian routes\n2. Install physical barriers where possible\n3. Mark pedestrian walkways clearly\n4. Control site access with gateman\n5. Implement one-way systems where feasible\n6. Use banksmen for reversing vehicles\n7. Ensure all plant has audible reversing alarm\n8. Enforce site speed limits (typically 5-10 mph)\n9. Brief all drivers on site rules",
                ApplicableLegislation = "SHWW-CONST-2013",
                IsActive = true,
                SortOrder = 10,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            }
        };

        await context.Set<SopReference>().AddRangeAsync(sops);
        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} RAMS SOPs", sops.Count);
    }

    private static async Task SeedHazardControlLinksAsync(DbContext context, ILogger logger)
    {
        // Get existing hazards and controls
        var hazards = await context.Set<HazardLibrary>()
            .IgnoreQueryFilters()
            .Where(h => h.TenantId == DefaultTenantId)
            .ToListAsync();

        var controls = await context.Set<ControlMeasureLibrary>()
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == DefaultTenantId)
            .ToListAsync();

        if (!hazards.Any() || !controls.Any())
        {
            logger.LogInformation("No hazards or controls found, skipping links");
            return;
        }

        // Check if any links already exist (HazardControlLink doesn't have TenantId)
        if (await context.Set<HazardControlLink>().IgnoreQueryFilters().AnyAsync())
        {
            logger.LogInformation("RAMS hazard-control links already exist, skipping");
            return;
        }

        var links = new List<HazardControlLink>();
        var sortOrder = 1;

        // Link controls to hazards based on category
        foreach (var hazard in hazards)
        {
            // Find controls that apply to this hazard's category or are general (null category)
            var applicableControls = controls
                .Where(c => c.ApplicableToCategory == null || c.ApplicableToCategory == hazard.Category)
                .Take(5) // Limit to top 5 controls per hazard
                .ToList();

            foreach (var control in applicableControls)
            {
                links.Add(new HazardControlLink
                {
                    Id = Guid.NewGuid(),
                    HazardLibraryId = hazard.Id,
                    ControlMeasureLibraryId = control.Id,
                    SortOrder = sortOrder++,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                });
            }
        }

        if (links.Any())
        {
            await context.Set<HazardControlLink>().AddRangeAsync(links);
            await context.SaveChangesAsync();
            logger.LogInformation("Created {Count} RAMS hazard-control links", links.Count);
        }
    }
}
