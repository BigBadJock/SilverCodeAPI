using Microsoft.EntityFrameworkCore;
using System;

namespace Core.Common.Contracts
{
    public interface IDatabaseFactory : IDisposable
    {
        DbContext GetDatabase();
    }
}
