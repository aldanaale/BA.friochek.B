using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using BA.Backend.Application.Common.Interfaces;

namespace BA.Backend.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        builder.UseSqlServer(connectionString);

        return new ApplicationDbContext(builder.Options, new CurrentTenantServiceStub());
    }
}

// Stub service since we don't have a real request context at design time
public class CurrentTenantServiceStub : BA.Backend.Application.Common.Interfaces.ICurrentTenantService
{
    public Guid? TenantId => null;
    public string? UserId => null;
    public string? Role => null;
    public bool IsPlatformAdmin => false;
    public string? IpAddress => null;
    public string? DeviceFingerprint => null;
}
