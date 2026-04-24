using BA.Backend.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;

namespace BA.Backend.Infrastructure.Services;

public class PdfReportService : IPdfReportService
{
    public PdfReportService()
    {
        // Required by QuestPDF for free community usage
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateDeliveryCertificatePdf(
        Guid certificateId,
        string tenantName,
        string transporterName,
        string storeName,
        DateTime acceptanceTimestamp,
        double latitude,
        double longitude,
        string ipAddress,
        string signatureBase64,
        string serverHash)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                page.Header().Element(compose => ComposeHeader(compose, tenantName));
                page.Content().Element(compose => ComposeContent(compose, certificateId, transporterName, storeName, acceptanceTimestamp, latitude, longitude, ipAddress, signatureBase64, serverHash));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, string tenantName)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text($"Certificado de Operación").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text($"Empresa: {tenantName}").FontSize(14).FontColor(Colors.Grey.Darken2);
            });
        });
    }

    private void ComposeContent(IContainer container, Guid certificateId, string transporterName, string storeName, DateTime acceptanceTimestamp, double latitude, double longitude, string ipAddress, string signatureBase64, string serverHash)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(5);

            column.Item().Text("Detalles de la Entrega").FontSize(14).SemiBold().Underline();
            
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(120);
                    columns.RelativeColumn();
                });

                table.Cell().Text("ID Certificado:");
                table.Cell().Text(certificateId.ToString());

                table.Cell().Text("Transportista:");
                table.Cell().Text(transporterName);

                table.Cell().Text("Tienda Destino:");
                table.Cell().Text(storeName);

                table.Cell().Text("Fecha y Hora:");
                table.Cell().Text(acceptanceTimestamp.ToString("yyyy-MM-dd HH:mm:ss UTC"));

                table.Cell().Text("Dirección IP:");
                table.Cell().Text(ipAddress);

                table.Cell().Text("Coordenadas:");
                table.Cell().Text($"{latitude}, {longitude}");
            });

            column.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().Text("Firma de Conformidad").FontSize(14).SemiBold();
            
            if (!string.IsNullOrEmpty(signatureBase64))
            {
                try
                {
                    var base64Data = signatureBase64.Contains(",") 
                        ? signatureBase64.Split(',')[1] 
                        : signatureBase64;
                        
                    var imageBytes = Convert.FromBase64String(base64Data);
                    column.Item().PaddingTop(10).Width(250).Image(imageBytes);
                }
                catch
                {
                    column.Item().Text("[Error al cargar la imagen de la firma]").FontColor(Colors.Red.Medium);
                }
            }

            column.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().Text("Sello Criptográfico de Integridad (SHA-256)").FontSize(12).SemiBold();
            column.Item().Text(serverHash).FontSize(9).FontColor(Colors.Grey.Darken3).FontFamily(Fonts.CourierNew);
            column.Item().Text("Este documento está protegido contra alteraciones.").FontSize(10).Italic();
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Página ");
            x.CurrentPageNumber();
            x.Span(" de ");
            x.TotalPages();
            x.Span(" - Generado por BA.FrioCheck").FontSize(9).FontColor(Colors.Grey.Medium);
        });
    }
}
