// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Mq.Mediator.EventBus.RabbitMQ
{
    /// <summary>
    /// The connection factory for the RabbitMQ client.
    /// </summary>
    public class RabbitMQConnectionFactory
    {
      readonly  ConnectionFactory _connectionFactory;

        /// <summary>
        /// Constructs the class instance..
        /// </summary>
        /// <param name="configutationRabbitMQ">The Event Bus configuration.</param>
        public RabbitMQConnectionFactory(IOptions<EventBusConfigutation> configutationRabbitMQ)
        {
            var cnfg = configutationRabbitMQ.Value;

            _connectionFactory = new ConnectionFactory();
            if (cnfg.HostName != null) _connectionFactory.HostName = cnfg.HostName;
            if (cnfg.UserName != null) _connectionFactory.UserName = cnfg.UserName;
            if (cnfg.Password != null) _connectionFactory.Password = cnfg.Password;
            if (cnfg.Uri != null) _connectionFactory.Uri = cnfg.Uri;
            if (cnfg.Port.HasValue) _connectionFactory.Port = cnfg.Port.Value;
            if (cnfg.SocketReadTimeout.HasValue) _connectionFactory.SocketReadTimeout = TimeSpan.FromMilliseconds(cnfg.SocketReadTimeout.Value);
            if (cnfg.SocketWriteTimeout.HasValue) _connectionFactory.SocketWriteTimeout = TimeSpan.FromMilliseconds(cnfg.SocketWriteTimeout.Value);
        }

        /// <summary>
        /// Gets the connection factory instance.
        /// </summary>
        /// <returns></returns>
        public ConnectionFactory GetConnectionFactory() => _connectionFactory;
    }
}
