using AFBus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace AFBus.Tests
{
    [TestClass]
    public class JSONSerializer_tests
    {
        [TestMethod]
        public void JSONSerializer_SelfReferencing()
        {
            var serializer = new JSONSerializer();

            try
            {
                throw new Exception("asdf");
            }
            catch (Exception ex)
            {
                serializer.Serialize(ex);
            }

            
        }
    }
}
