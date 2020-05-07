using Core.Common.DataModels.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Core.Common.Contracts
{
    public interface IBaseTokenService<T> where T: IdentityUser, IBaseUser
    {
        Task<string> BuildAccessToken(T user);
        bool ValidateToken(string accessToken);
        Task<string> GenerateRefreshToken(T user);
        Task<string> RefreshAccessToken(string userName, string refreshTokenString);
    }
}
