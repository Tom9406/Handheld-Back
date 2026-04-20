using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Api.DTOs;
using Wms.Api.Repository;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _repo;
    private readonly IJwtService _jwt;

    public AuthController(IAuthRepository repo, IJwtService jwt)
    {
        _repo = repo;
        _jwt = jwt;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("EMAIL_AND_PASSWORD_REQUIRED");

        var result = (await _repo.Login(request.Email)).ToList();

        if (!result.Any())
            return Unauthorized("USER_NOT_FOUND");

        var user = result.First();

        if (string.IsNullOrEmpty(user.PasswordHash))
            return Unauthorized("INVALID_USER");

        var valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!valid)
            return Unauthorized("INVALID_PASSWORD");

        var companies = result
            .Where(x => x.CompanyId != null)
            .GroupBy(x => x.CompanyId)
            .Select(g => new AuthCompanyAccessDto
            {
                CompanyId = g.Key!.Value,
                RoleCode = g.First().RoleCode
            })
            .ToList();

        var token = _jwt.GenerateToken(
            user.UserId,
            user.Email,
            false,
            companies
        );

        return Ok(new
        {
            token,
            user.UserId,
            user.Email,
            user.FullName,
            Companies = companies
        });
    }

    [Authorize]
    [HttpGet("test")]
    public IActionResult Test()
    {
        var userId = User.FindFirst("userId")?.Value;
        var companyId = User.FindFirst("companyId")?.Value;
        var role = User.FindFirst("role")?.Value;
        var companies = User.FindAll("company_access").Select(x => x.Value).ToList();

        return Ok(new
        {
            message = "OK",
            userId,
            companyId,
            role,
            companies
        });
    }
}
