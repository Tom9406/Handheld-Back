using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Wms.Api.Controllers;

[Authorize]
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected Guid UserId => GetRequiredGuidClaim("userId");

    protected string UserEmail => User.FindFirst("email")?.Value ?? string.Empty;

    protected string Role => User.FindFirst("role")?.Value ?? string.Empty;

    protected Guid CompanyId => ResolveCompanyId();

    /// <summary>
    /// Resolves the company context for the request without trusting arbitrary query values.
    /// </summary>
    protected Guid ResolveCompanyId(Guid? requestedCompanyId = null)
    {
        if (requestedCompanyId.HasValue)
        {
            EnsureCompanyAccess(requestedCompanyId.Value);
            return requestedCompanyId.Value;
        }

        if (Request.Headers.TryGetValue("X-Company-Id", out var headerValues)
            && Guid.TryParse(headerValues.FirstOrDefault(), out var headerCompanyId))
        {
            EnsureCompanyAccess(headerCompanyId);
            return headerCompanyId;
        }

        var defaultCompanyId = GetOptionalGuidClaim("companyId");
        if (defaultCompanyId.HasValue)
        {
            EnsureCompanyAccess(defaultCompanyId.Value);
            return defaultCompanyId.Value;
        }

        throw new UnauthorizedAccessException("The token does not contain a valid company context.");
    }

    protected IReadOnlyCollection<Guid> AccessibleCompanyIds =>
        User.FindAll("company_access")
            .Select(claim => Guid.TryParse(claim.Value, out var value) ? value : Guid.Empty)
            .Where(value => value != Guid.Empty)
            .Distinct()
            .ToArray();

    protected bool HasCompanyAccess(Guid companyId) =>
        AccessibleCompanyIds.Contains(companyId);

    protected void EnsureCompanyAccess(Guid companyId)
    {
        if (!HasCompanyAccess(companyId))
            throw new UnauthorizedAccessException("The authenticated user has no access to the requested company.");
    }

    private Guid GetRequiredGuidClaim(string claimType)
    {
        var value = User.FindFirst(claimType)?.Value;
        if (!Guid.TryParse(value, out var parsed))
            throw new UnauthorizedAccessException($"Missing or invalid claim '{claimType}'.");

        return parsed;
    }

    private Guid? GetOptionalGuidClaim(string claimType)
    {
        var value = User.FindFirst(claimType)?.Value;
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }
}
