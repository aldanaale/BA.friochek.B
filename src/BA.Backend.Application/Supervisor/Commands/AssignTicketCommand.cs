using MediatR;
using System;

namespace BA.Backend.Application.Supervisor.Commands;

/// <summary>
/// Comando para asignar un técnico a un ticket de soporte específico.
/// </summary>
public record AssignTicketCommand(
    Guid TicketId, 
    Guid TechnicianId, 
    Guid TenantId) : IRequest<bool>;
