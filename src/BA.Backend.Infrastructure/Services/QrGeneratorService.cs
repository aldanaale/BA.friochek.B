using BA.Backend.Application.Common.Interfaces;
using QRCoder;
using System;

namespace BA.Backend.Infrastructure.Services;

public class QrGeneratorService : IQrGeneratorService
{
    public string GenerateQrBase64(string payload)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        
        var qrCodeImage = qrCode.GetGraphic(20);
        var base64 = Convert.ToBase64String(qrCodeImage);
        
        return $"data:image/png;base64,{base64}";
    }
}
