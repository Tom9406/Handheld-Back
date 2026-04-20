using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Wms.Api.DTOs;
using Wms.Api.Repository;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(Guid userId, string email, bool isSuperAdmin, IReadOnlyCollection<AuthCompanyAccessDto> companies)
    {
        var jwtKey = _config["Jwt:Key"];
        var expiresInMinutes = _config["Jwt:ExpiresInMinutes"];

        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("Jwt:Key is missing.");

        if (!int.TryParse(expiresInMinutes, out var expirationMinutes))
            throw new InvalidOperationException("Jwt:ExpiresInMinutes is invalid.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("userId", userId.ToString()),
            new("email", email),
            new("isSuperAdmin", isSuperAdmin.ToString())
        };

        foreach (var company in companies)
        {
            claims.Add(new Claim("company_access", company.CompanyId.ToString()));
        }

        var defaultCompany = companies.FirstOrDefault();
        if (defaultCompany is not null)
        {
            claims.Add(new Claim("companyId", defaultCompany.CompanyId.ToString()));
            claims.Add(new Claim("role", defaultCompany.RoleCode ?? string.Empty));
            claims.Add(new Claim(ClaimTypes.Role, defaultCompany.RoleCode ?? string.Empty));
        }

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
