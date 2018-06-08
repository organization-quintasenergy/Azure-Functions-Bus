using System;
using System.Threading.Tasks;
using AFBus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OrderSaga.Messages;
using PaymentService.Messages;
using ShippingService.Messages;

namespace OrderSaga.Tests
{
    [TestClass]
    public class OrderSaga_Tests
    {
        [TestMethod]
        public void OrderSaga_CartItemAdded_SagaIsCreated()
        {
            var saga = new OrderSaga.Sagas.OrderSaga();

            var busMock = new Mock<IBus>();

            busMock.Setup<ISerializeMessages>(m => m.Serializer).Returns(new JSONSerializer());

            saga.HandleAsync(busMock.Object, new CartItemAdded() { ProductName = "any product", UserName = "username" }, null).Wait();

            Assert.IsTrue(saga.Data != null);
        }

        [TestMethod]
        public void OrderSaga_ProcessOrder_TwoMessagesAreSent()
        {
            var saga = new OrderSaga.Sagas.OrderSaga();

            var busMock = new Mock<IBus>();

            busMock.Setup<ISerializeMessages>(m => m.Serializer).Returns(new JSONSerializer());

            busMock.Setup<Task>(m => m.SendAsync(It.IsAny<ShipOrder>(), It.IsAny<string>(),null)).Returns(Task.CompletedTask);
            busMock.Setup<Task>(m => m.SendAsync(It.IsAny<PayOrder>(), It.IsAny<string>(), null)).Returns(Task.CompletedTask);

            saga.HandleAsync(busMock.Object, new ProcessOrder() {UserName = "username" }, null).Wait();

            busMock.Verify(foo => foo.SendAsync(It.IsAny<ShipOrder>(), It.IsAny<string>(), null), Times.Once());
            busMock.Verify(foo => foo.SendAsync(It.IsAny<PayOrder>(), It.IsAny<string>(), null), Times.Once());
        }
    }
}
