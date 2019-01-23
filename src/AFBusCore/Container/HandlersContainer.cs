using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Globalization;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;

[assembly: InternalsVisibleTo("AFBusCore.Tests")]
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
        private ISendMessages messageSender;
        private bool lockSaga = false;
        private string serviceName;
        

        /// <summary>
        /// Scans the dlls and creates a dictionary in which each message in IFunctions is referenced to each function.
        /// </summary>
        public HandlersContainer(string serviceName, bool lockSaga = true)
        {
            lock (o)
            {
                this.serviceName = serviceName;

                this.lockSaga = SettingsUtil.HasSettings(SETTINGS.LOCKSAGAS)? SettingsUtil.GetSettings<bool>(SETTINGS.LOCKSAGAS):lockSaga;

                AddDependency<ISerializeMessages, JSONSerializer>();
                AddDependency<ISagaLocker, SagaAzureStorageLocker>();
                AddDependency<ISendMessages, AzureStorageQueueSendTransport>(SolveDependency<ISerializeMessages>());
                AddDependency<IPublishEvents, AzureEventHubPublishTransport>(SolveDependency<ISerializeMessages>());

                this.serializer = SolveDependency<ISerializeMessages>();
                this.sagaLocker = SolveDependency<ISagaLocker>();
                AddDependency<ISagaStoragePersistence, SagaAzureStoragePersistence>(this.sagaLocker as ISagaLocker, this.lockSaga);

                sagaPersistence = SolveDependency<ISagaStoragePersistence>();
                messageSender = SolveDependency<ISendMessages>();

                var assemblies = new List<Assembly>();

                assemblies.Add(Assembly.GetCallingAssembly());
                assemblies.AddRange(Assembly.GetCallingAssembly().GetReferencedAssemblies().Select(a => Assembly.Load(a.FullName)));

                var types = assemblies.SelectMany(a => a.GetTypes());

                LookForSagas(types);

                LookForHandlers(types);

                this.sagaLocker.CreateLocksContainer();
                sagaPersistence.CreateSagaPersistenceTableAsync().Wait();

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
            {              
                return (I)Activator.CreateInstance(dependencyInfo.ConcreteType, dependencyInfo.args);

            }
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

                //events that correlates the saga
                var interfacesWithCorrelation = s.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleEventWithCorrelation<>));
                var messageTypes = interfacesWithCorrelation.Select(i => i.GetGenericArguments()[0]).ToList();
                sagaInfo.EventsThatCorrelatesSagas = interfacesWithCorrelation.
                                                                    Select(
                                                                           i => new MessageToMethod()
                                                                           {
                                                                               Message = i.GetGenericArguments()[0],
                                                                               HandlingMethod = i.GetMethods()[0],
                                                                               CorrelatingMethod = i.GetMethods()[1]
                                                                           }).ToList();



                //commands with correlation
                var commandInterfacesWithCorrelation = s.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleCommandWithCorrelation<>));
                messageTypes.AddRange(commandInterfacesWithCorrelation.Select(i => i.GetGenericArguments()[0]).ToList());                
                sagaInfo.CommandsThatAreCorrelatedByTheSaga = commandInterfacesWithCorrelation.
                                                                    Select(
                                                                           i => new MessageToMethod()
                                                                           {
                                                                               Message = i.GetGenericArguments()[0],
                                                                               HandlingMethod = i.GetMethods()[0],
                                                                               CorrelatingMethod = i.GetMethods()[1]                                                                               
                                                                           }).ToList();

                //commands starting sagas
                var commandInterfacesStartingSagas = s.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleCommandStartingSaga<>));
                var startingMessageTypes=commandInterfacesStartingSagas.Select(i => i.GetGenericArguments()[0]);
                sagaInfo.CommandsThatActivateTheSaga = commandInterfacesStartingSagas.
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
                foreach (var interfaceType in t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandle<>)))
                {
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
        }

        /// <summary>
        /// Calls each function referenced by each message in the dictionary.
        /// </summary>
        internal async Task HandleAsync<T>(T message, AFBusMessageContext messageContext, ILogger log) where T : class
        {
            log?.LogInformation("message of type " + message.GetType().ToString() + " received in AFBus");

            if (!messageHandlersDictionary.ContainsKey(message.GetType()) && !messageToSagaDictionary.ContainsKey(message.GetType()))
                throw new Exception("Handler not found for this message." + serializer.Serialize(message));

            //still sometime to wait, go back to queue
            if(messageContext.MessageDelayedTime !=null && messageContext.MessageDelayedTime > TimeSpan.Zero)
            {
                var transport = SolveDependency<ISendMessages>();

                var differenceUntilFinalWakeUp = messageContext.MessageFinalWakeUpTimeStamp.Value - DateTime.UtcNow;

                if (differenceUntilFinalWakeUp > TimeSpan.FromSeconds(1))
                {
                    if (differenceUntilFinalWakeUp >= transport.MaxDelay())
                    {
                        messageContext.MessageDelayedTime = transport.MaxDelay();
                    }
                    else
                    {
                        messageContext.MessageDelayedTime = differenceUntilFinalWakeUp;
                    }


                    await transport.SendMessageAsync(message, messageContext.Destination, messageContext);

                    return;
                }
            }
                        
            await InvokeStatelessHandlers(message, messageContext, log).ConfigureAwait(false);

            await InvokeSagaHandlers(message, messageContext, log).ConfigureAwait(false);
          
        }

        /// <summary>
        /// Deserializes and invokes the handlers.
        /// </summary>
        /// <param name="serializedMessage"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public async Task HandleAsync(string serializedMessage, ILogger log)
        {
         
            var deserializedMessageWrapper = serializer.Deserialize(serializedMessage, typeof(AFBusMessageEnvelope)) as AFBusMessageEnvelope;

            string messageBody = deserializedMessageWrapper.Body;

            if (deserializedMessageWrapper.Context.BodyInFile)
            {
                try
                {
                    messageBody = await messageSender.ReadMessageBodyFromFileAsync(messageBody).ConfigureAwait(false);
                }
                catch (StorageException ex)
                {
                    if(ex.Message.Contains("The specified blob does not exist"))
                    {
                        log?.LogWarning("message ignored because file " + messageBody + " has not been found");
                        return;
                    }
                    else
                    {
                        throw;
                    }

                  
                }

            }

            var deserializedMessage = serializer.Deserialize(messageBody, Type.GetType(deserializedMessageWrapper.Context.BodyType));

            deserializedMessageWrapper.Context.ActualServiceName = serviceName;


            await HandleAsync(deserializedMessage, deserializedMessageWrapper.Context, log).ConfigureAwait(false);

            if (deserializedMessageWrapper.Context.BodyInFile)
            {
                await messageSender.DeleteFileWithMessageBodyAsync(deserializedMessageWrapper.Body).ConfigureAwait(false);
            }
          
        }

        /// <summary>
        /// Calls each function referenced by each message in the dictionary.
        /// </summary>
        public async Task HandleAsync<T>(T message, ILogger log) where T : class
        {
            log?.LogInformation("message of type "+message.GetType().ToString()+" received in AFBus");

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

        private async Task InvokeSagaHandlers<T>(T message, AFBusMessageContext messageContext, ILogger log) where T : class
        {
            //The message can not be executed in a Saga
            if (!messageToSagaDictionary.ContainsKey(message.GetType()))
                return;

            var instantiated = await LookForCommandsProcessedByASaga(message, messageContext, log).ConfigureAwait(false);
            instantiated = instantiated || await LookForEventsProcessedBySagas(message, messageContext, log).ConfigureAwait(false);

            if (!instantiated)
                log?.LogInformation("Saga not found for message " + serializer.Serialize(message));

        }

        private async Task<bool> LookForCommandsProcessedByASaga<T>(T message, AFBusMessageContext messageContext, ILogger log) where T : class
        {
            var instantiated = false;

            foreach (var sagaInfo in messageToSagaDictionary[message.GetType()])
            {
               
                var saga = CreateInstance(sagaInfo.SagaType);//Activator.CreateInstance(sagaInfo.SagaType);
                dynamic sagaDynamic = saga;

                var sagaMessageToMethod = sagaInfo.CommandsThatAreCorrelatedByTheSaga.FirstOrDefault(m => m.Message == message.GetType());

                //try to load saga from repository
                if (sagaMessageToMethod != null)
                {
                    var locker = new SagaAzureStorageLocker();
                    object[] lookForInstanceParametersArray = new object[] { message };
                    sagaDynamic.SagaPersistence = new SagaAzureStoragePersistence(locker, this.lockSaga);
                    dynamic sagaData = await ((Task<SagaData>)sagaMessageToMethod.CorrelatingMethod.Invoke(saga, lookForInstanceParametersArray)).ConfigureAwait(false);

                    if (sagaData != null)
                    {
                        try
                        {
                            sagaDynamic.Data = sagaData;

                            var bus = new Bus(serializer, SolveDependency<ISendMessages>(), SolveDependency<IPublishEvents>())
                            {
                                Context = messageContext
                            };

                            object[] parametersArray = new object[] { bus, message, log };

                            await ((Task)sagaMessageToMethod.HandlingMethod.Invoke(saga, parametersArray)).ConfigureAwait(false);

                            await sagaPersistence.UpdateAsync(sagaDynamic.Data).ConfigureAwait(false);

                            instantiated = true;
                        }
                        catch (Exception ex)
                        {
                            //if there is an error release the lock.
                            log?.LogError(ex.Message, ex);
                            var sagaID = sagaData.PartitionKey + sagaData.RowKey;
                            await locker.ReleaseLock(sagaID, sagaData.LockID);
                            throw ex;
                        }
                    }
                }


                sagaMessageToMethod = sagaInfo.CommandsThatActivateTheSaga.FirstOrDefault(m => m.Message == message.GetType());
                //if not => create the saga
                if (!instantiated && sagaMessageToMethod != null)
                {
                    var bus = new Bus(serializer, SolveDependency<ISendMessages>(), SolveDependency<IPublishEvents>())
                    {
                        Context = messageContext
                    };

                    object[] parametersArray = new object[] { bus, message, log };

                    await ((Task)sagaMessageToMethod.HandlingMethod.Invoke(saga, parametersArray)).ConfigureAwait(false);

                    await sagaPersistence.InsertAsync(sagaDynamic.Data).ConfigureAwait(false);

                    instantiated = true;
                }
                                               
            }

            return instantiated;
        }

        private async Task<bool> LookForEventsProcessedBySagas<T>(T message, AFBusMessageContext messageContext, ILogger log) where T : class
        {
            var instantiated = false;

            foreach (var sagaInfo in messageToSagaDictionary[message.GetType()])
            {
                
                var saga = CreateInstance(sagaInfo.SagaType);
                dynamic sagaDynamic = saga;

                var sagaMessageToMethod = sagaInfo?.EventsThatCorrelatesSagas?.FirstOrDefault(m => m.Message == message.GetType());

                //try to load saga from repository
                if (sagaMessageToMethod != null)
                {
                    var locker = new SagaAzureStorageLocker();
                    object[] lookForInstanceParametersArray = new object[] { message };
                    sagaDynamic.SagaPersistence = new SagaAzureStoragePersistence(locker, this.lockSaga);
                    var sagasData = await ((Task<List<SagaData>>)sagaMessageToMethod.CorrelatingMethod.Invoke(saga, lookForInstanceParametersArray)).ConfigureAwait(false);

                    if (sagasData != null)
                    {
                        //process each saga data independently
                        foreach (var sagaData in sagasData)
                        {
                            try
                            {
                                dynamic finalSagaData = sagaData;
                                SagaData lockedSagaData = await sagaDynamic.SagaPersistence.GetSagaDataAsync<SagaData>(sagaData.PartitionKey, sagaData.RowKey).ConfigureAwait(false);
                                finalSagaData.Timestamp = lockedSagaData.Timestamp;
                                finalSagaData.ETag = lockedSagaData.ETag;
                                finalSagaData.LockID = lockedSagaData.LockID;

                                sagaDynamic.Data = finalSagaData;

                                var bus = new Bus(serializer, SolveDependency<ISendMessages>(), SolveDependency<IPublishEvents>())
                                {
                                    Context = messageContext
                                };

                                object[] parametersArray = new object[] { bus, message, log };

                                await ((Task)sagaMessageToMethod.HandlingMethod.Invoke(saga, parametersArray)).ConfigureAwait(false);

                                await sagaPersistence.UpdateAsync(sagaDynamic.Data).ConfigureAwait(false);

                                instantiated = true;

                            }
                            catch (Exception ex)
                            {
                                //if there is an error release the lock.
                                log?.LogError(ex.Message, ex);
                                var sagaID = sagaData.PartitionKey + sagaData.RowKey;
                                await locker.ReleaseLock(sagaID, sagaData.LockID);
                                throw ex;
                            }
                        }
                    }
                }
                                               
            }

            return instantiated;
        }

        private async Task InvokeStatelessHandlers<T>(T message, AFBusMessageContext messageContext, ILogger log) where T : class
        {
            //The message can not be executed in a stateless handler
            if (!messageHandlersDictionary.ContainsKey(message.GetType()))
                return;

            var handlerTypeList = messageHandlersDictionary[message.GetType()];

            foreach (var t in handlerTypeList)
            {
                var handler = CreateInstance(t);

                var bus = new Bus(serializer, SolveDependency<ISendMessages>(), SolveDependency<IPublishEvents>())
                {
                    Context = messageContext
                };

                object[] parametersArray = new object[] { bus, message, log };

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
