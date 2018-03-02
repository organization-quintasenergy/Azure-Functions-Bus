using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public interface ISendMessages
    {
        Task AddMessageAsync<T>(T message, string serviceName, TimeSpan? initialVisibilityDelay = null) where T : class;
    }
}
