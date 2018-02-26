using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFUtils.IoC
{
    public interface ICommand
    {
        Guid TransactionID { get; set; }
    }
}
