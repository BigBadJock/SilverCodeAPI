using Core.Common.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseAuditor : IAuditor
    {
        public void Audit(string audit)
        {
            throw new NotImplementedException();
        }
    }
}
