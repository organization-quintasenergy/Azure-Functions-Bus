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

        internal static Dictionary<Type, DependencyInfo> dependencies = new Dictionary<Type, DependencyInfo>();

        private static object o = new object();
        private ISagaStoragePersistence sagaPersistence;
        private ISagaLocker sagaLocker;
        private ISerializeMessages serializer = null;
        private bool lockSaga = false;
        

        /// <summary>
        /// Scans the dlls and creates a dictionary in which each message in IFunctions is referenced to each function.
        /// </summary>
        public HandlersContainer(bool? lockSaga = null)
        {
            lock (o)
            {
                this.lockSaga = lockSaga ?? SettingsUtil.GetSettings<bool>(SETTINGS.LOCKSAGAS);

                AddDependency<ISerializeMessages, JSONSerializer>();
                AddDependency<ISagaLocker, SagaAzureStorageLocker>();
                
                this.serializer = SolveDependency<ISerializeMessages>();
                this.sagaLocker = SolveDependency<ISagaLocker>();
                AddDependency<ISagaStoragePersistence, SagaAzureStoragePersistence>(this.sagaLocker as ISagaLocker, this.lockSaga);

                sagaPersistence = SolveDependency<ISagaStoragePersistence>();

                var assemblies = new List<Assembly>();

                assemblies.Add(Assembly.GetCallingAssembly());
                assemblies.AddRange(Assembly.GetCallingAssembly().GetReferencedAssemblies().Select(a => Assembly.Load(a.FullName)));

                var types = assemblies.SelectMany(a => a.GetTypes());

                LookForSagas(types);

                LookForHandlers(types);

                this.sagaLocker.CreateLocksContainer();
                sagaPersistence.CreateSagaPersistenceTable().Wait();

            }

        }

        public static void AddDependency<I, C>(params object[] arguments)
        {
            if(!dependencies.ContainsKey(typeof(I)))
                dependencies.Add(typeof(I), new DependencyInfo() { Interface = typeof(I), ConcreteType = typeof(C), args = arguments });
            else
                dependencies[typeof(I)]= new DependencyInfo() { Interface = typeof(I), ConcreteType = typeof(C), args = arguments };
        }

        public static void AddDependencyWithInstance<I>(I objectInstance)
        {
            if (!dependencies.ContainsKey(typeof(I)))
                dependencies.Add(typeof(I), new DependencyInfo() { Interface = typeof(I), ConcreteType = typeof(I), instance = objectInstance });
            else
                dependencies[typeof(I)] = new DependencyInfo() { Interface = typeof(I), ConcreteType = typeof(I), instance = objectInstance };
        }

        public static I SolveDependency<I>()
        {
            if (!dependencies.ContainsKey(typeof(I)))
                throw new Exception("No depedency can be solved for "+ typeof(I).Name+". Please add to the depedency graph using AddDepedency");

            var dependencyInfo = dependencies[typeof(I)];

            if (dependencyInfo.instance == null)
                return (I)Activator.CreateInstance(dependencyInfo.ConcreteType, dependencyInfo.args);
            else
                return (I)dependencyInfo.instance;
        }

        private object SolveDependencyAsObject(Type parameterType)
        {
            if (!dependencies.ContainsKey(parameterType))
                throw new Exception("No depedency can be solved for " + parameterType.Name + ". Please add to the depedency graph using AddDepedency");

            var dependencyInfo = dependencies[parameterType];

            if (dependencyInfo.instance == null)
                return (object)Activator.CreateInstance(dependencyInfo.ConcreteType, dependencyInfo.args);
            else
                return (object)dependencyInfo.instance;
        }

        internal static void ClearDependencies()
        {
            dependencies.Clear();
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
        internal async Task HandleAsync<T>(T message, AFBusMessageContext messageContext, TraceWriter log) where T : class
        {

            if (!messageHandlersDictionary.ContainsKey(message.GetType()) && !messageToSagaDictionary.ContainsKey(message.GetType()))
                throw new Exception("Handler not found for this message." + serializer.Serialize(message));

            await InvokeStatelessHandlers(message, messageContext, log).ConfigureAwait(false);

            await InvokeSagaHandlers(message, messageContext, log).ConfigureAwait(false);
          
        }

        /// <summary>
        /// Deserializes and invokes the handlers.
        /// </summary>
        /// <param name="serializedMessage"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public async Task HandleAsync(string serializedMessage, TraceWriter log)
        {

            var deserializedMessageWrapper = serializer.Deserialize(serializedMessage) as AFBusMessageEnvelope;

            var deserializedMessage = serializer.Deserialize(deserializedMessageWrapper.Body);
            
            await HandleAsync(deserializedMessage, deserializedMessageWrapper.Context, log).ConfigureAwait(false);

        }

        /// <summary>
        /// Calls each function referenced by each message in the dictionary.
        /// </summary>
        public async Task HandleAsync<T>(T message, TraceWriter log) where T : class
        {
            if(message.GetType()==typeof(AFBusMessageEnvelope))
            {
                throw new Exception("AFBusMessageEnvelope type not permited");
            }

            if (!messageHandlersDictionary.ContainsKey(message.GetType()) && !messageToSagaDictionary.ContainsKey(message.GetType()))
                throw new Exception("Handler not found for this message." + serializer.Serialize(message));

            var messageContext = new AFBusMessageContext()
            {
                MessageID = Guid.NewGuid(),
                TransactionID = Guid.NewGuid()
            };

            await InvokeStatelessHandlers(message, messageContext, log).ConfigureAwait(false);

            await InvokeSagaHandlers(message, messageContext, log).ConfigureAwait(false);

        }

        private async Task InvokeSagaHandlers<T>(T message, AFBusMessageContext messageContext, TraceWriter log) where T : class
        {
            //The message can not be executed in a Saga
            if (!messageToSagaDictionary.ContainsKey(message.GetType()))
                return;

            foreach (var sagaInfo in messageToSagaDictionary[message.GetType()])
            {
                var instantiated = false;
                var saga = CreateInstance(sagaInfo.SagaType);//Activator.CreateInstance(sagaInfo.SagaType);
                dynamic sagaDynamic = saga;

                var sagaMessageToMethod = sagaInfo.MessagesThatAreCorrelatedByTheSaga.FirstOrDefault(m => m.Message == message.GetType());

                //try to load saga from repository
                if (sagaMessageToMethod!=null)
                {                        

                    object[] lookForInstanceParametersArray = new object[] { message };
                    sagaDynamic.SagaPersistence = new SagaAzureStoragePersistence(new SagaAzureStorageLocker(), this.lockSaga);  
                    dynamic sagaData = await ((Task<SagaData>)sagaMessageToMethod.CorrelatingMethod.Invoke(saga, lookForInstanceParametersArray)).ConfigureAwait(false);

                    if (sagaData != null)
                    {
                        sagaDynamic.Data = sagaData;

                        var bus = new Bus(serializer, new AzureStorageQueueSendTransport(serializer));
                        bus.Context = messageContext;

                        object[] parametersArray = new object[] {bus , message, log };

                        await ((Task)sagaMessageToMethod.HandlingMethod.Invoke(saga, parametersArray)).ConfigureAwait(false);

                        await sagaPersistence.Update(sagaDynamic.Data).ConfigureAwait(false);

                        instantiated = true;
                    }
                }


                sagaMessageToMethod = sagaInfo.MessagesThatActivateTheSaga.FirstOrDefault(m => m.Message == message.GetType());
                //if not => create
                if (!instantiated && sagaMessageToMethod!=null)
                {
                    var bus = new Bus(serializer, new AzureStorageQueueSendTransport(serializer));
                    bus.Context = messageContext;

                    object[] parametersArray = new object[] { bus, message, log };
                   
                    await ((Task)sagaMessageToMethod.HandlingMethod.Invoke(saga, parametersArray)).ConfigureAwait(false);                    
                                       
                    await sagaPersistence.Insert(sagaDynamic.Data).ConfigureAwait(false);

                    instantiated = true;
                }

                if (!instantiated)
                    log?.Info("Saga not found for message "+serializer.Serialize(message));
                
            }
        }

        private async Task InvokeStatelessHandlers<T>(T message, AFBusMessageContext messageContext, TraceWriter log) where T : class
        {
            //The message can not be executed in a stateless handler
            if (!messageHandlersDictionary.ContainsKey(message.GetType()))
                return;

            var handlerTypeList = messageHandlersDictionary[message.GetType()];

            foreach (var t in handlerTypeList)
            {
                var handler = CreateInstance(t);
                
                object[] parametersArray = new object[] { new Bus(serializer, new AzureStorageQueueSendTransport(serializer)), message, log };

                var methodsToInvoke = t.GetMethods().Where(m => m.GetParameters().Any(p => p.ParameterType == message.GetType()));
                               
                foreach (var m in methodsToInvoke)
                {
                    await ((Task)m.Invoke(handler, parametersArray)).ConfigureAwait(false);
                }

            }
        }

        private object CreateInstance(Type instanceType)
        {
            var constructors = instanceType.GetConstructors();

            var parameters = constructors.First().GetParameters();

            object[] args = new object[parameters.Count()];
            
            for(int i=0;i<args.Count();i++)
            {
                args[i] = SolveDependencyAsObject(parameters[i].ParameterType);
            }
            
            return Activator.CreateInstance(instanceType, args);
        }


    }
}
