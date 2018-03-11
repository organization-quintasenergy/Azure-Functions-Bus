using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public interface ISagaLocker
    {
        Task CreateLocksContainer();

        Task<string> CreateLock(string sagaId);

        Task ReleaseLock(string sagaId, string leaseId);
    }
}
