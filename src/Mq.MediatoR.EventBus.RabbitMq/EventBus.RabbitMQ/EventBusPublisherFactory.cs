// Copyright © Alexander Paskhin 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Mq.Mediator.EventBus.RabbitMQ
{
    /// <summary>
    /// The RabbitMQ implementation of the <see cref="EventBusPublisherFactory{TNotification}"/>
    /// </summary>
    public class EventBusPublisherFactory<TNotification> : IEventBusPublisherFactory<TNotification> where TNotification : class
    {
        private readonly EventBusConfigutation _configutationRabbitMQ;
        private readonly RabbitMQConnectionFactory _connectionFactory;

        public EventBusPublisherFactory(IOptions<EventBusConfigutation> configutationRabbitMQ,
                                        RabbitMQConnectionFactory connectionFactory)
        {
            _configutationRabbitMQ = configutationRabbitMQ.Value;
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Creates the <see cref="IEventBusPublisher{TNotification}"/> instance.
        /// </summary>
        /// <returns>The <see cref="IEventBusPublisher{TNotification}"/> instance.</returns>
        public IEventBusPublisher<TNotification> CreatePublisher()
        {
            return new EventBusPublishProvider(_configutationRabbitMQ, _connectionFactory);
        }

        /// <summary>
        /// The RabbitMQ implementation of the <see cref="IEventBusPublisher{TNotification}"/>
        /// </summary>
        public class EventBusPublishProvider : IEventBusPublisher<TNotification>
        {
            private readonly object sync_root = new object();
            private readonly Type NotificationType = typeof(TNotification);
            private readonly EventBusConfigutation _configutationRabbitMQ;
            private readonly RabbitMQConnectionFactory _connectionFactory;
            private readonly RetryPolicy _policy;
            private IConnection _connection;
            private IModel _channel;
            private readonly IBasicProperties _basicProperties;
            private readonly EventBusPublisherFactory<TNotification> _eventBusPublisherFactory;
            private bool _disposed;
            private RabbitMQTypeMap messageMqParms;
            private BusConnectionState _busConnectionState;
            private Exception _connectionError;

            /// <summary>
            /// Constructs the class object.
            /// </summary>
            /// <param name="configutationRabbitMQ">The event bus configuration.</param>
            /// <param name="connectionFactory">The connection factory.</param>
            public EventBusPublishProvider(EventBusConfigutation configutationRabbitMQ,
                                           RabbitMQConnectionFactory connectionFactory)
            {
                _configutationRabbitMQ = configutationRabbitMQ;
                _connectionFactory = connectionFactory;
                _policy = RetryPolicy.Handle<SocketException>().Or<BrokerUnreachableException>()
                                     .WaitAndRetry(_configutationRabbitMQ.RecconectCount,
                                     retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            }

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
                        throw new ObjectDisposedException(nameof(EventBusPublishProvider));
                    }
                    else
                    {
                        return false;
                    }
                }

                return TryConnectInternal(CancellationToken.None, returnErrorFlag);
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

            public RabbitMQConnectionFactory ConnectionFactory => _connectionFactory;

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

            private RabbitMQTypeMap LocateTheMapper(string typeName)
            {
                return _configutationRabbitMQ.Mapper.FirstOrDefault((s) => typeName.StartsWith(s.TypePrefix, StringComparison.InvariantCultureIgnoreCase)) ??
                    _configutationRabbitMQ.DefaultMap; ;
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
                          }, cancellationToken);

                    }
                    catch (Exception ex)
                    {
                        _connection = null;
                        _channel = null;
                        _busConnectionState = BusConnectionState.ErrorDisconnected;
                        _connectionError = ex;
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

            /// <summary>
            /// Publishes notification over the bus.
            /// </summary>
            /// <param name="notification">The send notification.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <param name="returnErrorFlag">If it's true the method returns error flag but not throw an exception.</param>
            /// <exception>The wide range.</exception>
            /// <returns>The task of notification to publish with flag of success.
            /// Returns false if there is no a subscription handler.</returns>
            public bool PublishSync(TNotification notification, CancellationToken cancellationToken, bool returnErrorFlag = false)
            {
                if (_disposed)
                {
                    if (!returnErrorFlag)
                    {
                        throw new ObjectDisposedException(nameof(EventBusPublishProvider));
                    }
                    else
                    {
                        return false;
                    }
                }


                if (!IsConnected)
                {
                    if (!TryConnectInternal(cancellationToken, returnErrorFlag))
                    {
                        return false;
                    }
                }

                var message = JsonConvert.SerializeObject(notification);
                var body = Encoding.UTF8.GetBytes(message);
                string nName = NotificationType.Name;
                string queueName = string.IsNullOrEmpty(messageMqParms.QueueName) ? nName : messageMqParms.QueueName;
                string routingKey = string.IsNullOrEmpty(messageMqParms.QueueRoutingKey) ? queueName : messageMqParms.QueueRoutingKey;

                try
                {
                    _policy.Execute((cToken) =>
                    {
                        cToken.ThrowIfCancellationRequested();
                        var properties = _channel.CreateBasicProperties();
                        properties.ContentType = "text/plain";
                        properties.DeliveryMode = 2; // persistent

                        _channel.BasicPublish(messageMqParms.ExchangeName, routingKey, messageMqParms.PublishMandatory, properties, body);

                    }, cancellationToken);

                }
                catch (Exception ex)
                {
                    _busConnectionState = BusConnectionState.ErrorDisconnected;
                    _connectionError = ex;

                    if (!returnErrorFlag)
                    {
                        throw;
                    }
                    return false;
                }
                return true;
            }


            /// <summary>
            /// Publishes notification over the bus.
            /// </summary>
            /// <param name="notification">The send notification.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <param name="returnErrorFlag">If it's true the method returns error flag but not throw an exception.</param>
            /// <exception>The wide range.</exception>
            /// <returns>The task of notification to publish with flag of success.
            /// Returns false if there is no a subscription handler.</returns>
            public Task<bool> PublishAsync(TNotification notification, CancellationToken cancellationToken, bool returnErrorFlag = false)
            {
                return Task.Run(() => PublishSync(notification, cancellationToken, returnErrorFlag));
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

        }
    }
}
