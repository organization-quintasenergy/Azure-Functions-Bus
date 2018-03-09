![buildstatus](https://quintasenergyvsts.visualstudio.com/_apis/public/build/definitions/4df6b67f-0048-4b1c-b762-43d477416731/1/badge)

# Azure Functions Bus
Azure Functions Bus is a simple framework that creates a message bus on top of the Azure Functions infrastructure. That way you can create a distributed system using a serverless technology on top of the Azure Storage.

## Advantajes comparing to the normal use Azure Functions
* You can gather different functions into one host.
* You can have just one queue per service.
* You can have sagas.

## First steps
* Grab the nuget package.
* Define the connection string in the appconfig
```xml
    <configSections>
        <sectionGroup name="applicationSettings" type="System.Configuration.ApplicationSettingsGroup, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089">
            <section name="AFBus.Properties.Settings" type="System.Configuration.ClientSettingsSection, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false" />
        </sectionGroup>
    </configSections>
    <applicationSettings>
        <AFBus.Properties.Settings>
            <setting name="StorageConnectionString" serializeAs="String">
                <value>UseDevelopmentStorage=true</value>
            </setting>
        </AFBus.Properties.Settings>
    </applicationSettings>
```

## Recommended solution structure (Please see the SolutionExample folder)
The system gets divided in different parts:
* Sagas (Folder)
  * SagaA.Host (Project)
    * Sagas
  * SagaA.Message (Project)
* Services(Folder)
  * ServiceA.Host (Project)
    * Handlers
  * ServicesA.Messages (Project)


### Messages
Just POCO classes that are shared between different services to communicate to each other.

### Host
Project with the functions that are the entrance of the service. It must create the handler container.

```cs
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
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            await container.HandleAsync(myQueueItem, new AFTraceWriter(log));
        }
    }
```

#### Stateless handlers

Defining a stateless handler is just implementing the IHandle<MessageType> interface in a class. For instance:
```cs
    public class ShipOrderHandler : IHandle<ShipOrder>
    {
        public async Task HandleAsync(IBus bus, ShipOrder message, ITraceWriter Log)
        {
            Log.Info("order shipped");
                        
            await bus.SendAsync(new ShipOrderResponse() { UserName = "pablo" }, "ordersaga");

            
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
