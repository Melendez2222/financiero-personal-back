using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinancieroPersonal.Application.Abstractions;
using FinancieroPersonal.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace FinancieroPersonal.Infrastructure.Auth;

public class JwtTokenService(JwtOptions options) : IJwtTokenService
{
    public string GenerarToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim("nombre", usuario.Nombre),
        };

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(options.ExpiraMinutos),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
