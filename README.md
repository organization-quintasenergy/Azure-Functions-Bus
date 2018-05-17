
# Azure Functions Bus
Azure Functions Bus is a simple framework that creates a message bus on top of the Azure Functions infrastructure. That way you can create a distributed system using a serverless technology on top of the Azure Storage.

## Advantajes comparing to the normal use Azure Functions
* You can gather different functions into one host.
* You can have just one queue per service.
* You can have sagas.

## First steps
* Grab the [nuget package](https://www.nuget.org/packages/AFBusCore/) for AFBus.
* Define the connection string in the host.json 
```json
"Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true"   
  }
```

or in the appsettings.json or local.settings.json if you want to modify the tests 

```json
{
  "AzureWebJobsStorage": "UseDevelopmentStorage=true",
  "AzureWebJobsDashboard": "UseDevelopmentStorage=true",
  "LockSagas": "True"
}
```

## Recommended solution structure (Please see the SolutionExample folder)
The system gets divided in different parts:
* Sagas (Folder)
  * SagaA.Host (Project)
    * Sagas
  * SagaA.Messages (Project)
  * SagaA.Tests
* Services(Folder)
  * ServiceA.Host (Project)
    * Handlers
  * ServiceA.Messages (Project)
  * ServiceA.Tests


### Messages
Just POCO classes that are shared between different services to communicate to each other.

### Host
Project with the functions that are the entrance of the service. It must create the handler container.

```cs
//here the dlls are scanned looking for handlers
private static HandlersContainer container = new HandlersContainer();
```

This container looks for every IHandle<> implementation.
Each message gets passed to its handlers when the handle method of the container is called.

```cs
    public static class ShippingService
    {
        static HandlersContainer container = new HandlersContainer();

        [FunctionName("ShippingServiceEndpointFunction")]
        public static async Task Run([QueueTrigger("shippingservice", Connection = "")]string myQueueItem, TraceWriter log)
        {            
            //Calls to every handler that receives that message
            await container.HandleAsync(myQueueItem, new AFTraceWriter(log));
            
        }
    }
```

#### Stateless handlers
Defining a stateless handler is just implementing the IHandle<MessageType> interface in a class. For instance:
```cs
    public class ShipOrderHandler : IHandle<ShipOrder>
    {
        IShippingRepository rep;

        public ShipOrderHandler(IShippingRepository rep)
        {
            this.rep = rep;
        }

        public async Task HandleAsync(IBus bus, ShipOrder message, TraceWriter Log)
        {
            Log.Info("order shipped");

            rep.AddOrderShipped(new OrderShipped { User = message.UserName });

            await bus.SendAsync(new ShipOrderResponse() { UserName = message.UserName }, message.ReplyTo);

            
        }
    }
```


#### Sagas
Sagas are stateful components that orchestrates differents messages. In a saga you must define three parts:
* SagaData: the data that will be stored between messages.
* IHandleStartingSaga: handlers that creates sagas (the first message received by the saga).
* IHandleWithCorrelation: handlers that correlates in the saga. You need to implement two methods, the one with the logic and the one with the correlation logic.

```cs
    public class SimpleTestSaga : Saga<SimpleTestSagaData>, IHandleStartingSaga<SimpleSagaStartingMessage>,  IHandleWithCorrelation<SimpleSagaIntermediateMessage>, IHandleWithCorrelation<SimpleSagaTerminatingMessage>
    {
        private const string PARTITION_KEY = "SimpleTestSaga";

        
        public Task HandleAsync(IBus bus, SimpleSagaStartingMessage input, ITraceWriter Log)
        {           

            this.Data.PartitionKey = PARTITION_KEY;
            this.Data.RowKey = input.Id.ToString();
            this.Data.Counter++;

            return Task.CompletedTask;
        }

        public Task HandleAsync(IBus bus, SimpleSagaIntermediateMessage input, ITraceWriter Log)
        {
            this.Data.Counter++;
            return Task.CompletedTask;
        }

        public async Task HandleAsync(IBus bus, SimpleSagaTerminatingMessage message, ITraceWriter Log)
        {
            await this.DeleteSaga();
        }

        public async Task<SagaData> LookForInstance(SimpleSagaIntermediateMessage message)
        {
            var sagaData =  await sagaPersistence.GetSagaData<SimpleTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }

        public async Task<SagaData> LookForInstance(SimpleSagaTerminatingMessage message)
        {
            var sagaData = await sagaPersistence.GetSagaData<SimpleTestSagaData>(PARTITION_KEY, message.Id.ToString());

            return sagaData;
        }
    }
```
#### How to send a message out of a handler
Messages outside the AFBus framework can be launched using the SendOnlyBus class.

```cs
SendOnlyBus.SendAsync(message, SERVICENAME).Wait();
```

Messages inside the framework can be launched using the bus object parameter in the handler method.

```cs
bus.SendAsync(new PayOrderResponse() { UserName = "pablo"}, message.ReplyTo);
```


#### Dependency Injection
Dependency injection can be set using special methods for it

Here a dependency is set in the static constructor
```cs
public static class ShippingService
{
    static HandlersContainer container = new HandlersContainer();

    static ShippingService()
    {
        HandlersContainer.AddDependency<IShippingRepository, InMemoryShippingRepository>();
    }


    [FunctionName("ShippingServiceEndpointFunction")]
    public static async Task Run([QueueTrigger("shippingservice")]string myQueueItem, TraceWriter log)
    {
        log.Info($"C# Queue trigger function processed: {myQueueItem}");

        await container.HandleAsync(myQueueItem, log);
    }
}
```

Here the dependency is injected into the constructor
```cs
public class ShipOrderHandler : IHandle<ShipOrder>
{

    IShippingRepository rep;

    public ShipOrderHandler(IShippingRepository rep)
    {
        this.rep = rep;
    }

    public async Task HandleAsync(IBus bus, ShipOrder message, TraceWriter Log)
    {
        Log.Info("order shipped");

        rep.AddOrderShipped(new OrderShipped { User = message.UserName });

        await bus.SendAsync(new ShipOrderResponse() { UserName = message.UserName }, message.ReplyTo);

        
    }
}
```