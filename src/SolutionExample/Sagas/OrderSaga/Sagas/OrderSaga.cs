using AFBus;
using Newtonsoft.Json;
using OrderSaga.Messages;
using PaymentService.Messages;
using ShippingService.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSaga.Sagas
{
    public class OrderSaga : Saga<OrderSagaData>, IHandleStartingSaga<CartItemAdded>, IHandleWithCorrelation<CartItemAdded>, IHandleWithCorrelation<ProcessOrder>, IHandleWithCorrelation<ShipOrderResponse>, IHandleWithCorrelation<PayOrderResponse>
    {
        const string PARTITION_KEY = "OrderSaga";

        public Task HandleAsync(IBus bus, CartItemAdded message, ITraceWriter Log)
        {
            
            var productsList = new List<string>();

            if (Data.PartitionKey == null)
            {
                this.Data.PartitionKey = PARTITION_KEY;
                this.Data.RowKey = message.UserName;
                this.Data.Products = bus.serializer.Serialize(productsList);

            }
            else
            {
                productsList = JsonConvert.DeserializeObject<List<string>>(this.Data.Products);
            }

            productsList.Add(message.ProductName);

            this.Data.Products = JsonConvert.SerializeObject(productsList);

            return Task.CompletedTask;
        }

        public async Task HandleAsync(IBus bus, ProcessOrder message, ITraceWriter Log)
        {
            await bus.SendAsync(new ShipOrder() { UserName = this.Data.RowKey }, "shippingservice");

            await bus.SendAsync(new PayOrder() { UserName = this.Data.RowKey }, "paymentservice");
        }

        public async Task HandleAsync(IBus bus, ShipOrderResponse message, ITraceWriter Log)
        {
            this.Data.Shipped = true;
            await EndOfOrder(Log);

        }

        

        public async Task HandleAsync(IBus bus, PayOrderResponse message, ITraceWriter Log)
        {
            this.Data.Payed = true;

            await EndOfOrder(Log);

        }

        private async Task EndOfOrder(ITraceWriter Log)
        {
            if (Data.Shipped && Data.Payed)
            {
                Log.Info("Process finished");
                await this.DeleteSaga();
            }
        }

        public async Task<SagaData> LookForInstance(CartItemAdded message)
        {
            return await sagaPersistence.GetSagaData<OrderSagaData>(PARTITION_KEY, message.UserName);
        }

        public async Task<SagaData> LookForInstance(ProcessOrder message)
        {
            return await sagaPersistence.GetSagaData<OrderSagaData>(PARTITION_KEY, message.UserName);
        }

        public async Task<SagaData> LookForInstance(ShipOrderResponse message)
        {
            return await sagaPersistence.GetSagaData<OrderSagaData>(PARTITION_KEY, message.UserName);
        }

        public async Task<SagaData> LookForInstance(PayOrderResponse message)
        {
            return await sagaPersistence.GetSagaData<OrderSagaData>(PARTITION_KEY, message.UserName);
        }
    }
}
