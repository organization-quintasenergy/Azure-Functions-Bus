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

            Assert.IsTrue(container.messageToSagaDictionary[typeof(SimpleSagaStartingMessage)].Count == 1);
        }


        [TestMethod]
        public void SagasAreCorrectlyStartedAndCorrelated()
        {
            var sagaId = Guid.NewGuid();

            var container = new HandlersContainer();

            Assert.IsTrue(container.messageToSagaDictionary[typeof(SimpleSagaStartingMessage)].Count == 1);

            container.HandleAsync(new SimpleSagaStartingMessage() { Id = sagaId }, null).Wait();

            for(int i=0;i<10;i++)
                container.HandleAsync(new SimpleSagaIntermediateMessage() { Id = sagaId }, null).Wait();

            var sagaPersistence = new SagaAzureStoragePersistence();

            var sagaData = sagaPersistence.GetSagaData<SimpleTestSagaData>("SimpleTestSaga", sagaId.ToString()).Result as SimpleTestSagaData;

            Assert.IsTrue(sagaData.Counter == 11);
        }

        [TestMethod]
        public void SagasCanBeSingletons()
        {
            var sagaId = Guid.NewGuid();

            var container = new HandlersContainer();

            Assert.IsTrue(container.messageToSagaDictionary[typeof(SingletonSagaStartingMessage)].Count == 1);

            container.HandleAsync(new SingletonSagaStartingMessage() { Id = sagaId }, null).Wait();
           
            container.HandleAsync(new SingletonSagaStartingMessage() { Id = sagaId }, null).Wait();

            var sagaPersistence = new SagaAzureStoragePersistence();

            var sagaData = sagaPersistence.GetSagaData<SingletonTestSagaData>("SingletonTestSaga", sagaId.ToString()).Result as SingletonTestSagaData;

            Assert.IsTrue(sagaData.Counter == 1);
        }
    }
}
