using System;
using System.Data.Entity;

namespace Core.Common.Contracts
{
    public interface IDatabaseFactory : IDisposable
    {
        DbContext GetDatabase();
    }
}
