using System;
using System.Configuration;
using System.Threading.Tasks;
using AFBus.Tests.TestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AFBus.Tests
{
    [TestClass]
    public class Saga_Tests
    {
        

        [TestMethod]
        public void Sagas_Without_Locks_Are_Correctly_Scanned()
        {
            var container = new HandlersContainer(false); 

            Assert.IsTrue(container.messageToSagaDictionary[typeof(SimpleSagaStartingMessage)].Count == 1);
        }

        [TestMethod]
        public void Sagas_Without_Locks_Are_Correctly_Started_And_A_Message_Is_Not_Correlated()
        {

            var sagaId = Guid.NewGuid();

            var container = new HandlersContainer(false);

            Assert.IsTrue(container.messageToSagaDictionary[typeof(SimpleSagaStartingMessage)].Count == 1);

            container.HandleCommandAsync(new SimpleSagaStartingMessage() { Id = sagaId }, null).Wait();
                        
            //non correlating message
            container.HandleCommandAsync(new SimpleSagaIntermediateMessage() { Id = Guid.NewGuid() }, null).Wait();          
            
        }

        [TestMethod]
        public void Sagas_Without_Locks_Are_Correctly_Started_And_10_Non_Parallel_Messages_Are_Correlated()
        {
            var sagaId = Guid.NewGuid();

            var container = new HandlersContainer(false);

            Assert.IsTrue(container.messageToSagaDictionary[typeof(SimpleSagaStartingMessage)].Count == 1);

            container.HandleCommandAsync(new SimpleSagaStartingMessage() { Id = sagaId }, null).Wait();

            for(int i=0;i<10;i++)
                container.HandleCommandAsync(new SimpleSagaIntermediateMessage() { Id = sagaId }, null).Wait();

            var lockSaga = false;
            var sagaPersistence = new SagaAzureStoragePersistence(new SagaAzureStorageLocker(), lockSaga);

            var sagaData = sagaPersistence.GetSagaDataAsync<SimpleTestSagaData>("SimpleTestSaga", sagaId.ToString()).Result as SimpleTestSagaData;

            Assert.IsTrue(sagaData.Counter == 11);

            container.HandleCommandAsync(new SimpleSagaTerminatingMessage() { Id = sagaId }, null).Wait();
        }

        
        [TestMethod]
        public void Sagas_Without_Locks_Can_Be_Singletons()
        {
            var sagaId = Guid.NewGuid();

            var container = new HandlersContainer(false);

            Assert.IsTrue(container.messageToSagaDictionary[typeof(SingletonSagaStartingMessage)].Count == 1);

            container.HandleCommandAsync(new SingletonSagaStartingMessage() { Id = sagaId }, null).Wait();
           
            container.HandleCommandAsync(new SingletonSagaStartingMessage() { Id = sagaId }, null).Wait();

            var lockSaga = false;
            var sagaPersistence = new SagaAzureStoragePersistence(new SagaAzureStorageLocker(), lockSaga);

            var sagaData = sagaPersistence.GetSagaDataAsync<SingletonTestSagaData>("SingletonTestSaga", sagaId.ToString()).Result as SingletonTestSagaData;

            Assert.IsTrue(sagaData.Counter == 1);
        }
    }
}
