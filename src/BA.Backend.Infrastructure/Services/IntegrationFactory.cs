using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BA.Backend.Infrastructure.Services.Integrations;

namespace BA.Backend.Infrastructure.Services;

/// <summary>
/// El "Cerebro" del Hub de Integración: Resuelve a qué sistema externo
/// llamar basado en la configuración del Tenant actual.
/// </summary>
public class IntegrationFactory : IIntegrationFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IntegrationFactory> _logger;

    public IntegrationFactory(IServiceProvider serviceProvider, ILogger<IntegrationFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IExternalIntegrationService Create(Tenant tenant)
    {
        var type = (IntegrationType)tenant.IntegrationType;

        _logger.LogInformation("Resolviendo Adaptador de Integración para el Tenant: {Tenant} (Tipo: {Type})", tenant.Name, type);

        return type switch
        {
            IntegrationType.SavoryDirect => _serviceProvider.GetRequiredService<SavoryIntegrationAdapter>(),
            _ => _serviceProvider.GetRequiredService<MockIntegrationAdapter>()
        };
    }
}
