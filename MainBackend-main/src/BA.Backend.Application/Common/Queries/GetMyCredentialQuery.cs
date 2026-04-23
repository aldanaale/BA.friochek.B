using MediatR;
using BA.Backend.Application.Common.Interfaces;

namespace BA.Backend.Application.Common.Queries;

public record GetMyCredentialQuery() : IRequest<string>;

public class GetMyCredentialQueryHandler : IRequestHandler<GetMyCredentialQuery, string>
{
    private readonly ICurrentTenantService _tenantService;
    private readonly IQrGeneratorService _qrService;
    private readonly IJwtTokenService _jwtService;

    public GetMyCredentialQueryHandler(
        ICurrentTenantService tenantService,
        IQrGeneratorService qrService,
        IJwtTokenService jwtService)
    {
        _tenantService = tenantService;
        _qrService = qrService;
        _jwtService = jwtService;
    }

    public async Task<string> Handle(GetMyCredentialQuery request, CancellationToken cancellationToken)
    {
        var userId = _tenantService.UserId ?? throw new UnauthorizedAccessException();
        var role = _tenantService.Role ?? "Unknown";

        var (token, _) = _jwtService.GenerateCredentialToken(userId, role);
        var qrBase64 = _qrService.GenerateQrBase64(token);
        
        return await Task.FromResult(qrBase64);
    }
}
