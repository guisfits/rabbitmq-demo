using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MediatR;
using MicroRabbit.SharedKernel.Core.CQRS;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MicroRabbit.SharedKernel.Infra.Bus
{
    public class Bus : IBus
    {
        #region Constructor 

        private readonly ConnectionFactory _rabbitFactory;
        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;

        public Bus(IMediator mediator)
        {
            this._mediator = mediator;
            this._handlers = new Dictionary<string, List<Type>>();
            this._eventTypes = new List<Type>();
            this._rabbitFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "user",
                Password = "password",
                Port = 5672
            };
        }

        #endregion

        public Task SendCommand<T>(T command) where T : Command
        {
            return this._mediator.Send(command);
        }

        public void Publish<T>(T @event) where T : Event
        {
            using(var connection = _rabbitFactory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                var body = JsonSerializer.SerializeToUtf8Bytes(@event);

                var eventName = @event.Type;
                channel.QueueDeclare(eventName, false, false, false, null);
                channel.BasicPublish("", eventName, null, body);
            }
        }

        public void Subscribe<T, TH>() where T : Event where TH : IEventHandler<T>
        {
            var eventType = typeof(T);
            var handlerType = typeof(TH);

            this.AddEvent(eventType);

            if (_handlers[eventType.Name].Any(x => x == handlerType))
                return;

            _handlers[eventType.Name].Add(handlerType);

            StartBasicConsume<T>();
        }

        #region Privates

        private void AddEvent(Type eventType)
        {
            if (this._eventTypes.Contains(eventType) == false)
                this._eventTypes.Add(eventType);

            if (this._handlers.ContainsKey(eventType.Name) == false)
                this._handlers.Add(eventType.Name, new List<Type>());
        }

        private void StartBasicConsume<TEvent>() where TEvent : Event
        {
            using(var connection = _rabbitFactory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                var eventName = typeof(TEvent).Name;
                channel.QueueDeclare(eventName, false, false, false, null);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;

                channel.BasicConsume(eventName, false, consumer);
            }
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var eventName = e.RoutingKey;
                var message = Encoding.UTF8.GetString(e.Body.ToArray());

                await ProcessEvent(eventName, message).ConfigureAwait(false);
            }
            catch (Exception ex) { }
        }

        private async Task ProcessEvent(string eventName, string messageJson)
        {
            if (this._handlers.ContainsKey(eventName))
            {
                foreach (var subscription in _handlers[eventName])
                {
                    var handler = Activator.CreateInstance(subscription);
                    if (handler == null) continue;

                    var eventType = _eventTypes.SingleOrDefault(x => x.Name == eventName);
                    var @event = JsonSerializer.Deserialize(messageJson, eventType);
                    var conreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    await (Task) conreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                }
            }
        }

        #endregion
    }
}
