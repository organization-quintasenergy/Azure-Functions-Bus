using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFUtils
{
    public interface IBus
    {
        Task SendAsync<T>(T input, string serviceName);
    }
}
