﻿using AFBus;
using Microsoft.Azure.WebJobs.Host;
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
    public class OrderSaga : Saga<OrderSagaData>, IHandleCommandStartingSaga<CartItemAdded>, IHandleCommandWithCorrelation<CartItemAdded>, IHandleCommandWithCorrelation<ProcessOrder>, IHandleCommandWithCorrelation<ShipOrderResponse>, IHandleCommandWithCorrelation<PayOrderResponse>
    {
        const string PARTITION_KEY = "OrderSaga";
        const string SERVICE_NAME = "ordersaga";
        const string UI_SERVICE_NAME = "uiexample";

        public async Task HandleCommandAsync(IBus bus, CartItemAdded message, TraceWriter log)
        {
            
            var productsList = new List<string>();

            if (Data.PartitionKey == null)
            {
                this.Data.PartitionKey = PARTITION_KEY;
                this.Data.RowKey = message.UserName;
                
                this.Data.UserName = message.UserName;
            }
            else
            {                
                productsList = await this.LoadBlobPropertyAsync<List<string>>(Data.Products);
            }

            productsList.Add(message.ProductName);

            Data.Products=await this.WriteBlobPropertyAsync(productsList);


            
        }

        public async Task HandleCommandAsync(IBus bus, ProcessOrder message, TraceWriter log)
        {
            await bus.SendAsync(new ShipOrder() { UserName = this.Data.RowKey, ReplyTo = SERVICE_NAME }, "shippingservice");
             
            await bus.SendAsync(new PayOrder() { UserName = this.Data.RowKey, ReplyTo = SERVICE_NAME }, "paymentservice");
        }

        public async Task HandleCommandAsync(IBus bus, ShipOrderResponse message, TraceWriter log)
        {
            this.Data.Shipped = true;
            await EndOfOrder(bus, log);

        }

        

        public async Task HandleCommandAsync(IBus bus, PayOrderResponse message, TraceWriter log)
        {
            this.Data.Payed = true;

            await EndOfOrder(bus,log);

        }

        private async Task EndOfOrder(IBus bus, TraceWriter Log)
        {
            if (Data.Shipped && Data.Payed)
            {
                Log.Info("Process finished");

                await bus.SendAsync(new OrderFinished { UserName = Data.UserName }, UI_SERVICE_NAME);

                await this.DeleteSagaAsync();
            }
        } 

        public async Task<SagaData> LookForInstanceAsync(CartItemAdded message)
        {
            return await SagaPersistence.GetSagaDataAsync<OrderSagaData>(PARTITION_KEY, message.UserName);
        }

        public async Task<SagaData> LookForInstanceAsync(ProcessOrder message)
        {
            return await SagaPersistence.GetSagaDataAsync<OrderSagaData>(PARTITION_KEY, message.UserName);
        }

        public async Task<SagaData> LookForInstanceAsync(ShipOrderResponse message)
        {
            return await SagaPersistence.GetSagaDataAsync<OrderSagaData>(PARTITION_KEY, message.UserName);
        }

        public async Task<SagaData> LookForInstanceAsync(PayOrderResponse message)
        {
            return await SagaPersistence.GetSagaDataAsync<OrderSagaData>(PARTITION_KEY, message.UserName);
        }
    }
}
