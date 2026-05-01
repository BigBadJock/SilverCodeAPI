using System.Threading;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IAuditor
    {
        Task AuditAsync(string message, CancellationToken cancellationToken = default);
    }
}
