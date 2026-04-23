using BA.Backend.Application.EjecutivoComercial.Commands;
using BA.Backend.Application.Exceptions;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BA.Backend.Application.EjecutivoComercial.Handlers;

public class AddClientNoteCommandHandler : IRequestHandler<AddClientNoteCommand, Guid>
{
    private readonly IClientNoteRepository _noteRepository;
    private readonly IStoreRepository _storeRepository;

    public AddClientNoteCommandHandler(IClientNoteRepository noteRepository, IStoreRepository storeRepository)
    {
        _noteRepository = noteRepository;
        _storeRepository = storeRepository;
    }

    public async Task<Guid> Handle(AddClientNoteCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar que la tienda (cliente) existe
        var store = await _storeRepository.GetByIdAsync(request.StoreId, cancellationToken);
        if (store == null || store.TenantId != request.TenantId)
            throw new NotFoundException("Store", request.StoreId);

        // 2. Crear la nota
        var note = new ClientNote
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            StoreId = request.StoreId,
            AuthorId = request.AuthorId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _noteRepository.AddAsync(note, cancellationToken);
        await _noteRepository.SaveChangesAsync(cancellationToken);

        return note.Id;
    }
}
