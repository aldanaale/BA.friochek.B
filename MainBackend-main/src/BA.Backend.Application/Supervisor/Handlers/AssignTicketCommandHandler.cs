using BA.Backend.Application.Supervisor.Commands;
using BA.Backend.Application.Exceptions;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Enums;
using BA.Backend.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.Supervisor.Handlers;

public class AssignTicketCommandHandler : IRequestHandler<AssignTicketCommand, bool>
{
    private readonly ITechSupportRepository _techSupportRepository;
    private readonly IUserRepository _userRepository;

    public AssignTicketCommandHandler(
        ITechSupportRepository techSupportRepository, 
        IUserRepository userRepository)
    {
        _techSupportRepository = techSupportRepository;
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(AssignTicketCommand request, CancellationToken cancellationToken)
    {
        // 1. Obtener el ticket
        var ticket = await _techSupportRepository.GetByIdAsync(request.TicketId, request.TenantId, cancellationToken);

        if (ticket == null)
            throw new NotFoundException("TechSupportRequest", request.TicketId);

        // 2. Validar que el técnico existe
        var technician = await _userRepository.GetByIdAsync(request.TechnicianId, request.TenantId, cancellationToken);

        if (technician == null || technician.Role != UserRole.Tecnico)
            throw new BadRequestException("El usuario seleccionado no es un técnico válido para este tenant.");

        // 3. Asignar
        ticket.TechnicianId = request.TechnicianId;
        ticket.Status = "Asignado";

        await _techSupportRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
