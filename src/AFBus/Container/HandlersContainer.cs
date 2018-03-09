using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Globalization;
using Microsoft.Azure.WebJobs.Host;

[assembly: InternalsVisibleTo("AFBus.Tests")]
namespace AFBus
{  
    public class HandlersContainer : IHandlersContainer
    {
       
        internal Dictionary<Type, List<Type>> messageHandlersDictionary = new Dictionary<Type, List<Type>>();

        internal Dictionary<Type, List<SagaInfo>> messageToSagaDictionary = new Dictionary<Type, List<SagaInfo>>();

        private static object o = new object();
        private ISagaStoragePersistence sagaPersistence;
        private ISerializeMessages serializer = null;


        /// <summary>
        /// Scans the dlls and creates a dictionary in which each message in IFunctions is referenced to each function.
        /// </summary>
        public HandlersContainer(ISagaStoragePersistence sagaStorage = null, ISerializeMessages serializer = null)
        {
            lock (o)
            {
                this.serializer = serializer ?? new JSONSerializer();

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
                var interfacesWithCorrelation = s.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleWithCorrelation<>));
                var messageTypes = interfacesWithCorrelation.Select(i => i.GetGenericArguments()[0]).ToList();                
                sagaInfo.MessagesThatAreCorrelatedByTheSaga = interfacesWithCorrelation.
                                                                    Select(
                                                                           i => new MessageToMethod()
                                                                           {
                                                                               Message = i.GetGenericArguments()[0],
                                                                               HandlingMethod = i.GetMethods()[0],
                                                                               CorrelatingMethod = i.GetMethods()[1]                                                                               
                                                                           }).ToList();

                //messages starting sagas
                var interfacesStartingSagas = s.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleStartingSaga<>));
                var startingMessageTypes = interfacesStartingSagas.Select(i => i.GetGenericArguments()[0]);
                sagaInfo.MessagesThatActivateTheSaga = interfacesStartingSagas.
                                                                    Select(
                                                                           i => new MessageToMethod()
                                                                           {
                                                                               Message = i.GetGenericArguments()[0],
                                                                               HandlingMethod = i.GetMethods()[0],
                                                                               CorrelatingMethod = null
                                                                           }).ToList();

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
        public async Task HandleAsync<T>(T message, TraceWriter log) where T : class
        {

            if (!messageHandlersDictionary.ContainsKey(message.GetType()) && !messageToSagaDictionary.ContainsKey(message.GetType()))
                throw new Exception("Handler not found for this message.");

            await InvokeStatelessHandlers(message, log);

            await InvokeSagaHandlers(message, log);
           

        }

        private async Task InvokeSagaHandlers<T>(T message, TraceWriter log) where T : class
        {
            //The message can not be executed in a Saga
            if (!messageToSagaDictionary.ContainsKey(message.GetType()))
                return;

            foreach (var sagaInfo in messageToSagaDictionary[message.GetType()])
            {
                var instantiated = false;
                var saga = Activator.CreateInstance(sagaInfo.SagaType);
                dynamic sagaDynamic = saga;

                var sagaMessageToMethod = sagaInfo.MessagesThatAreCorrelatedByTheSaga.FirstOrDefault(m => m.Message == message.GetType());

                //try to load saga from repository
                if (sagaMessageToMethod!=null)
                {                        

                    object[] lookForInstanceParametersArray = new object[] { message };
                    dynamic sagaData = await (Task<SagaData>)sagaMessageToMethod.CorrelatingMethod.Invoke(saga, lookForInstanceParametersArray);

                    if (sagaData != null)
                    {
                        sagaDynamic.Data = sagaData;                      

                        object[] parametersArray = new object[] { new Bus(serializer, new AzureStorageQueueSendTransport(serializer)), message, log };

                        await ((Task)sagaMessageToMethod.HandlingMethod.Invoke(saga, parametersArray)).ConfigureAwait(false);

                        await sagaPersistence.Update(sagaDynamic.Data);

                        instantiated = true;
                    }
                }


                sagaMessageToMethod = sagaInfo.MessagesThatActivateTheSaga.FirstOrDefault(m => m.Message == message.GetType());
                //if not => create
                if (!instantiated && sagaMessageToMethod!=null)
                {                            
                    
                    object[] parametersArray = new object[] { new Bus(serializer, new AzureStorageQueueSendTransport(serializer)), message, log };
                   
                    await ((Task)sagaMessageToMethod.HandlingMethod.Invoke(saga, parametersArray)).ConfigureAwait(false);                    
                                       
                    await sagaPersistence.Insert(sagaDynamic.Data);

                    instantiated = true;
                }

                if (!instantiated)
                    log?.Info("Saga not found for message "+serializer.Serialize(message));
                
            }
        }

        private async Task InvokeStatelessHandlers<T>(T message, TraceWriter log) where T : class
        {
            //The message can not be executed in a stateless handler
            if (!messageHandlersDictionary.ContainsKey(message.GetType()))
                return;

            var handlerTypeList = messageHandlersDictionary[message.GetType()];

            foreach (var t in handlerTypeList)
            {
                var handler = Activator.CreateInstance(t);
                
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
        public async Task HandleAsync(string serializedMessage, TraceWriter log)
        {            

            var deserializedMessage = serializer.Deserialize(serializedMessage);

            await HandleAsync(deserializedMessage,log);
            
        }
    }
}
