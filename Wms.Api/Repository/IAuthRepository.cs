using Wms.Api.DTOs;

namespace Wms.Api.Repository
{
    public interface IAuthRepository
    {
        Task<IEnumerable<LoginRawDto>> Login(string email);
    }
}
