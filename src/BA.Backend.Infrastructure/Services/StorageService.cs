
using BA.Backend.Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace BA.Backend.Infrastructure.Services;

public class StorageService : IStorageService
{
    public async Task<string> SubirArchivoAsync(IFormFile archivo)
    {
        return await Task.FromResult("https://url_de_prueba.com/" + archivo.FileName);
    }
} 
