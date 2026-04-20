namespace Wms.Api.DTOs
{
    public class LoginRawDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public Guid? CompanyId { get; set; }
        public string RoleCode { get; set; } = string.Empty;
    }
}
