using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.WebJobs.Host;
using System.Linq;
using AFUtils.IoC;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AFUtils.Tests")]
namespace AFUtils
{  
    public class FunctionContainer : IFunctionContainer
    {
       
        internal Dictionary<Type, Type> messageHandlersDictionary = new Dictionary<Type, Type>();

        public FunctionContainer()
        {

            var ifunctionTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                                .Where(x => x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition()==typeof(IFunction<>)));



            foreach (var t in ifunctionTypes)
            {
                var interfaceType = t.GetInterfaces()[0];
                var messageType = interfaceType.GetGenericArguments()[0];

                messageHandlersDictionary.Add(messageType, t);               

            }
                                                  
        }


        public Task InvokeAsync<T>(T message, ITraceWriter log)
        {
            var handlerType = messageHandlersDictionary[message.GetType()];

            var handler = Activator.CreateInstance(handlerType);

            object[] parametersArray = new object[] { message, log };
            var result = handlerType.GetMethods()[0].Invoke(handler, parametersArray);

            return Task.CompletedTask;

        }

        public void Dispose()
        {
            
        }


    }
}
