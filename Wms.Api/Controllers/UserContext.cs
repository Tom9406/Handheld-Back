using System.Security.Claims;

namespace Wms.Api.Controllers
{
    public static class UserContext
    {
        public static Guid? GetUserId(ClaimsPrincipal user)
            => TryGetGuid(user, "userId");

        public static Guid? GetCompanyId(ClaimsPrincipal user)
            => TryGetGuid(user, "companyId");

        public static string GetRole(ClaimsPrincipal user)
            => user.FindFirst("role")?.Value ?? string.Empty;

        private static Guid? TryGetGuid(ClaimsPrincipal user, string claimType)
        {
            var value = user.FindFirst(claimType)?.Value;
            return Guid.TryParse(value, out var parsed) ? parsed : null;
        }
    }
}
