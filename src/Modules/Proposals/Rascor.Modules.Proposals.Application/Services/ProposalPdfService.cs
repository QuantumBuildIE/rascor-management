using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Rascor.Modules.Proposals.Application.Common.Interfaces;
using Rascor.Modules.Proposals.Domain.Entities;

namespace Rascor.Modules.Proposals.Application.Services;

/// <summary>
/// Service for generating professional PDF documents from proposals using QuestPDF
/// </summary>
public class ProposalPdfService : IProposalPdfService
{
    private readonly IProposalsDbContext _context;

    // Company details - hardcoded for now, could come from tenant settings in future
    private static class CompanyDetails
    {
        public const string Name = "RASCOR Ireland";
        public const string AddressLine1 = "Unit 1, Rascor Business Park";
        public const string AddressLine2 = "Dublin, Ireland";
        public const string Phone = "+353 1 XXX XXXX";
        public const string Email = "info@rascor.ie";
        public const string Website = "www.rascor.ie";
        public const string VatNumber = "IE1234567X";
        public const string RegNumber = "123456";
    }

    // Theme colors
    private static readonly string PrimaryColor = "#1e3a5f";     // Dark blue
    private static readonly string SecondaryColor = "#f8fafc";   // Light gray
    private static readonly string WarningColor = "#fef3c7";     // Light yellow for internal sections

    public ProposalPdfService(IProposalsDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GeneratePdfAsync(Guid proposalId, bool includeCosting = false)
    {
        var proposal = await GetProposalWithDetailsAsync(proposalId);
        if (proposal == null)
        {
            throw new InvalidOperationException($"Proposal with ID {proposalId} not found");
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginTop(40);
                page.MarginBottom(40);
                page.MarginLeft(40);
                page.MarginRight(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                page.Header().Element(c => ComposeHeader(c, proposal));
                page.Content().Element(c => ComposeContent(c, proposal, includeCosting));
                page.Footer().Element(c => ComposeFooter(c, proposal));
            });
        });

