using System;
using AFBus.Tests.TestClasses.DI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AFBus.Tests
{
    [TestClass]
    public class DI_Tests
    {
        [TestMethod]
        public void DI_Nominal()
        {
            var container = new HandlersContainer();
            HandlersContainer.AddDependency<IUoWTest, UoWTest>();

            Assert.IsTrue(container.messageHandlersDictionary[typeof(DIMessage)].Count == 1);

            container.HandleCommandAsync(new DIMessage(), null).Wait();
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void DI_Dependency_not_found()
        {            
            var container = new HandlersContainer();
            HandlersContainer.ClearDependencies();
           
            Assert.IsTrue(container.messageHandlersDictionary[typeof(DIMessage)].Count == 1);


            try
            {
                container.HandleCommandAsync(new DIMessage(), null).Wait();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.InnerException.Message.Contains("No depedency can be solved"));
                throw ex;
            }

        }
    }
}
