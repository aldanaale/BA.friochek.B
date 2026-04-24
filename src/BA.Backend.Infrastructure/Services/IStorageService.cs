
using Microsoft.AspNetCore.Http;

namespace BA.Backend.Infrastructure.Services;

public interface IStorageService
{
    Task<string> SubirArchivoAsync(IFormFile archivo);
}
