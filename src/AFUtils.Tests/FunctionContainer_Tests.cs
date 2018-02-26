using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AFUtils.Tests
{
    [TestClass]
    public class FunctionContainer_Tests
    {
        [TestMethod]
        public void FunctionContainer_IFunctionTypesAreCorrectlyScanned()
        {
            var container = new FunctionContainer();

            Assert.IsTrue(container.messageHandlersDictionary.Count == 1);

        }


      
    }
}
