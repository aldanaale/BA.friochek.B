
using Microsoft.AspNetCore.Http;

namespace BA.Backend.Infrastructure.Services;

/// <summary>
/// Implementación pendiente de integración con el proveedor de almacenamiento real
/// (Azure Blob Storage, AWS S3, etc.).
/// </summary>
public class StorageService : IStorageService
{
    public Task<string> SubirArchivoAsync(IFormFile archivo)
    {
        throw new NotImplementedException(
            "StorageService no está implementado. Configure un proveedor de almacenamiento real (Azure Blob, S3, etc.).");
    }
}
