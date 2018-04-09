
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIExample.ViewModels;

namespace UIExample.Proxies
{
    public interface IPaymentProxies
    {
        Task<List<PaymentViewModel>> GetPayments();
    }
}
