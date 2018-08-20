using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AFBus.Tests
{
    [TestClass]
    public class BigPropertiesUtils_Tests
    {
        [TestMethod]
        public void BigPropertiesUtils_Tests_Nominal()
        {
            var id = Guid.NewGuid().ToString();

            var propertyInfoSerialized = StorePropertyInBlobUtil.StoreDataInBlob(new OneDTO() { AProperty = id}).Result;

            var propertyInfoDeserialized = StorePropertyInBlobUtil.LoadDataFromBlob<OneDTO>(propertyInfoSerialized).Result;

            var deleted = StorePropertyInBlobUtil.DeleteBlob(propertyInfoSerialized).Result;

            Assert.IsTrue(propertyInfoDeserialized.AProperty == id);
            Assert.IsTrue(deleted);
        }

        public class OneDTO
        {
            public string AProperty { get; set; }
        }
    }

}
