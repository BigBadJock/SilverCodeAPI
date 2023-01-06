using Core.Common.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Core.Common.Contracts
{
    public interface IRefreshTokenRepository<DBC> : IRepository<DBC, RefreshToken>
        where DBC : DbContext
    {

    }
}
