using System;
using System.Collections.Generic;
using EventFeed.Consumer.EventFeed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EventFeed.Consumer.Infrastructure
{
    internal class EventDispatcher: IEventDispatcher
    {
        public void Dispatch(Event @event, Action markEventAsProcessed)
        {
            _logger.LogDebug("Dispatching event {Id}", @event.Id);
            
            var deserializedEvent = Deserialize();

            if (deserializedEvent == null)
            {
                _logger.LogDebug("Unrecognized event type {Type}. Message will be skipped.", @event.Type);
                markEventAsProcessed();
                
                return;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                Type handlerType = GetHandlerType();
                object handler = scope.ServiceProvider.GetRequiredService(handlerType);

                dynamic handlerAsDynamic = handler;
                dynamic eventAsDynamic = deserializedEvent;
                handlerAsDynamic.HandleEvent(eventAsDynamic);

                _logger.LogDebug("Dispatched event {Id} of type {Type}", @event.Id, @event.Type);

                markEventAsProcessed();
            }
            
            object Deserialize()
            {
                var type = GetEventType();

                if (type == null)
                    return null;

                return JsonConvert.DeserializeObject(@event.Payload, type);
            }
            
            Type GetEventType()
            {
                _contentTypeMapping.TryGetValue(@event.Type, out var type);
                return type;
            }

            Type GetHandlerType()
            {
                var type = GetEventType();
                return typeof(IEventHandler<>).MakeGenericType(type);
            }
        }

        public EventDispatcher(
            IServiceProvider serviceProvider,
            IReadOnlyDictionary<string, Type> contentTypeMapping,
            ILogger<EventDispatcher> logger
        )
        {
            _serviceProvider = serviceProvider;
            _contentTypeMapping = contentTypeMapping;
            _logger = logger;
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly IReadOnlyDictionary<string, Type> _contentTypeMapping;
        private readonly ILogger<EventDispatcher> _logger;
    }
}
