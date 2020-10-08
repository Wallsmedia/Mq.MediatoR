// Copyright © Alexander Paskhin 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Mq.Mediator.Abstractions;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Mq.Mediator.EventBus.RabbitMQ
{
    /// <summary>
    /// The RabbitMQ implementation of the <see cref="EventBusSubscriberFactory{TNotification}"/>
    /// </summary>
    public class EventBusSubscriberFactory<TNotification> : IEventBusSubscribtionFactory<TNotification> where TNotification : class
    {
        private readonly EventBusConfigutation _configutationRabbitMQ;
        private readonly RabbitMQConnectionFactory _connectionFactory;

        public EventBusSubscriberFactory(IOptions<EventBusConfigutation> configutationRabbitMQ,
                                        RabbitMQConnectionFactory connectionFactory)
        {
            _configutationRabbitMQ = configutationRabbitMQ.Value;
            _connectionFactory = connectionFactory;
        }


        /// <summary>
        /// Subscribe for notification events.
        /// </summary>
        /// <param name="notificationDelegate">The delegate that handlers subscription.</param>
        /// <returns>The task <see cref="IEventBusSubscription"/> 
        /// with <see cref="IDisposable"/> that can be used to remove the subscription.</returns>
        public Task<IEventBusSubscription> SubscribeAsync(SubscriptionDelegate<TNotification> notificationDelegate)
        {
            return Task.Run(() => SubscribeSync(notificationDelegate));
        }

        /// <summary>
        /// Subscribe for notification events.
        /// </summary>
        /// <param name="notificationDelegate">The delegate that handlers subscription.</param>
        /// <returns>The <see cref="IEventBusSubscription"/> 
        /// with <see cref="IDisposable"/> that can be used to remove the subscription.</returns>
        public IEventBusSubscription SubscribeSync(SubscriptionDelegate<TNotification> notificationDelegate)
        {
            EventBusSubscriberProvider eventBusSubscriberProvider = new EventBusSubscriberProvider(_configutationRabbitMQ,
                                                                                _connectionFactory, notificationDelegate);
            eventBusSubscriberProvider.TryToConnect();
            return eventBusSubscriberProvider;
        }


        /// <summary>
        /// The RabbitMQ implementation of the <see cref="IEventBusSubscription"/>
        /// </summary>
        public class EventBusSubscriberProvider : IEventBusSubscription
        {
            private readonly object sync_root = new object();
            private readonly Type NotificationType = typeof(TNotification);
            private readonly EventBusConfigutation _configutationRabbitMQ;
            private readonly RabbitMQConnectionFactory _connectionFactory;
            private readonly SubscriptionDelegate<TNotification> _notificationDelegate;
            private readonly RetryPolicy _policy;
            private IConnection _connection;
            private IModel _channel;
            private readonly IBasicProperties _basicProperties;
            private readonly EventBusPublisherFactory<TNotification> _eventBusPublisherFactory;
            private bool _disposed;
            private RabbitMQTypeMap messageMqParms;
            private BusConnectionState _busConnectionState;
            private Exception _connectionError;

            public EventBusSubscriberProvider(EventBusConfigutation configutationRabbitMQ,
                                           RabbitMQConnectionFactory connectionFactory,
                                           SubscriptionDelegate<TNotification> notificationDelegate)
            {
                _configutationRabbitMQ = configutationRabbitMQ;
                _connectionFactory = connectionFactory;
                _notificationDelegate = notificationDelegate;

                _policy = RetryPolicy.Handle<SocketException>().Or<BrokerUnreachableException>()
                                     .WaitAndRetry(_configutationRabbitMQ.RecconectCount,
                                     retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            }
            /// <summary>
            /// Gets the RabbitMQ connection status.
            /// </summary>
            public bool IsConnected => (_connection != null && _channel != null)
                                    && (!_disposed) && (_connection.IsOpen) && (_channel.IsOpen);
            /// <summary>
            /// Gets the RabbitMq connection
            /// </summary>
            public IConnection Connection { get => _connection; private set => _connection = value; }

            /// <summary>
            /// Gets the RabbitMQ channel.
            /// </summary>
            public IModel Channel { get => _channel; private set => _channel = value; }

            /// <summary>
            /// Gets the effective configuration.
            /// </summary>
            public EventBusConfigutation ConfigutationRabbitMQ => _configutationRabbitMQ;

            /// <summary>
            /// The RabbitMQ connection factory.
            /// </summary>
            public RabbitMQConnectionFactory ConnectionFactory => _connectionFactory;

            /// <summary>
            /// The current connection state to event bus.
            /// It is used for health check monitoring.
            /// </summary>
            public BusConnectionState BusConnectionState => _busConnectionState;

            /// <summary>
            /// The connection error.
            /// </summary>
            public Exception ConnectionError => _connectionError;

            /// <summary>
            /// The event that handles subscription errors.
            /// </summary>
            public event Action<Exception> ErrorHandler;

            /// <summary>
            /// Auto ask mode for pulling messages from the queue.
            /// </summary>
            public bool AutoAsk { get; private set; }

            /// <summary>
            /// The RabbitMQ consumer instance.
            /// </summary>
            public EventingBasicConsumer BasicConsumer { get; private set; }

            /// <summary>
            /// The current connection state to event bus.
            /// </summary>
            public void Dispose()
            {
                if (_disposed == true)
                {
                    _disposed = true;
                    _channel?.Dispose();
                    _connection?.Dispose();
                }
            }

            /// <summary>
            /// Tries to connect to the event bus.
            /// </summary>
            /// <param name="returnErrorFlag">If it's true the method returns error flag but not throw an exception.</param>
            /// <exception>The wide range.</exception>
            /// <returns>The connection success flag.</returns>
            public bool TryToConnect(bool returnErrorFlag = false)
            {
                if (_disposed)
                {
                    if (!returnErrorFlag)
                    {
                        throw new ObjectDisposedException(nameof(EventBusSubscriberProvider));
                    }
                    else
                    {
                        return false;
                    }
                }

                return TryConnectInternal(CancellationToken.None, returnErrorFlag);
            }


            private bool TryConnectInternal(CancellationToken cancellationToken, bool returnErrorFlag = false)
            {

                lock (sync_root)
                {
                    _connectionError = null;

                    if (IsConnected)
                    {
                        _busConnectionState = BusConnectionState.Connected;
                        return true;
                    }

                    _busConnectionState = BusConnectionState.Connecting;

                    try
                    {
                        _policy.Execute((cToken) =>
                        {

                            cToken.ThrowIfCancellationRequested();
                            _connection = _connectionFactory.GetConnectionFactory().CreateConnection();
                            _channel = _connection.CreateModel();
                            string nName = NotificationType.Name;
                            messageMqParms = LocateTheMapper(nName);

                            AutoAsk = messageMqParms.ConsumerAutoAsk;

                            if (!string.IsNullOrEmpty(messageMqParms.ExchangeName))
                            {
                                _channel.ExchangeDeclare(messageMqParms.ExchangeName, messageMqParms.ExchangeStyle, messageMqParms.ExchangeDurable, messageMqParms.ExchangeAutoDelete);
                            }

                            string queueName = string.IsNullOrEmpty(messageMqParms.QueueName) ? nName : messageMqParms.QueueName;
                            string routingKey = string.IsNullOrEmpty(messageMqParms.QueueRoutingKey) ? queueName : messageMqParms.QueueRoutingKey;
                            _channel.QueueDeclare(queueName, messageMqParms.QueueDurable,
                                messageMqParms.QueueExclusive,
                                messageMqParms.QueueAutoDelete);

                            if (!string.IsNullOrEmpty(messageMqParms.ExchangeName))
                            {
                                _channel.QueueBind(queueName, messageMqParms.ExchangeName, routingKey);
                            }

                            BasicConsumer = new EventingBasicConsumer(_channel);
                            BasicConsumer.Received += ProcessSubscriptionEvent;
                            BasicConsumer.ConsumerCancelled += ConsumerCancelled;
                            BasicConsumer.Shutdown += ConsumerShutdown;
                            BasicConsumer.Unregistered += ConsumerUnregested;

                            _channel.BasicConsume(BasicConsumer, queueName,
                                messageMqParms.ConsumerAutoAsk, messageMqParms.ConsumerTag,
                                messageMqParms.ConsumerNoLocal, messageMqParms.ConsumerExclusive);

                        }, cancellationToken);

                    }
                    catch (Exception ex)
                    {
                        _connection = null;
                        _channel = null;
                        _busConnectionState = BusConnectionState.ErrorDisconnected;
                        _connectionError = ex;
                        ErrorHandler?.Invoke(ex);

                        if (!returnErrorFlag)
                        {
                            throw;
                        }
                    }

                    if (!IsConnected)
                    {
                        _busConnectionState = BusConnectionState.ErrorDisconnected;
                        return false;
                    }
                    else
                    {
                        _busConnectionState = BusConnectionState.Connected;
                        _connection.ConnectionShutdown += OnConnectionShutdown;
                        _connection.CallbackException += OnCallbackException;
                        _connection.ConnectionBlocked += OnConnectionBlocked;
                        _channel.CallbackException += OnCallbackException;
                        _channel.ModelShutdown += OnConnectionShutdown;
                    }
                }
                return true;
            }


            private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
            {
                if (_disposed) return;
                TryConnectInternal(CancellationToken.None, true);
            }

            private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
            {
                if (_disposed) return;
                TryConnectInternal(CancellationToken.None, true);
            }

            private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
            {
                if (_disposed) return;
                TryConnectInternal(CancellationToken.None, true);
            }

            private void ConsumerUnregested(object sender, ConsumerEventArgs e)
            {
                if (_disposed) return;
            }

            private void ConsumerShutdown(object sender, ShutdownEventArgs e)
            {
                if (_disposed) return;
            }

            private void ConsumerCancelled(object sender, ConsumerEventArgs e)
            {
                if (_disposed) return;
            }

            private RabbitMQTypeMap LocateTheMapper(string typeName)
            {
                return _configutationRabbitMQ.Mapper.FirstOrDefault((s) => typeName.StartsWith(s.TypePrefix, StringComparison.InvariantCultureIgnoreCase)) ??
                    _configutationRabbitMQ.DefaultMap; ;
            }
            private void ProcessSubscriptionEvent(object sender, BasicDeliverEventArgs e)
            {
                var eventName = e.RoutingKey;
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                TNotification notification = JsonConvert.DeserializeObject<TNotification>(message);

                _notificationDelegate?.Invoke(notification);

                if (!AutoAsk)
                {
                    Channel.BasicAck(e.DeliveryTag, multiple: false);
                }
            }


        }

    }
}
