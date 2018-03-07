using System;
using AFBus.Tests.TestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AFBus.Tests
{
    [TestClass]
    public class Saga_Tests
    {

        [TestMethod]
        public void SagasAreCorrectlyScanned()
        {
            var container = new HandlersContainer();

            Assert.IsTrue(container.messageToSagaDictionary[typeof(SagaStartingMessage)].Count == 1);
        }


        [TestMethod]
        public void SagasAreCorrectlyStartedAndCorrelated()
        {
            var sagaId = Guid.NewGuid();

            var container = new HandlersContainer();

            Assert.IsTrue(container.messageToSagaDictionary[typeof(SagaStartingMessage)].Count == 1);

            container.HandleAsync(new SagaStartingMessage() { Id = sagaId }, null).Wait();

            for(int i=0;i<10;i++)
                container.HandleAsync(new SagaIntermediateMessage() { Id = sagaId }, null).Wait();

            var sagaPersistence = new SagaAzureStoragePersistence();

            var sagaData = sagaPersistence.GetSagaData<TestSagaData>("TestSaga", sagaId.ToString()).Result as TestSagaData;

            Assert.IsTrue(sagaData.Counter == 11);
        }
    }
}
