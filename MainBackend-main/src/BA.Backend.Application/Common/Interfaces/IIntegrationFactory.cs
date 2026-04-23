using BA.Backend.Domain.Entities;

namespace BA.Backend.Application.Common.Interfaces;

public interface IIntegrationFactory
{
    IExternalIntegrationService Create(Tenant tenant);
}
