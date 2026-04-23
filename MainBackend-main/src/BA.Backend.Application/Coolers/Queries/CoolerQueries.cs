using BA.Backend.Application.Coolers.DTOs;
using MediatR;
using System.Collections.Generic;

namespace BA.Backend.Application.Coolers.Queries;

public record GetAllCoolersQuery(Guid TenantId) : IRequest<IEnumerable<CoolerListDto>>;

public record GetCoolerByIdQuery(Guid Id, Guid TenantId) : IRequest<CoolerDto?>;

public record GetCoolerTagsQuery(Guid CoolerId, Guid TenantId) : IRequest<NfcTagDto?>;