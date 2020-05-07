using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Infra.Bus
{
    public sealed class RabbitMqBus : IEventBus
    {
        private readonly IMediator mediator;
        private readonly Dictionary<string, List<Type>> handlers;
        private readonly List<Type> eventTypes;

        public RabbitMqBus(IMediator mediator)
        {
            this.mediator = mediator;
            this.handlers = new Dictionary<string, List<Type>>();
            this.eventTypes = new List<Type>();
        }

        public Task SendCommand<T>(T command) where T : Command
        {
            return mediator.Send(command);
        }

        public void Publish<T>(T @event) where T : Event
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://kuzzntna:sPmjUpmaMLW-2S-yGhemcwzz1cByDCdN@baboon.rmq.cloudamqp.com/kuzzntna");
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var eventName = @event.GetType().Name;
                    channel.QueueDeclare(eventName, false, false, false, null);
                    var message = JsonConvert.SerializeObject(@event);
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish("", eventName, null, body);

                }
            }
        }

        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);
            if (!eventTypes.Contains(typeof(T)))
            {
                eventTypes.Add(typeof(T));
            }
            if (!handlers.ContainsKey(eventName))
            {
                handlers.Add(eventName, new List<Type>());
            }
            if (handlers[eventName].Any(s => s.GetType() == handlerType))
            {
                throw new ArgumentException("");
            }
            handlers[eventName].Add(handlerType);
            StartBasicConsume<T>();
        }

        private void StartBasicConsume<T>() where T : Event
        {
            var factory = new ConnectionFactory()
            {
                DispatchConsumersAsync = true
            };
            factory.Uri = new Uri("amqp://kuzzntna:sPmjUpmaMLW-2S-yGhemcwzz1cByDCdN@baboon.rmq.cloudamqp.com/kuzzntna");
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            var eventName = typeof(T).Name;
            channel.QueueDeclare(eventName, false, false, false, null);
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;
            channel.BasicConsume(eventName, true, consumer);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var eventName = @event.RoutingKey;
            var message = Encoding.UTF8.GetString(@event.Body.ToArray());
            try
            {
                await ProcessEvent(eventName, message).ConfigureAwait(false);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (handlers.ContainsKey(eventName))
            {
                var subscriptions = handlers[eventName];
                foreach (var subscription in subscriptions)
                {
                    var handler = Activator.CreateInstance(subscription);
                    if (handler == null) continue;
                    var eventType = eventTypes.SingleOrDefault(t => t.Name == eventName);
                    var @event = JsonConvert.DeserializeObject(message, eventType);
                    var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                }
            }
        }
    }
}
