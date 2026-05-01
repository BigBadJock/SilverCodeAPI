using System.Threading;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IUnitOfWork
    {
        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}
