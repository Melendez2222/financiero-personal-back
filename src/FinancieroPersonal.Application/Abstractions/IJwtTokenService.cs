using FinancieroPersonal.Domain.Entities;

namespace FinancieroPersonal.Application.Abstractions;

public interface IJwtTokenService
{
    string GenerarToken(Usuario usuario);
}
