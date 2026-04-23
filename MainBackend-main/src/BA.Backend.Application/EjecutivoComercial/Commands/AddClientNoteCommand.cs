using MediatR;
using System;

namespace BA.Backend.Application.EjecutivoComercial.Commands;

public record AddClientNoteCommand(
    Guid StoreId, 
    Guid AuthorId, 
    string Content, 
    Guid TenantId) : IRequest<Guid>;
