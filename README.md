# Azure Functions Bus
Azure Functions Bus is a simple framework that creates a message bus on top of the Azure Functions infrastructure. That way you can create a distributed system with a serverless technology.

## Advantajes comparing to the normal use Azure Functions
* You can gather different functions into one host.
* You can have just one queue per service.
* You can have sagas.

## Recommended project structure
The system gets divided in different parts:

### Messages
Just POCO classes that are shared between different components.

### Host
Project with the functions that are the entrance of the service. It must create the function container.

```cs
private static IFunctionContainer container = new FunctionContainer();
```

This container looks for every IFunction<> implementation.


#### Functions
Each message gets passed to its function when the invoke method of the container is called.

```cs
var command = JsonConvert.DeserializeObject(message, new JsonSerializerSettings()
{
    TypeNameHandling = TypeNameHandling.Objects,
    TypeNameAssemblyFormat=System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
});

var messagetype = command.GetType();

await container.InvokeAsync(command, new AFTraceWritter(log));
```


