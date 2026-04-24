using System;
using System.IO;
using System.Threading.Tasks;

namespace BA.Backend.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadPhotoAsync(Stream file, string fileName, Guid tenantId);
}
