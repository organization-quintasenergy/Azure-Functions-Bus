using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Globalization;

[assembly: InternalsVisibleTo("AFBus.Tests")]
namespace AFBus
{  
    public class HandlersContainer : IHandlersContainer
    {
       
        internal Dictionary<Type, List<Type>> messageHandlersDictionary = new Dictionary<Type, List<Type>>();

        internal Dictionary<Type, List<SagaInfo>> messageToSagaDictionary = new Dictionary<Type, List<SagaInfo>>();


        private static object o = new object();


        /// <summary>
        /// Scans the dlls and creates a dictionary in which each message in IFunctions is referenced to each function.
        /// </summary>
        public HandlersContainer()
        {
            lock (o)
            {

                var assemblies = new List<Assembly>();

                assemblies.Add(Assembly.GetCallingAssembly());
                assemblies.AddRange(Assembly.GetCallingAssembly().GetReferencedAssemblies().Select(a => Assembly.Load(a.FullName)));

                var types = assemblies.SelectMany(a => a.GetTypes());

                LookForSagas(types);

                LookForHandlers(types);                

            }

        }

        private void LookForSagas(IEnumerable<Type> types)
        {
            var sagaTypes = types.Where(t => t.BaseType != null && t.BaseType.IsGenericType &&
                t.BaseType.GetGenericTypeDefinition() == typeof(Saga<>));

            foreach(var s in sagaTypes)
            {
                var sagaInfo = new SagaInfo();

                sagaInfo.SagaType = s;

                //messages with correlation
                var messagesWithCorrelation = s.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleWithCorrelation<,>));
                var messageTypes = messagesWithCorrelation.Select(i => i.GetGenericArguments()[0]);
                sagaInfo.MessagesThatAreHandledByTheSaga = messageTypes.ToList();

                //messages starting sagas
                var messagesStartingSagas = s.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleStartingSaga<>));
                messageTypes = messagesStartingSagas.Select(i => i.GetGenericArguments()[0]);
                sagaInfo.MessagesThatActivatesTheSaga = messageTypes.ToList();

                messageTypes.ToList().AddRange(messagesStartingSagas.Select(i => i.GetGenericArguments()[0]));
                
                foreach(var messageType in messageTypes)
                {
                    if (!messageToSagaDictionary.ContainsKey(messageType))
                    {
                        var sagaInfoList = new List<SagaInfo>();
                        sagaInfoList.Add(sagaInfo);
                        messageToSagaDictionary.Add(messageType, sagaInfoList);

                    }
                    else
                    {
                        var sagaInfoList = messageToSagaDictionary[messageType];
                        sagaInfoList.Add(sagaInfo);
                       
                    }

                }

            }


        }

        private void LookForHandlers(IEnumerable<Type> types)
        {
            var ifunctionTypes = types.Where(x => x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IFunction<>)));


            foreach (var t in ifunctionTypes)
            {
                var interfaceType = t.GetInterfaces()[0];
                var messageType = interfaceType.GetGenericArguments()[0];

                List<Type> handlerTypeList;

                if (!messageHandlersDictionary.ContainsKey(messageType))
                {
                    handlerTypeList = new List<Type>();
                    handlerTypeList.Add(t);
                    messageHandlersDictionary.Add(messageType, handlerTypeList);

                }
                else
                {
                    handlerTypeList = messageHandlersDictionary[messageType];
                    handlerTypeList.Add(t);
                    
                }


            }
        }

        /// <summary>
        /// Calls each function referenced by each message in the dictionary.
        /// </summary>
        public Task InvokeAsync<T>(T message, ITraceWriter log) where T : class
        {

            if (!messageHandlersDictionary.ContainsKey(message.GetType()) && !messageToSagaDictionary.ContainsKey(message.GetType()))
                throw new Exception("Handler not found for this message.");

            InvokeStatelessHandlers(message, log);

            InvokeSagaHandlers(message, log);

            return Task.CompletedTask;

        }

        private void InvokeSagaHandlers<T>(T message, ITraceWriter log) where T : class
        {
            //The message can not be executed in a Saga
            if (!messageToSagaDictionary.ContainsKey(message.GetType()))
                return;

            foreach (var sagaInfo in messageToSagaDictionary[message.GetType()])
            {
                var instantiated = false;
                var saga = Activator.CreateInstance(sagaInfo.SagaType);

                //try to load saga from repository
                if(sagaInfo.MessagesThatAreHandledByTheSaga.Any(m=>m.GetType()==typeof(T)))
                {
                    var sagaHandler = sagaInfo.MessagesThatAreHandledByTheSaga.First(m => m.GetType() == typeof(T));
                    
                }


                //if not => create
                if (sagaInfo.MessagesThatActivatesTheSaga.Any(m => m.GetType() == typeof(T)))
                {
                    var sagaHandler = sagaInfo.MessagesThatActivatesTheSaga.First(m => m.GetType() == typeof(T));
                    var methodsToInvoke = sagaHandler.GetMethods().Where(m => m.GetParameters().Any(p => p.ParameterType == message.GetType())).ToList();

                    ISerializeMessages serializer = new JSONSerializer();
                    object[] parametersArray = new object[] { new Bus(serializer, new AzureStorageQueueSendTransport(serializer)), message, log };
                    methodsToInvoke.ForEach(m => m.Invoke(saga, parametersArray));


                }

                
            }
        }

        private void InvokeStatelessHandlers<T>(T message, ITraceWriter log) where T : class
        {
            //The message can not be executed in a stateless handler
            if (!messageHandlersDictionary.ContainsKey(message.GetType()))
                return;

            var handlerTypeList = messageHandlersDictionary[message.GetType()];

            foreach (var t in handlerTypeList)
            {
                var handler = Activator.CreateInstance(t);
                ISerializeMessages serializer = new JSONSerializer();
                object[] parametersArray = new object[] { new Bus(serializer, new AzureStorageQueueSendTransport(serializer)), message, log };

                var methodsToInvoke = t.GetMethods().Where(m => m.GetParameters().Any(p => p.ParameterType == message.GetType()));

                methodsToInvoke.ToList().ForEach(m => m.Invoke(handler, parametersArray));
            }
        }

        /// <summary>
        /// Deserializes and invokes the handlers.
        /// </summary>
        /// <param name="serializedMessage"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public async Task InvokeAsync(string serializedMessage, ITraceWriter log)
        {
            var serializer = new JSONSerializer();

            var deserializedMessage = serializer.Deserialize(serializedMessage);

            await InvokeAsync(deserializedMessage,log);
            
        }
    }
}
