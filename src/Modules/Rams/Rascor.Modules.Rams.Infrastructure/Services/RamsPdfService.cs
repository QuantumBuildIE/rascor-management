using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Rascor.Core.Application.Interfaces;
using Rascor.Modules.Rams.Application.Common.Interfaces;
using Rascor.Modules.Rams.Application.DTOs;
using Rascor.Modules.Rams.Application.Services;
using Rascor.Modules.Rams.Domain.Entities;

namespace Rascor.Modules.Rams.Infrastructure.Services;

/// <summary>
/// Service for generating professional PDF documents from RAMS documents using QuestPDF
/// </summary>
public class RamsPdfService : IRamsPdfService
{
    private readonly IRamsDbContext _context;
    private readonly ICoreDbContext _coreContext;

    // Company details - hardcoded for now, could come from tenant settings in future
    private static class CompanyDetails
    {
        public const string Name = "RASCOR Ireland";
        public const string AddressLine1 = "Unit 1, Rascor Business Park";
        public const string AddressLine2 = "Dublin, Ireland";
        public const string Phone = "+353 1 XXX XXXX";
        public const string Email = "info@rascor.ie";
        public const string Website = "www.rascor.ie";
    }

    // Theme colors
    private static readonly string PrimaryColor = "#1e3a5f";     // Dark blue
    private static readonly string HighRiskColor = "#dc2626";    // Red
    private static readonly string MediumRiskColor = "#f97316";  // Orange
    private static readonly string LowRiskColor = "#16a34a";     // Green

