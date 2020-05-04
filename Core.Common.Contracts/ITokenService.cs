using Core.Common.DataModels.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Core.Common.Contracts
{
    public interface ITokenService<T> where T: IdentityUser, IBaseUser
    {
        string BuildAccessToken(T user);
        bool ValidateToken(string accessToken);
        Task<string> GenerateRefreshToken(T user);
        Task<string> RefreshAccessToken(string userName, string refreshTokenString);
    }
}
