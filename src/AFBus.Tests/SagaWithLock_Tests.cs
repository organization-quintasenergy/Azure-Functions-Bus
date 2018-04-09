using System;
using System.Configuration;
using System.Threading.Tasks;
using AFBus.Tests.TestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AFBus.Tests
{
    [TestClass]
    public class SagaWithLock_Tests
    {
        

        [TestMethod]
        public void Sagas_With_Locks_Are_Correctly_Scanned()
        {
            var container = new HandlersContainer(true);

            Assert.IsTrue(container.messageToSagaDictionary[typeof(SimpleSagaStartingMessage)].Count == 1);
        }

        [TestMethod]
        public void Sagas_With_Locks_Are_Correctly_Started_And_A_Message_Is_Not_Correlated()
        {
            var sagaId = Guid.NewGuid();

            var container = new HandlersContainer(true);

            Assert.IsTrue(container.messageToSagaDictionary[typeof(SimpleSagaStartingMessage)].Count == 1);

            container.HandleAsync(new SimpleSagaStartingMessage() { Id = sagaId }, null).Wait();
                        
            //non correlating message
            container.HandleAsync(new SimpleSagaIntermediateMessage() { Id = Guid.NewGuid() }, null).Wait();          
            
        }

        [TestMethod]
        public void Sagas_With_Locks_Are_Correctly_Started_And_10_Non_Parallel_Messages_Are_Correlated()
        {
            var sagaId = Guid.NewGuid();

            var container = new HandlersContainer(true);

            Assert.IsTrue(container.messageToSagaDictionary[typeof(SimpleSagaStartingMessage)].Count == 1);

            container.HandleAsync(new SimpleSagaStartingMessage() { Id = sagaId }, null).Wait();

            for(int i=0;i<10;i++)
                container.HandleAsync(new SimpleSagaIntermediateMessage() { Id = sagaId }, null).Wait();

            var lockSaga = false;
            var sagaPersistence = new SagaAzureStoragePersistence(new SagaAzureStorageLocker(), lockSaga);

            var sagaData = sagaPersistence.GetSagaData<SimpleTestSagaData>("SimpleTestSaga", sagaId.ToString()).Result as SimpleTestSagaData;

            Assert.IsTrue(sagaData.Counter == 11);

            container.HandleAsync(new SimpleSagaTerminatingMessage() { Id = sagaId }, null).Wait();
        }

        [TestMethod]
        public void Sagas_With_Locks_Are_Correctly_Started_And_10_Parallel_Messages_Are_Correlated()
        {
            InvocationCounter.Instance.Reset();
            var sagaId = Guid.NewGuid();

            var container = new HandlersContainer(true);

            Assert.IsTrue(container.messageToSagaDictionary[typeof(SimpleSagaStartingMessage)].Count == 1);

            container.HandleAsync(new SimpleSagaStartingMessage() { Id = sagaId }, null).Wait();

            Parallel.For(0, 10, i =>
            {
                bool retry = true;
                while (retry)
                {
                    try
                    {
                        container.HandleAsync(new SimpleSagaIntermediateMessage() { Id = sagaId }, null).Wait();
                        retry = false;
                    }
                    catch(Exception)
                    {
    
                    }

                }
            });

            var lockSaga = false;
            var sagaPersistence = new SagaAzureStoragePersistence(new SagaAzureStorageLocker(), lockSaga);

            var sagaData = sagaPersistence.GetSagaData<SimpleTestSagaData>("SimpleTestSaga", sagaId.ToString()).Result as SimpleTestSagaData;

            Assert.IsTrue(sagaData.Counter == 11);

            Assert.IsTrue(InvocationCounter.Instance.Counter == 11);
            
            container.HandleAsync(new SimpleSagaTerminatingMessage() { Id = sagaId }, null).Wait();
        }

        [TestMethod]
        public void Sagas_With_Locks_Can_Be_Singletons()
        {
            var sagaId = Guid.NewGuid();

            var container = new HandlersContainer(true);

            Assert.IsTrue(container.messageToSagaDictionary[typeof(SingletonSagaStartingMessage)].Count == 1);

            container.HandleAsync(new SingletonSagaStartingMessage() { Id = sagaId }, null).Wait();
           
            container.HandleAsync(new SingletonSagaStartingMessage() { Id = sagaId }, null).Wait();

            var lockSaga = false;
            var sagaPersistence = new SagaAzureStoragePersistence(new SagaAzureStorageLocker(), lockSaga);

            var sagaData = sagaPersistence.GetSagaData<SingletonTestSagaData>("SingletonTestSaga", sagaId.ToString()).Result as SingletonTestSagaData;

            Assert.IsTrue(sagaData.Counter == 1);
        }
    }
}
