using Core.Common.Contracts;
using Core.Common.DataModels;
using Core.Common.DataModels.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseTokenService<T> : ITokenService<T> where T : IdentityUser, IBaseUser
    {
        protected readonly UserManager<T> userManager;
        protected readonly SignInManager<T> signInManager;
        protected readonly JWTSettings options;
        protected IRefreshTokenRepository refreshTokenRepository;
        protected IUserClaimRepository userClaimRepository;

        //public BaseTokenService(UserManager<T> userManager, SignInManager<T> signInManager, IOptions<JWTSettings> optionsAccessor, IRefreshTokenRepository refreshTokenRepository)
        //{
        //    this.userManager = userManager;
        //    this.signInManager = signInManager;
        //    this.options = optionsAccessor.Value;
        //    this.refreshTokenRepository = refreshTokenRepository;
        //}

        public string BuildAccessToken(T user)
        {
            var secret = options.SecretKey;

            IEnumerable<UserClaim> claims = this.userClaimRepository.GetAll().Where(x => x.UserId == user.Id).ToList();


            var claimIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
            });

            foreach(UserClaim claim in claims)
            {
                claimIdentity.AddClaim(new Claim(claim.CustomClaimId.ToString(), true.ToString()));
            }

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: options.Issuer,
                audience: options.Audience,
                claims: claimIdentity.Claims,
                expires: DateTime.Now.AddMinutes(options.ExpiryMinutes),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey)),
                    SecurityAlgorithms.HmacSha256
                )
            );

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshToken(T user)
        {
            Guid refreshTokenGuid = Guid.NewGuid();
            RefreshToken newToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenGuid,
                Expiry = DateTime.Now.AddYears(1)
            };

            RefreshToken existingToken = this.refreshTokenRepository.GetAll().Where(x => x.UserId == user.Id).FirstOrDefault();
            if(existingToken != null)
            {
                await refreshTokenRepository.Delete(existingToken);
            }
            await refreshTokenRepository.Add(newToken);

            return refreshTokenGuid.ToString();
        }

        public async Task<string> RefreshAccessToken(string userName, string refreshTokenString)
        {
            string accessToken = string.Empty;

            T user = await this.userManager.FindByEmailAsync(userName);
            if(user != null)
            {
                RefreshToken token = this.refreshTokenRepository.GetAll().Where(x => x.UserId == user.Id).FirstOrDefault();
                if(token != null)
                {
                    if (token.Token.ToString().Equals(refreshTokenString))
                    {
                        if(token.Expiry > DateTime.Now)
                        {
                            accessToken = this.BuildAccessToken(user);
                        }
                    }
                }
            }

            return accessToken;
        }

        public bool ValidateToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();
            tokenHandler.ValidateToken(accessToken, validationParameters, out _);
            return true;
        }

        private  TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = this.options.Issuer,
                ValidAudience = this.options.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey))
            };
        }
    }
}