        return document.GeneratePdf();
    }

    private async Task<Proposal?> GetProposalWithDetailsAsync(Guid proposalId)
    {
        return await _context.Proposals
            .Include(p => p.Sections.OrderBy(s => s.SortOrder))
                .ThenInclude(s => s.LineItems.OrderBy(i => i.SortOrder))
            .Include(p => p.Contacts)
            .FirstOrDefaultAsync(p => p.Id == proposalId);
    }

    private void ComposeHeader(IContainer container, Proposal proposal)
    {
        container.Column(column =>
        {
            // Company header
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

                // Proposal title on the right
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text("PROPOSAL")
                        .FontSize(24)
                        .Bold()
                        .FontColor(Color.FromHex(PrimaryColor));
                });
            });

            // Proposal details bar
            column.Item().PaddingTop(15).BorderBottom(1).BorderColor(Color.FromHex(PrimaryColor))
                .PaddingBottom(8).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(text =>
                    {
                        text.Span("Proposal No: ").FontColor(Colors.Grey.Medium);
                        text.Span($"{proposal.ProposalNumber}").Bold();
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Version: ").FontColor(Colors.Grey.Medium);
                        text.Span($"{proposal.Version}").Bold();
                    });
                });
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text(text =>
                    {
                        text.Span("Date: ").FontColor(Colors.Grey.Medium);
                        text.Span(proposal.ProposalDate.ToString("dd MMM yyyy")).Bold();
                    });
                    if (proposal.ValidUntilDate.HasValue)
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Valid Until: ").FontColor(Colors.Grey.Medium);
                            text.Span(proposal.ValidUntilDate.Value.ToString("dd MMM yyyy")).Bold();
                        });
                    }
                });
            });

            column.Item().PaddingTop(15);
        });
    }

    private void ComposeContent(IContainer container, Proposal proposal, bool includeCosting)
    {
        container.Column(column =>
        {
            // Client and Project section
            ComposeClientProjectSection(column, proposal);

            // Sections and Line Items
            ComposeSectionsAndItems(column, proposal, includeCosting);

            // Totals section
            ComposeTotalsSection(column, proposal, includeCosting);

            // Terms section
            if (!string.IsNullOrWhiteSpace(proposal.PaymentTerms) || !string.IsNullOrWhiteSpace(proposal.TermsAndConditions))
            {
                ComposeTermsSection(column, proposal);
            }

            // Signature section
            ComposeSignatureSection(column, proposal);
        });
    }

    private void ComposeClientProjectSection(ColumnDescriptor column, Proposal proposal)
    {
        column.Item().Row(row =>
        {
            // Client details
            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                .Padding(10).Column(col =>
            {
                col.Item().Text("TO:")
                    .FontSize(9)
                    .Bold()
                    .FontColor(Color.FromHex(PrimaryColor));
                col.Item().PaddingTop(5).Text(proposal.CompanyName)
                    .FontSize(11)
                    .Bold();
                if (!string.IsNullOrWhiteSpace(proposal.PrimaryContactName))
                {
                    col.Item().Text($"Attn: {proposal.PrimaryContactName}")
                        .FontSize(10);
                }

                // Get primary contact details
                var primaryContact = proposal.Contacts.FirstOrDefault(c => c.IsPrimary);
                if (primaryContact != null)
                {
                    if (!string.IsNullOrWhiteSpace(primaryContact.Email))
                    {
                        col.Item().Text(primaryContact.Email).FontSize(9);
                    }
                    if (!string.IsNullOrWhiteSpace(primaryContact.Phone))
                    {
                        col.Item().Text(primaryContact.Phone).FontSize(9);
                    }
                }
            });

            row.ConstantItem(15);

            // Project details
            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                .Padding(10).Column(col =>
            {
                col.Item().Text("PROJECT:")
                    .FontSize(9)
                    .Bold()
                    .FontColor(Color.FromHex(PrimaryColor));
                col.Item().PaddingTop(5).Text(proposal.ProjectName)
                    .FontSize(11)
                    .Bold();
                if (!string.IsNullOrWhiteSpace(proposal.ProjectAddress))
                {
                    col.Item().Text(proposal.ProjectAddress).FontSize(10);
                }
                if (!string.IsNullOrWhiteSpace(proposal.ProjectDescription))
                {
                    col.Item().PaddingTop(3).Text(proposal.ProjectDescription)
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);
                }
            });
        });

        column.Item().PaddingTop(20);
    }

    private void ComposeSectionsAndItems(ColumnDescriptor column, Proposal proposal, bool includeCosting)
    {
        var sections = proposal.Sections.OrderBy(s => s.SortOrder).ToList();

        foreach (var section in sections)
        {
            // Section header
            column.Item().Background(Color.FromHex(SecondaryColor))
                .Border(1).BorderColor(Colors.Grey.Lighten2)
                .Padding(8).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(section.SectionName.ToUpper())
                        .FontSize(11)
                        .Bold()
                        .FontColor(Color.FromHex(PrimaryColor));
                    if (!string.IsNullOrWhiteSpace(section.Description))
                    {
                        col.Item().Text(section.Description)
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    }
                });
            });

            // Line items table
            var lineItems = section.LineItems.OrderBy(i => i.SortOrder).ToList();
            if (lineItems.Any())
            {
                column.Item().Table(table =>
                {
                    // Define columns based on whether costing is included
                    if (includeCosting)
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(60);   // Item code
                            columns.RelativeColumn(3);    // Description
                            columns.ConstantColumn(40);   // Qty
                            columns.ConstantColumn(40);   // Unit
                            columns.ConstantColumn(65);   // Unit Cost
                            columns.ConstantColumn(65);   // Unit Price
                            columns.ConstantColumn(65);   // Line Cost
                            columns.ConstantColumn(50);   // Margin %
                            columns.ConstantColumn(70);   // Total
                        });
                    }
                    else
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(70);   // Item code
                            columns.RelativeColumn(4);    // Description
                            columns.ConstantColumn(50);   // Qty
                            columns.ConstantColumn(50);   // Unit
                            columns.ConstantColumn(80);   // Unit Price
                            columns.ConstantColumn(90);   // Total
                        });
                    }

                    // Table header
                    table.Header(header =>
                    {
                        header.Cell().Background(Color.FromHex(PrimaryColor))
                            .Padding(5).Text("Item").FontSize(8).FontColor(Colors.White).Bold();
                        header.Cell().Background(Color.FromHex(PrimaryColor))
                            .Padding(5).Text("Description").FontSize(8).FontColor(Colors.White).Bold();
                        header.Cell().Background(Color.FromHex(PrimaryColor))
                            .Padding(5).AlignRight().Text("Qty").FontSize(8).FontColor(Colors.White).Bold();
                        header.Cell().Background(Color.FromHex(PrimaryColor))
                            .Padding(5).Text("Unit").FontSize(8).FontColor(Colors.White).Bold();

                        if (includeCosting)
                        {
                            header.Cell().Background(Color.FromHex(PrimaryColor))
                                .Padding(5).AlignRight().Text("Unit Cost").FontSize(8).FontColor(Colors.White).Bold();
                        }

                        header.Cell().Background(Color.FromHex(PrimaryColor))
                            .Padding(5).AlignRight().Text("Unit Price").FontSize(8).FontColor(Colors.White).Bold();

                        if (includeCosting)
                        {
                            header.Cell().Background(Color.FromHex(PrimaryColor))
                                .Padding(5).AlignRight().Text("Line Cost").FontSize(8).FontColor(Colors.White).Bold();
                            header.Cell().Background(Color.FromHex(PrimaryColor))
                                .Padding(5).AlignRight().Text("Margin").FontSize(8).FontColor(Colors.White).Bold();
                        }

                        header.Cell().Background(Color.FromHex(PrimaryColor))
                            .Padding(5).AlignRight().Text("Total").FontSize(8).FontColor(Colors.White).Bold();
                    });

                    // Table rows
                    var isAlternate = false;
                    foreach (var item in lineItems)
                    {
                        var bgColor = isAlternate ? Colors.Grey.Lighten4 : Colors.White;

                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text(item.ProductCode ?? "-").FontSize(8);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text(item.Description).FontSize(8);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).AlignRight().Text(item.Quantity.ToString("N2")).FontSize(8);
                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).Text(item.Unit).FontSize(8);

                        if (includeCosting)
                        {
                            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(4).AlignRight().Text(FormatCurrency(item.UnitCost, proposal.Currency)).FontSize(8);
                        }

                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).AlignRight().Text(FormatCurrency(item.UnitPrice, proposal.Currency)).FontSize(8);

                        if (includeCosting)
                        {
                            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(4).AlignRight().Text(FormatCurrency(item.LineCost, proposal.Currency)).FontSize(8);

                            var marginColor = item.LineMargin >= 0 ? Colors.Green.Darken1 : Colors.Red.Medium;
                            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(4).AlignRight().Text($"{item.MarginPercent:N1}%").FontSize(8).FontColor(marginColor);
                        }

                        table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4).AlignRight().Text(FormatCurrency(item.LineTotal, proposal.Currency)).FontSize(8).Bold();

                        isAlternate = !isAlternate;
                    }
                });

                // Section subtotal
                column.Item().AlignRight().Padding(5).Text(text =>
                {
                    text.Span("Section Subtotal: ").FontSize(9).FontColor(Colors.Grey.Darken1);
                    text.Span(FormatCurrency(section.SectionTotal, proposal.Currency)).FontSize(10).Bold();
                });
            }

            column.Item().PaddingTop(15);
        }
    }

    private void ComposeTotalsSection(ColumnDescriptor column, Proposal proposal, bool includeCosting)
    {
        column.Item().AlignRight().Width(250).Border(1).BorderColor(Colors.Grey.Lighten2)
            .Background(Color.FromHex(SecondaryColor)).Padding(10).Column(col =>
        {
            // Subtotal
            col.Item().Row(row =>
            {
                row.RelativeItem().Text("Subtotal:").FontSize(10);
                row.ConstantItem(100).AlignRight().Text(FormatCurrency(proposal.Subtotal, proposal.Currency)).FontSize(10);
            });

            // Discount
            if (proposal.DiscountPercent > 0 || proposal.DiscountAmount > 0)
            {
                col.Item().PaddingTop(3).Row(row =>
                {
                    row.RelativeItem().Text($"Discount ({proposal.DiscountPercent:N1}%):").FontSize(10);
                    row.ConstantItem(100).AlignRight().Text($"-{FormatCurrency(proposal.DiscountAmount, proposal.Currency)}")
                        .FontSize(10).FontColor(Colors.Red.Medium);
                });
            }

            // Net Total
            col.Item().PaddingTop(3).Row(row =>
            {
                row.RelativeItem().Text("Net Total:").FontSize(10);
                row.ConstantItem(100).AlignRight().Text(FormatCurrency(proposal.NetTotal, proposal.Currency)).FontSize(10);
            });

            // VAT
            col.Item().PaddingTop(3).Row(row =>
            {
                row.RelativeItem().Text($"VAT ({proposal.VatRate:N0}%):").FontSize(10);
                row.ConstantItem(100).AlignRight().Text(FormatCurrency(proposal.VatAmount, proposal.Currency)).FontSize(10);
            });

            // Grand Total
            col.Item().PaddingTop(8).BorderTop(1).BorderColor(Color.FromHex(PrimaryColor))
                .PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text("GRAND TOTAL:").FontSize(12).Bold().FontColor(Color.FromHex(PrimaryColor));
                row.ConstantItem(100).AlignRight().Text(FormatCurrency(proposal.GrandTotal, proposal.Currency))
                    .FontSize(12).Bold().FontColor(Color.FromHex(PrimaryColor));
            });
        });

        // Internal costing summary (only shown if includeCosting is true)
        if (includeCosting)
        {
            column.Item().PaddingTop(15).AlignRight().Width(300)
                .Background(Color.FromHex(WarningColor))
                .Border(2).BorderColor(Colors.Orange.Medium)
                .Padding(10).Column(col =>
            {
                col.Item().Text("INTERNAL - DO NOT SHARE WITH CLIENT")
                    .FontSize(9).Bold().FontColor(Colors.Red.Medium);
                col.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Text("Total Cost:").FontSize(10);
                    row.ConstantItem(100).AlignRight().Text(FormatCurrency(proposal.TotalCost, proposal.Currency)).FontSize(10);
                });
                col.Item().PaddingTop(3).Row(row =>
                {
                    var marginColor = proposal.TotalMargin >= 0 ? Colors.Green.Darken1 : Colors.Red.Medium;
                    row.RelativeItem().Text("Total Margin:").FontSize(10);
                    row.ConstantItem(100).AlignRight().Text(text =>
                    {
                        text.Span(FormatCurrency(proposal.TotalMargin, proposal.Currency)).FontColor(marginColor);
                        text.Span($" ({proposal.MarginPercent:N1}%)").FontSize(9).FontColor(marginColor);
                    });
                });
            });
        }

        column.Item().PaddingTop(20);
    }

    private void ComposeTermsSection(ColumnDescriptor column, Proposal proposal)
    {
        if (!string.IsNullOrWhiteSpace(proposal.PaymentTerms))
        {
            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
            {
                col.Item().Text("PAYMENT TERMS")
                    .FontSize(10).Bold().FontColor(Color.FromHex(PrimaryColor));
                col.Item().PaddingTop(5).Text(proposal.PaymentTerms).FontSize(9);
            });
            column.Item().PaddingTop(10);
        }

        if (!string.IsNullOrWhiteSpace(proposal.TermsAndConditions))
        {
            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
            {
                col.Item().Text("TERMS & CONDITIONS")
                    .FontSize(10).Bold().FontColor(Color.FromHex(PrimaryColor));
                col.Item().PaddingTop(5).Text(proposal.TermsAndConditions).FontSize(9);
            });
            column.Item().PaddingTop(10);
        }

        column.Item().PaddingTop(10);
    }

    private void ComposeSignatureSection(ColumnDescriptor column, Proposal proposal)
    {
        column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(col =>
        {
            col.Item().Text("ACCEPTANCE")
                .FontSize(11).Bold().FontColor(Color.FromHex(PrimaryColor));
            col.Item().PaddingTop(8).Text("I accept this proposal and agree to the terms and conditions stated above.")
                .FontSize(9);

            col.Item().PaddingTop(25).Row(row =>
            {
                row.RelativeItem().Column(signCol =>
                {
                    signCol.Item().BorderBottom(1).BorderColor(Colors.Grey.Medium).Height(1);
                    signCol.Item().PaddingTop(3).Text("Signature").FontSize(8).FontColor(Colors.Grey.Medium);
                });
                row.ConstantItem(30);
                row.ConstantItem(150).Column(dateCol =>
                {
                    dateCol.Item().BorderBottom(1).BorderColor(Colors.Grey.Medium).Height(1);
                    dateCol.Item().PaddingTop(3).Text("Date").FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });

            col.Item().PaddingTop(20).Row(row =>
            {
                row.RelativeItem().Column(nameCol =>
                {
                    nameCol.Item().BorderBottom(1).BorderColor(Colors.Grey.Medium).Height(1);
                    nameCol.Item().PaddingTop(3).Text("Print Name").FontSize(8).FontColor(Colors.Grey.Medium);
                });
                row.ConstantItem(30);
                row.ConstantItem(150).Column(posCol =>
                {
                    posCol.Item().BorderBottom(1).BorderColor(Colors.Grey.Medium).Height(1);
                    posCol.Item().PaddingTop(3).Text("Position").FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });
    }

    private void ComposeFooter(IContainer container, Proposal proposal)
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
                    text.Span($"{CompanyDetails.Name} | VAT: {CompanyDetails.VatNumber} | Reg: {CompanyDetails.RegNumber}");
                });

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
                    text.Span($"Generated: {DateTime.Now:dd MMM yyyy HH:mm}");
                });
            });
        });
    }

    private static string FormatCurrency(decimal amount, string currency)
    {
        var symbol = currency?.ToUpper() switch
        {
            "EUR" => "\u20ac",
            "GBP" => "\u00a3",
            "USD" => "$",
            _ => "\u20ac"
        };
        return $"{symbol}{amount:N2}";
    }
}
