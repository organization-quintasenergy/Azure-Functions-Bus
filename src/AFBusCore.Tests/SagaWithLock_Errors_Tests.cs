using AFBus;
using AFBus.Tests.TestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace AFBus.Tests
{
    [TestClass]
    public class SagaWithLock_Errors_Tests
    {
        [TestMethod]
        public void Sagas_With_Locks_Lock_is_freed_in_exception()
        {
            var sagaId = Guid.NewGuid();

            var container = new HandlersContainer(true);

            Assert.IsTrue(container.messageToSagaDictionary[typeof(ErrorSagaStartingMessage)].Count == 1);

            container.HandleAsync(new ErrorSagaStartingMessage() { Id = sagaId }, null).Wait();

            try
            {
                container.HandleAsync(new ErrorSagaIntermediateMessage() { Id = sagaId }, null).Wait();
            }
            catch
            { }

            var locker = new SagaAzureStorageLocker();

            locker.CreateLock(ErrorTestSaga.PARTITION_KEY + sagaId.ToString()).Wait();
        }
    }
}
