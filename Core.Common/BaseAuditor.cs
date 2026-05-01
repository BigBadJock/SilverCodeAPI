using Core.Common.Contracts;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Common
{
    /// <summary>
    /// Default audit implementation that writes to the application logger.
    /// Override AuditAsync in a derived class to persist to a dedicated audit store.
    /// </summary>
    public abstract class BaseAuditor : IAuditor
    {
        private readonly ILogger<BaseAuditor> logger;

        protected BaseAuditor(ILogger<BaseAuditor> logger)
        {
            this.logger = logger;
        }

        public virtual Task AuditAsync(string message, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("AUDIT: {Message}", message);
            return Task.CompletedTask;
        }
    }
}
