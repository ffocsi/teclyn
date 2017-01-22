﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Teclyn.Core.Domains;
using Teclyn.Core.Dummies;
using Teclyn.Core.Events.Metadata;
using Teclyn.Core.Ioc;
using Teclyn.Core.Tools;

namespace Teclyn.Core.Events.Handlers
{
    public class EventHandlerService
    {
        [Inject]
        public TeclynApi Teclyn { get; set; }

        private IDictionary<Type, List<Metadata.EventHandlerMetadata>> handlersMetaData = new Dictionary<Type, List<EventHandlerMetadata>>();
       
        public IEnumerable<EventHandlerMetadata> GetEventHandlers(Type eventType)
        {
            var handlersMetadata = handlersMetaData.GetValueOrDefault(eventType);

            if (handlersMetadata == null)
            {
                return Enumerable.Empty<EventHandlerMetadata>();
            }
            else
            {
                return handlersMetadata;
            }
        }

        public IReadOnlyDictionary<Type, IEnumerable<Metadata.EventHandlerMetadata>> GetEventHandlers()
        {
            return this.handlersMetaData.ToDictionary(pair => pair.Key, pair => pair.Value.SafeCast<IEnumerable<Metadata.EventHandlerMetadata>>());
        }
        
        public void RegisterEventHandler(Type eventHandlerType)
        {
            var handlerInterfacesWithoutAggregate = eventHandlerType
            .GetTypeInfo()
            .ImplementedInterfaces
            .Where(@interface => @interface.IsConstructedGenericType && @interface.GetGenericTypeDefinition() == typeof(IEventHandler<>))
            .Select(@interface => new Tuple<Type, Func<IAggregate, IEventInformation, Task>>(@interface.GenericTypeArguments[0],
                (aggregate, eventInformation) =>
                {
                    var handler = Teclyn.Get(eventHandlerType);
                    var method = typeof(IEventHandler<>).MakeGenericType(@interface.GenericTypeArguments[0]).GetRuntimeMethod("Handle", new[]
                          {
                                typeof(IEventInformation<>).MakeGenericType(@interface.GenericTypeArguments[0])
                          });

                    return (Task) method.Invoke(handler, new object[] {eventInformation});
                }));

            var handlerInterfacesWithAggregate = eventHandlerType
                .GetTypeInfo()
                .ImplementedInterfaces
                .Where(@interface => @interface.IsConstructedGenericType && @interface.GetGenericTypeDefinition() == typeof(IEventHandler<,>))
                .Select(@interface => new Tuple<Type, Func<IAggregate, IEventInformation, Task>>(@interface.GenericTypeArguments[1],
                    (aggregate, eventInformation) =>
                    {
                        var handler = Teclyn.Get(eventHandlerType);
                        var method = typeof(IEventHandler<,>).MakeGenericType(@interface.GenericTypeArguments[0],
                            @interface.GenericTypeArguments[1]).GetRuntimeMethod("Handle", new[]
                            {
                                @interface.GenericTypeArguments[0],
                                typeof(IEventInformation<>).MakeGenericType(@interface.GenericTypeArguments[1])
                            });

                        return (Task) method.Invoke(handler, new object[] { aggregate, eventInformation });
                    }));

            var handledEvents = handlerInterfacesWithoutAggregate
                .Union(handlerInterfacesWithAggregate);
            
            var metadataList = handledEvents.Select(@eventInfo => new EventHandlerMetadata(eventHandlerType, @eventInfo.Item1, @eventInfo.Item2));

            foreach (var eventHandlerInfo in metadataList)
            {
                var list = handlersMetaData.GetValueOrDefault(eventHandlerInfo.EventType, null);

                if (list == null)
                {
                    list = new List<EventHandlerMetadata>();
                    this.handlersMetaData[eventHandlerInfo.EventType] = list;
                }

                list.Add(eventHandlerInfo);
            }
        }
    }
}