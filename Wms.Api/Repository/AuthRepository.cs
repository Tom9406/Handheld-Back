namespace Wms.Api.Repository
{
    using Dapper;
    using System.Data;
    using Wms.Api.DTOs;

    public class AuthRepository : IAuthRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AuthRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<LoginRawDto>> Login(string email)
        {
            using var connection = _connectionFactory.CreateConnection();

            return await connection.QueryAsync<LoginRawDto>(
                "auth.sp_LoginUser",
                new { Email = email },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
