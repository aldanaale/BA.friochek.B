namespace BA.Backend.Application.Common.Interfaces;

public interface IQrGeneratorService
{
    string GenerateQrBase64(string payload);
}
