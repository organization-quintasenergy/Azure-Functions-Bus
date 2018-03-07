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

        private ISagaStoragePersistence sagaPersistence;


        /// <summary>
        /// Scans the dlls and creates a dictionary in which each message in IFunctions is referenced to each function.
        /// </summary>
        public HandlersContainer(ISagaStoragePersistence sagaStorage = null)
        {
            lock (o)
            {
                sagaPersistence = sagaStorage ?? new SagaAzureStoragePersistence();
                
                var assemblies = new List<Assembly>();

                assemblies.Add(Assembly.GetCallingAssembly());
                assemblies.AddRange(Assembly.GetCallingAssembly().GetReferencedAssemblies().Select(a => Assembly.Load(a.FullName)));

                var types = assemblies.SelectMany(a => a.GetTypes());

                LookForSagas(types);

                LookForHandlers(types);

                sagaPersistence.CreateSagaPersistenceTable().Wait();

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
                var messagesWithCorrelation = s.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleWithCorrelation<>));
                var messageTypes = messagesWithCorrelation.Select(i => i.GetGenericArguments()[0]).ToList();
                sagaInfo.MessagesThatAreHandledByTheSaga = messageTypes.ToList();

                //messages starting sagas
                var messagesStartingSagas = s.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleStartingSaga<>));
                var startingMessageTypes = messagesStartingSagas.Select(i => i.GetGenericArguments()[0]);
                sagaInfo.MessagesThatActivatesTheSaga = startingMessageTypes.ToList();

                messageTypes.AddRange(startingMessageTypes);
                
                foreach(var messageType in messageTypes.Distinct())
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
            var ifunctionTypes = types.Where(x => x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandle<>)));


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
        public async Task HandleAsync<T>(T message, ITraceWriter log) where T : class
        {

            if (!messageHandlersDictionary.ContainsKey(message.GetType()) && !messageToSagaDictionary.ContainsKey(message.GetType()))
                throw new Exception("Handler not found for this message.");

            await InvokeStatelessHandlers(message, log);

            await InvokeSagaHandlers(message, log);
           

        }

        private async Task InvokeSagaHandlers<T>(T message, ITraceWriter log) where T : class
        {
            //The message can not be executed in a Saga
            if (!messageToSagaDictionary.ContainsKey(message.GetType()))
                return;

            foreach (var sagaInfo in messageToSagaDictionary[message.GetType()])
            {
                var instantiated = false;
                var saga = Activator.CreateInstance(sagaInfo.SagaType);
                dynamic sagaDynamic = saga;

                //try to load saga from repository
                if (sagaInfo.MessagesThatAreHandledByTheSaga.Any(m=>m == typeof(T)))
                {
                    var sagaHandler = sagaInfo.MessagesThatAreHandledByTheSaga.First(m => m == typeof(T));

                    var lookForInstanceMethod = sagaInfo.SagaType.GetRuntimeMethods().First(m =>  m.GetParameters().Any(p => p.ParameterType == message.GetType()) && m.GetParameters().Any(p => p.ParameterType == typeof(ISagaStoragePersistence)));

                    object[] lookForInstanceParametersArray = new object[] { sagaPersistence, message };
                    dynamic sagaData = await (Task<SagaData>) lookForInstanceMethod.Invoke(saga, lookForInstanceParametersArray);

                    if (sagaData != null)
                    {
                        sagaDynamic.Data = sagaData;

                        var methodToInvoke = sagaInfo.SagaType.GetRuntimeMethods().First(m => !m.Name.Contains("IHandleStartingSaga") &&  !m.GetParameters().Any(p => p.ParameterType == typeof(ISagaStoragePersistence)) && m.GetParameters().Any(p => p.ParameterType == message.GetType()));

                        ISerializeMessages serializer = new JSONSerializer();
                        object[] parametersArray = new object[] { new Bus(serializer, new AzureStorageQueueSendTransport(serializer)), message, log };

                        await ((Task)methodToInvoke.Invoke(saga, parametersArray));

                        await sagaPersistence.Update(sagaDynamic.Data);

                        instantiated = true;
                    }
                }


                //if not => create
                if (!instantiated && sagaInfo.MessagesThatActivatesTheSaga.Any(m => m == typeof(T)))
                {
                    var messageType = sagaInfo.MessagesThatActivatesTheSaga.First(m => m == typeof(T));
                    var methodsToInvoke = sagaInfo.SagaType.GetRuntimeMethods().Where(m => m.Name.Contains("IHandleStartingSaga") && m.GetParameters().Any(p => p.ParameterType == message.GetType())).ToList();

                    ISerializeMessages serializer = new JSONSerializer();
                    object[] parametersArray = new object[] { new Bus(serializer, new AzureStorageQueueSendTransport(serializer)), message, log };

                    foreach(var m in methodsToInvoke)
                    {
                        await ((Task) m.Invoke(saga, parametersArray));
                    }
                                       
                    await sagaPersistence.Insert(sagaDynamic.Data);
                    
                }

                
            }
        }

        private async Task InvokeStatelessHandlers<T>(T message, ITraceWriter log) where T : class
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
                               
                foreach (var m in methodsToInvoke)
                {
                    await ((Task)m.Invoke(handler, parametersArray));
                }

            }
        }

        /// <summary>
        /// Deserializes and invokes the handlers.
        /// </summary>
        /// <param name="serializedMessage"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public async Task HandleAsync(string serializedMessage, ITraceWriter log)
        {
            var serializer = new JSONSerializer();

            var deserializedMessage = serializer.Deserialize(serializedMessage);

            await HandleAsync(deserializedMessage,log);
            
        }
    }
}
