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
        public void SagasAreCorrectlyStarted()
        {
            var container = new HandlersContainer();

            Assert.IsTrue(container.messageToSagaDictionary[typeof(SagaStartingMessage)].Count == 1);

            container.InvokeAsync(new SagaStartingMessage(), null);
        }
    }
}
