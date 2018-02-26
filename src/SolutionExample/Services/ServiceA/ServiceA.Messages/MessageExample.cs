using System;
using AFUtils.IoC;

namespace ServiceA.Messages
{
    public class MessageExample : ICommand
    {
        public string SomeInfo { get; set; }

        public Guid TransactionID { get; set; }
    }
}
