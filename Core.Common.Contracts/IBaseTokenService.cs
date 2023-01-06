using Core.Common.DataModels.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IBaseTokenService<DBC, T>
        where T : IdentityUser, IBaseUser
        where DBC : DbContext
    {
        Task<string> BuildAccessToken(T user);
        bool ValidateToken(string accessToken);
        Task<string> GenerateRefreshToken(T user);
        Task<string> RefreshAccessToken(string userName, string refreshTokenString);
    }
}
