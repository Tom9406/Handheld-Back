using Wms.Api.DTOs;

namespace Wms.Api.Repository
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string email, bool isSuperAdmin, IReadOnlyCollection<AuthCompanyAccessDto> companies);
    }
}