    public RamsPdfService(IRamsDbContext context, ICoreDbContext coreContext)
    {
        _context = context;
        _coreContext = coreContext;

        // Set license type (Community is free for companies with <1M USD revenue)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GeneratePdfAsync(Guid ramsDocumentId, CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentWithDetailsAsync(ramsDocumentId, cancellationToken);

        if (document == null)
            throw new InvalidOperationException($"RAMS document {ramsDocumentId} not found");

        var pdfData = await MapToPdfDataAsync(document, cancellationToken);

        var pdfDocument = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginTop(40);
                page.MarginBottom(40);
                page.MarginLeft(40);
                page.MarginRight(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                page.Header().Element(c => ComposeHeader(c, pdfData));
                page.Content().Element(c => ComposeContent(c, pdfData));
                page.Footer().Element(c => ComposeFooter(c, pdfData));
            });
        });

        return pdfDocument.GeneratePdf();
    }

    private async Task<RamsDocument?> GetDocumentWithDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.RamsDocuments
            .Include(d => d.RiskAssessments.OrderBy(r => r.SortOrder))
            .Include(d => d.MethodSteps.OrderBy(m => m.StepNumber))
                .ThenInclude(m => m.LinkedRiskAssessment)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    private async Task<RamsPdfDataDto> MapToPdfDataAsync(RamsDocument document, CancellationToken cancellationToken)
    {
        // Get safety officer name if applicable
        string? safetyOfficerName = null;
        if (document.SafetyOfficerId.HasValue)
        {
            var employee = await _coreContext.Employees
                .Where(e => e.Id == document.SafetyOfficerId.Value)
                .Select(e => new { e.FirstName, e.LastName })
                .FirstOrDefaultAsync(cancellationToken);

            if (employee != null)
                safetyOfficerName = $"{employee.FirstName} {employee.LastName}";
        }

        // Note: ApprovedByName is left as null since ICoreDbContext doesn't expose Users
        // The approval information (date) is still shown on the PDF
        string? approvedByName = null;

        return new RamsPdfDataDto
        {
            ProjectName = document.ProjectName,
            ProjectReference = document.ProjectReference,
            ProjectTypeDisplay = FormatProjectType(document.ProjectType.ToString()),
            ClientName = document.ClientName,
            SiteAddress = document.SiteAddress,
            AreaOfActivity = document.AreaOfActivity,
            ProposedStartDate = document.ProposedStartDate?.ToString("dd MMM yyyy"),
            ProposedEndDate = document.ProposedEndDate?.ToString("dd MMM yyyy"),
            SafetyOfficerName = safetyOfficerName,
            StatusDisplay = document.Status.ToString(),
            DateApproved = document.DateApproved?.ToString("dd MMM yyyy"),
            ApprovedByName = approvedByName,
            MethodStatementBody = document.MethodStatementBody,
            GeneratedAt = DateTime.UtcNow.ToString("dd MMM yyyy HH:mm"),
            CompanyName = CompanyDetails.Name,

            RiskAssessments = document.RiskAssessments
                .Select((r, index) => new RamsPdfRiskAssessmentDto
                {
                    Number = index + 1,
                    TaskActivity = r.TaskActivity,
                    LocationArea = r.LocationArea,
                    HazardIdentified = r.HazardIdentified,
                    WhoAtRisk = r.WhoAtRisk,
                    InitialLikelihood = r.InitialLikelihood,
                    InitialSeverity = r.InitialSeverity,
                    InitialRiskRating = r.InitialRiskRating,
                    InitialRiskLevel = r.InitialRiskLevel.ToString(),
                    ControlMeasures = r.ControlMeasures,
                    RelevantLegislation = r.RelevantLegislation,
                    ResidualLikelihood = r.ResidualLikelihood,
                    ResidualSeverity = r.ResidualSeverity,
                    ResidualRiskRating = r.ResidualRiskRating,
                    ResidualRiskLevel = r.ResidualRiskLevel.ToString()
                }).ToList(),

            MethodSteps = document.MethodSteps
                .Select(s => new RamsPdfMethodStepDto
                {
                    StepNumber = s.StepNumber,
                    StepTitle = s.StepTitle,
                    DetailedProcedure = s.DetailedProcedure,
                    LinkedRiskAssessment = s.LinkedRiskAssessment?.TaskActivity,
                    RequiredPermits = s.RequiredPermits,
                    RequiresSignoff = s.RequiresSignoff
                }).ToList()
        };
    }

    private static string FormatProjectType(string projectType)
    {
        // Convert PascalCase to readable format
        return projectType switch
        {
            "RemedialInjection" => "Remedial Injection",
            "RascotankNewBuild" => "Rascotank New Build",
            "CarParkCoating" => "Car Park Coating",
            "GroundGasBarrier" => "Ground Gas Barrier",
            _ => projectType
        };
    }

    private void ComposeHeader(IContainer container, RamsPdfDataDto data)
    {
        container.Column(column =>
        {
            // Company header row
            column.Item().Row(row =>
            {
                // Company info on the left
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(CompanyDetails.Name)
                        .FontSize(18)
                        .Bold()
                        .FontColor(Color.FromHex(PrimaryColor));
                    col.Item().Text(CompanyDetails.AddressLine1)
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                    col.Item().Text(CompanyDetails.AddressLine2)
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(2).Text(text =>
                    {
                        text.Span("Phone: ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.Span(CompanyDetails.Phone).FontSize(8);
                        text.Span(" | Email: ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.Span(CompanyDetails.Email).FontSize(8);
                    });
                });

                // Document title on the right
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text("RISK ASSESSMENT")
                        .FontSize(16)
                        .Bold()
                        .FontColor(Color.FromHex(PrimaryColor));
                    col.Item().Text("AND METHOD STATEMENT")
                        .FontSize(14)
                        .Bold()
                        .FontColor(Color.FromHex(PrimaryColor));
                    col.Item().Text("(RAMS)")
                        .FontSize(12)
                        .FontColor(Color.FromHex(PrimaryColor));
                });
            });

            // Reference bar
            column.Item().PaddingTop(15).BorderBottom(2).BorderColor(Color.FromHex(PrimaryColor))
                .PaddingBottom(8).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Reference: ").FontColor(Colors.Grey.Medium);
                            text.Span(data.ProjectReference).Bold();
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Status: ").FontColor(Colors.Grey.Medium);
                            text.Span(data.StatusDisplay).Bold();
                        });
                    });
                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Project Type: ").FontColor(Colors.Grey.Medium);
                            text.Span(data.ProjectTypeDisplay).Bold();
                        });
                        if (!string.IsNullOrEmpty(data.DateApproved))
                        {
                            col.Item().Text(text =>
                            {
                                text.Span("Approved: ").FontColor(Colors.Grey.Medium);
                                text.Span(data.DateApproved).Bold();
                            });
                        }
                    });
                });

            column.Item().PaddingTop(15);
        });
    }

    private void ComposeContent(IContainer container, RamsPdfDataDto data)
    {
        container.Column(column =>
        {
            // Project Details Section
            ComposeProjectDetailsSection(column, data);

            column.Item().PaddingTop(15);

            // Risk Assessment Section
            ComposeRiskAssessmentSection(column, data);

            column.Item().PaddingTop(15);

            // Method Statement Section
            ComposeMethodStatementSection(column, data);

            column.Item().PaddingTop(15);

            // Sign-off Section
            ComposeSignoffSection(column, data);
        });
    }

    private void ComposeProjectDetailsSection(ColumnDescriptor column, RamsPdfDataDto data)
    {
        column.Item().Text("PROJECT DETAILS")
            .FontSize(12)
            .Bold()
            .FontColor(Color.FromHex(PrimaryColor));

        column.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
                columns.RelativeColumn(2);
            });

            // Row 1: Project Name and Reference
            AddTableCell(table, "Project Name", true);
            AddTableCell(table, data.ProjectName, false);
            AddTableCell(table, "Reference", true);
            AddTableCell(table, data.ProjectReference, false);

            // Row 2: Client and Project Type
            AddTableCell(table, "Client", true);
            AddTableCell(table, data.ClientName ?? "-", false);
            AddTableCell(table, "Project Type", true);
            AddTableCell(table, data.ProjectTypeDisplay, false);

            // Row 3: Site Address (spanning 3 columns)
            AddTableCell(table, "Site Address", true);
            table.Cell().ColumnSpan(3).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                .Text(data.SiteAddress ?? "-");

            // Row 4: Area of Activity (spanning 3 columns)
            AddTableCell(table, "Area of Activity", true);
            table.Cell().ColumnSpan(3).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                .Text(data.AreaOfActivity ?? "-");

            // Row 5: Start and End Dates
            AddTableCell(table, "Start Date", true);
            AddTableCell(table, data.ProposedStartDate ?? "-", false);
            AddTableCell(table, "End Date", true);
            AddTableCell(table, data.ProposedEndDate ?? "-", false);

            // Row 6: Safety Officer and Status
            AddTableCell(table, "Safety Officer", true);
            AddTableCell(table, data.SafetyOfficerName ?? "-", false);
            AddTableCell(table, "Status", true);
            AddTableCell(table, data.StatusDisplay, false);
        });
    }

    private static void AddTableCell(TableDescriptor table, string text, bool isHeader)
    {
        if (isHeader)
        {
            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                .Background(Colors.Grey.Lighten3)
                .Text(text).Bold();
        }
        else
        {
            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                .Text(text);
        }
    }

    private void ComposeRiskAssessmentSection(ColumnDescriptor column, RamsPdfDataDto data)
    {
        column.Item().Text("RISK ASSESSMENT")
            .FontSize(12)
            .Bold()
            .FontColor(Color.FromHex(PrimaryColor));

        // Risk Matrix Legend
        column.Item().PaddingTop(5).PaddingBottom(5).Row(row =>
        {
            row.AutoItem().PaddingRight(15).Row(legendRow =>
            {
                legendRow.AutoItem().Width(15).Height(15).Background(Color.FromHex(LowRiskColor));
                legendRow.AutoItem().PaddingLeft(3).AlignMiddle().Text("Low (1-4)").FontSize(8);
            });
            row.AutoItem().PaddingRight(15).Row(legendRow =>
            {
                legendRow.AutoItem().Width(15).Height(15).Background(Color.FromHex(MediumRiskColor));
                legendRow.AutoItem().PaddingLeft(3).AlignMiddle().Text("Medium (5-12)").FontSize(8);
            });
            row.AutoItem().Row(legendRow =>
            {
                legendRow.AutoItem().Width(15).Height(15).Background(Color.FromHex(HighRiskColor));
                legendRow.AutoItem().PaddingLeft(3).AlignMiddle().Text("High (13-25)").FontSize(8);
            });
        });

        if (data.RiskAssessments.Count == 0)
        {
            column.Item().Padding(10).Text("No risk assessments defined.").Italic();
            return;
        }

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25);   // #
                columns.RelativeColumn(2);    // Task/Hazard
                columns.ConstantColumn(55);   // Initial Risk
                columns.RelativeColumn(3);    // Control Measures
                columns.ConstantColumn(55);   // Residual Risk
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Background(Color.FromHex(PrimaryColor)).Padding(5)
                    .Text("#").FontColor(Colors.White).Bold().AlignCenter();
                header.Cell().Background(Color.FromHex(PrimaryColor)).Padding(5)
                    .Text("Task / Hazard").FontColor(Colors.White).Bold();
                header.Cell().Background(Color.FromHex(PrimaryColor)).Padding(5)
                    .Text("Initial").FontColor(Colors.White).Bold().AlignCenter();
                header.Cell().Background(Color.FromHex(PrimaryColor)).Padding(5)
                    .Text("Control Measures").FontColor(Colors.White).Bold();
                header.Cell().Background(Color.FromHex(PrimaryColor)).Padding(5)
                    .Text("Residual").FontColor(Colors.White).Bold().AlignCenter();
            });

            // Data rows
            foreach (var risk in data.RiskAssessments)
            {
                // Number
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                    .Text(risk.Number.ToString()).AlignCenter();

                // Task/Hazard
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Column(col =>
                {
                    col.Item().Text(risk.TaskActivity).Bold().FontSize(9);
                    if (!string.IsNullOrEmpty(risk.LocationArea))
                        col.Item().Text($"Location: {risk.LocationArea}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(3).Text($"Hazard: {risk.HazardIdentified}").FontSize(9);
                    if (!string.IsNullOrEmpty(risk.WhoAtRisk))
                        col.Item().Text($"At Risk: {risk.WhoAtRisk}").FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                // Initial Risk
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Column(col =>
                {
                    col.Item().Element(c => ComposeRiskBadge(c, risk.InitialRiskRating, risk.InitialRiskLevel));
                    col.Item().Text($"L{risk.InitialLikelihood} x S{risk.InitialSeverity}").FontSize(7).AlignCenter();
                });

                // Control Measures
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Column(col =>
                {
                    col.Item().Text(risk.ControlMeasures ?? "-").FontSize(9);
                    if (!string.IsNullOrEmpty(risk.RelevantLegislation))
                    {
                        col.Item().PaddingTop(3).Text($"Legislation: {risk.RelevantLegislation}")
                            .FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
                    }
                });

                // Residual Risk
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Column(col =>
                {
                    col.Item().Element(c => ComposeRiskBadge(c, risk.ResidualRiskRating, risk.ResidualRiskLevel));
                    col.Item().Text($"L{risk.ResidualLikelihood} x S{risk.ResidualSeverity}").FontSize(7).AlignCenter();
                });
            }
        });
    }

    private void ComposeRiskBadge(IContainer container, int rating, string level)
    {
        var color = level switch
        {
            "Low" => Color.FromHex(LowRiskColor),
            "Medium" => Color.FromHex(MediumRiskColor),
            "High" => Color.FromHex(HighRiskColor),
            _ => Colors.Grey.Medium
        };

        container.AlignCenter().Width(35).Height(22).Background(color)
            .AlignCenter().AlignMiddle()
            .Text(rating.ToString()).FontSize(11).Bold().FontColor(Colors.White);
    }

    private void ComposeMethodStatementSection(ColumnDescriptor column, RamsPdfDataDto data)
    {
        column.Item().Text("METHOD STATEMENT")
            .FontSize(12)
            .Bold()
            .FontColor(Color.FromHex(PrimaryColor));

        // General method statement body if present
        if (!string.IsNullOrEmpty(data.MethodStatementBody))
        {
            column.Item().PaddingTop(5).PaddingBottom(10)
                .Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10)
                .Text(data.MethodStatementBody).FontSize(9);
        }

        if (data.MethodSteps.Count == 0)
        {
            column.Item().Padding(10).Text("No method steps defined.").Italic();
            return;
        }

        column.Item().PaddingTop(5).Text("Work Procedure Steps:").Bold();

        column.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(35);   // Step #
                columns.RelativeColumn(1);    // Title
                columns.RelativeColumn(2);    // Procedure
                columns.RelativeColumn(1);    // Notes
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Background(Color.FromHex(PrimaryColor)).Padding(5)
                    .Text("Step").FontColor(Colors.White).Bold().AlignCenter();
                header.Cell().Background(Color.FromHex(PrimaryColor)).Padding(5)
                    .Text("Title").FontColor(Colors.White).Bold();
                header.Cell().Background(Color.FromHex(PrimaryColor)).Padding(5)
                    .Text("Procedure").FontColor(Colors.White).Bold();
                header.Cell().Background(Color.FromHex(PrimaryColor)).Padding(5)
                    .Text("Notes").FontColor(Colors.White).Bold();
            });

            // Data rows
            foreach (var step in data.MethodSteps)
            {
                // Step number
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                    .AlignCenter().AlignMiddle()
                    .Text(step.StepNumber.ToString()).Bold();

                // Title
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                    .Text(step.StepTitle).Bold().FontSize(9);

                // Procedure
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                    .Text(step.DetailedProcedure ?? "-").FontSize(9);

                // Notes
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Column(col =>
                {
                    if (!string.IsNullOrEmpty(step.LinkedRiskAssessment))
                        col.Item().Text($"Risk: {step.LinkedRiskAssessment}").FontSize(8);
                    if (!string.IsNullOrEmpty(step.RequiredPermits))
                        col.Item().Text($"Permits: {step.RequiredPermits}").FontSize(8).FontColor(Color.FromHex(MediumRiskColor));
                    if (step.RequiresSignoff)
                        col.Item().Text("Requires Sign-off").FontSize(8).Bold().FontColor(Color.FromHex(HighRiskColor));
                });
            }
        });
    }

    private void ComposeSignoffSection(ColumnDescriptor column, RamsPdfDataDto data)
    {
        column.Item().Text("SIGN-OFF")
            .FontSize(12)
            .Bold()
            .FontColor(Color.FromHex(PrimaryColor));

        column.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            // Prepared By
            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(col =>
            {
                col.Item().Text("Prepared By:").Bold();
                col.Item().PaddingTop(20).LineHorizontal(1);
                col.Item().Text("Name").FontSize(8);
                col.Item().PaddingTop(15).LineHorizontal(1);
                col.Item().Text("Signature").FontSize(8);
                col.Item().PaddingTop(15).LineHorizontal(1);
                col.Item().Text("Date").FontSize(8);
            });

            // Reviewed By
            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(col =>
            {
                col.Item().Text("Reviewed By:").Bold();
                col.Item().PaddingTop(20).LineHorizontal(1);
                col.Item().Text("Name").FontSize(8);
                col.Item().PaddingTop(15).LineHorizontal(1);
                col.Item().Text("Signature").FontSize(8);
                col.Item().PaddingTop(15).LineHorizontal(1);
                col.Item().Text("Date").FontSize(8);
            });

            // Approved By
            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(col =>
            {
                col.Item().Text("Approved By:").Bold();
                if (!string.IsNullOrEmpty(data.ApprovedByName))
                {
                    col.Item().PaddingTop(5).Text(data.ApprovedByName);
                    col.Item().PaddingTop(30).LineHorizontal(1);
                    col.Item().Text("Signature").FontSize(8);
                    col.Item().PaddingTop(5).Text(data.DateApproved ?? "");
                }
                else
                {
                    col.Item().PaddingTop(20).LineHorizontal(1);
                    col.Item().Text("Name").FontSize(8);
                    col.Item().PaddingTop(15).LineHorizontal(1);
                    col.Item().Text("Signature").FontSize(8);
                    col.Item().PaddingTop(15).LineHorizontal(1);
                    col.Item().Text("Date").FontSize(8);
                }
            });
        });

        // Worker acknowledgment
        column.Item().PaddingTop(15).Text("WORKER ACKNOWLEDGMENT")
            .FontSize(10).Bold();
        column.Item().PaddingTop(5).Text(
            "I confirm that I have read, understood, and will comply with this Risk Assessment and Method Statement.")
            .FontSize(9);

        column.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(5).Text("#").Bold().AlignCenter();
                header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(5).Text("Name (Print)").Bold();
                header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(5).Text("Signature").Bold();
                header.Cell().Border(1).Background(Colors.Grey.Lighten2).Padding(5).Text("Date").Bold();
            });

            for (int i = 1; i <= 6; i++)
            {
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Height(25)
                    .Text(i.ToString()).AlignCenter();
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Text("");
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Text("");
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Text("");
            }
        });
    }

    private void ComposeFooter(IContainer container, RamsPdfDataDto data)
    {
        container.Column(column =>
        {
            column.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });

                row.RelativeItem().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Grey.Medium));
                    text.Span($"{CompanyDetails.Name} | {CompanyDetails.Website}");
                });

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
                    text.Span($"Generated: {data.GeneratedAt}");
                });
            });
        });
    }
}
