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
        [TestMethod]
        public void Sagas_With_Events_Multiple_Saga_Can_Receive_An_Event()
        {
            InvocationCounter.Instance.Reset();

            var sagaId1 = Guid.NewGuid();
            var sagaId2 = Guid.NewGuid();

            var container = new HandlersContainer(true);

            Assert.IsTrue(container.messageToSagaDictionary[typeof(EventSagaStartingMessage)].Count == 1);

            container.HandleCommandAsync(new EventSagaStartingMessage() { Id = sagaId1 }, null).Wait();
            container.HandleCommandAsync(new EventSagaStartingMessage() { Id = sagaId2 }, null).Wait();

            container.HandleCommandAsync(new EventSagaIntermediateMessage() {}, null).Wait();

            Assert.IsTrue(InvocationCounter.Instance.Counter == 2);

            container.HandleCommandAsync(new EventSagaTerminatingMessage() { Id = sagaId1 }, null).Wait();
            container.HandleCommandAsync(new EventSagaTerminatingMessage() { Id = sagaId2 }, null).Wait();
         
        }

    }
}
