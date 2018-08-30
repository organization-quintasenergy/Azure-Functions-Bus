using AFBus;
using AFBus.Tests.TestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace AFBus.Tests
{
    [TestClass]
    public class SagaWithEvent_Tests
    {
        readonly static string SERVICENAME = "TESTSERVICE";

        [TestMethod]
        public void Sagas_With_Events_Multiple_Saga_Can_Receive_An_Event()
        {
            InvocationCounter.Instance.Reset();

            var sagaId1 = Guid.NewGuid();
            var sagaId2 = Guid.NewGuid();

            var container = new HandlersContainer(SERVICENAME,true);

            Assert.IsTrue(container.messageToSagaDictionary[typeof(EventSagaStartingMessage)].Count == 1);

            container.HandleAsync(new EventSagaStartingMessage() { Id = sagaId1 }, null).Wait();
            container.HandleAsync(new EventSagaStartingMessage() { Id = sagaId2 }, null).Wait();

            container.HandleAsync(new EventSagaIntermediateMessage() {}, null).Wait();

            Assert.IsTrue(InvocationCounter.Instance.Counter == 2);

            container.HandleAsync(new EventSagaTerminatingMessage() { Id = sagaId1 }, null).Wait();
            container.HandleAsync(new EventSagaTerminatingMessage() { Id = sagaId2 }, null).Wait();
         
        }

    }
}
