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
        readonly static string SERVICENAME = "TESTSERVICE";

        [TestMethod]
        public void Sagas_With_Locks_Lock_is_freed_in_exception()
        {
            var sagaId = Guid.NewGuid();

            var container = new HandlersContainer(SERVICENAME,true);

            Assert.IsTrue(container.messageToSagaDictionary[typeof(ErrorSagaStartingMessage)].Count == 1);

            container.HandleAsync(new ErrorSagaStartingMessage() { Id = sagaId }, null).Wait();

            try
            {
                container.HandleAsync(new ErrorSagaIntermediateMessage() { Id = sagaId }, null).Wait();
            }
            catch
            { }

            var locker = new SagaAzureStorageLocker();

            var lease = locker.CreateLock(ErrorTestSaga.PARTITION_KEY + sagaId.ToString()).Result;
            locker.DeleteLock(ErrorTestSaga.PARTITION_KEY + sagaId.ToString(),lease).Wait();

            container.HandleAsync(new ErrorSagaTerminatingMessage() { Id = sagaId }, null).Wait();
        }
    }
}
