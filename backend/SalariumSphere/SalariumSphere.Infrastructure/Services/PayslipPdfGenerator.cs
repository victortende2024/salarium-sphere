using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SalariumSphere.Application.Common;

namespace SalariumSphere.Infrastructure.Services;

public static class PayslipPdfGenerator
{
    public static byte[] GeneratePayslipPdf(PayslipDto payslip)
    {
        // Ensure QuestPDF is licensed (Community license is free)
        QuestPDF.Settings.License = LicenseType.Community;

        using var stream = new MemoryStream();
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Header
                page.Header()
                    .Column(col =>
                    {
                        col.Item().Text("PAYSLIP")
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Blue.Darken3)
                            .AlignCenter();

                        col.Item().PaddingTop(5)
                            .Text($"Generated: {payslip.GeneratedAt:dd MMM yyyy HH:mm}")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1)
                            .AlignCenter();

                        col.Item().PaddingTop(10)
                            .LineHorizontal(0.5f);
                    });

                // Content
                page.Content().Column(col =>
                {
                    // Employee Info
                    col.Item().PaddingTop(10)
                        .Row(row =>
                        {
                            row.RelativeItem(1).Text($"Employee: {payslip.EmployeeName} ({payslip.EmployeeCode})");
                            row.RelativeItem(1).Text($"Period: {payslip.PayPeriod}");
                            row.RelativeItem(1).Text($"Currency: {payslip.Currency}");
                        });

                    col.Item().PaddingTop(10)
                        .Row(row =>
                        {
                            row.RelativeItem(1).Text($"Company: {payslip.CompanyName}");
                            row.RelativeItem(1).Text($"Department: {payslip.DepartmentName}");
                            row.RelativeItem(1).Text($"Profile: {payslip.ProfileName}");
                        });

                    col.Item().PaddingTop(15)
                        .LineHorizontal(0.5f);

                    // Earnings Table
                    col.Item().PaddingTop(10)
                        .Text("Earnings & Benefits")
                        .FontSize(14)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    col.Item().PaddingTop(5)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Description").Bold();
                                header.Cell().Text("Type").Bold().AlignRight();
                                header.Cell().Text("Amount").Bold().AlignRight();
                            });

                            foreach (var item in payslip.LineItems.Where(i => i.Type == "Earning"))
                            {
                                table.Cell().Text(item.Name);
                                table.Cell().Text(item.Type).AlignRight();
                                table.Cell().Text($"${item.Amount:F2}").AlignRight();
                            }

                            // Total Gross
                            table.Cell().Text("Gross Pay").Bold();
                            table.Cell().Text("");
                            table.Cell().Text($"${payslip.GrossPay:F2}").Bold().AlignRight();
                        });

                    col.Item().PaddingTop(15)
                        .LineHorizontal(0.5f);

                    // Taxes & Deductions Table
                    col.Item().PaddingTop(10)
                        .Text("Taxes & Deductions")
                        .FontSize(14)
                        .Bold()
                        .FontColor(Colors.Red.Darken2);

                    col.Item().PaddingTop(5)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Description").Bold();
                                header.Cell().Text("Type").Bold().AlignRight();
                                header.Cell().Text("Amount").Bold().AlignRight();
                            });

                            foreach (var item in payslip.LineItems.Where(i => i.Type == "Tax" || i.Type == "Deduction"))
                            {
                                table.Cell().Text(item.Name);
                                table.Cell().Text(item.Type).AlignRight();
                                table.Cell().Text($"-${item.Amount:F2}").AlignRight().FontColor(Colors.Red.Darken2);
                            }

                            // Total Tax
                            table.Cell().Text("Total Tax").Bold();
                            table.Cell().Text("");
                            table.Cell().Text($"-${payslip.TotalTax:F2}").Bold().AlignRight().FontColor(Colors.Red.Darken2);

                            // Total Deductions
                            table.Cell().Text("Total Deductions").Bold();
                            table.Cell().Text("");
                            table.Cell().Text($"-${payslip.TotalDeductions:F2}").Bold().AlignRight().FontColor(Colors.Orange.Darken2);
                        });

                    col.Item().PaddingTop(15)
                        .LineHorizontal(0.5f);

                    // Net Pay Summary
                    col.Item().PaddingTop(10)
                        .Row(row =>
                        {
                            row.RelativeItem(1)
                                .Text("Net Pay")
                                .FontSize(16)
                                .Bold()
                                .FontColor(Colors.Green.Darken3);

                            row.RelativeItem(1)
                                .Text($"${payslip.NetPay:F2}")
                                .FontSize(16)
                                .Bold()
                                .FontColor(Colors.Green.Darken3)
                                .AlignRight();
                        });
                });

                // Footer (FIXED: removed lambda, used simple string)
                page.Footer()
                    .AlignCenter()
                    .Text($"This is a computer-generated document. No signature required. | Generated on {DateTime.Now:dd MMM yyyy HH:mm}")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken1);
            });
        });

        document.GeneratePdf(stream);
        return stream.ToArray();
    }
}