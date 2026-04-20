namespace Wms.Api.DTOs
{
    /// <summary>
    /// Represents one company the authenticated user can operate on.
    /// </summary>
    public class AuthCompanyAccessDto
    {
        public Guid CompanyId { get; set; }

        public string RoleCode { get; set; } = string.Empty;
    }
}
