using BA.Backend.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BA.Backend.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> UploadPhotoAsync(Stream file, string fileName, Guid tenantId)
    {
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", tenantId.ToString());
        
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/uploads/{tenantId}/{fileName}";
    }
}
