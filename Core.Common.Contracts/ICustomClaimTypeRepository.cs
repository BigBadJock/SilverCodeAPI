using Core.Common.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Core.Common.Contracts
{
    public interface ICustomClaimTypeRepository<DBC> : IRepository<DBC, CustomClaimType>
        where DBC : DbContext
    {
    }
}
