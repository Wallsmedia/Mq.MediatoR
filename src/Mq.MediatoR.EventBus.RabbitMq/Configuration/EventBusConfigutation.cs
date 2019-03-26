// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Mq.Mediator.EventBus.RabbitMQ
{
    public class EventBusConfigutation
    {
        /// <summary>
        /// The host to connect to.
        /// </summary>
        public string HostName { get; set; } = "localhost";

        /// <summary>
        ///  User name to use when authenticating to the server.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///  Password to use when authenticating to the server.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The Uri to connect to.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// The port to connect on. RabbitMQ.Client.AmqpTcpEndpoint.UseDefaultPort indicates
        /// the default for the protocol should be used.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        ///  Timeout setting for socket read operations (in milliseconds).
        /// </summary>
        public int? SocketReadTimeout { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? SocketWriteTimeout { get; set; }

        /// <summary>
        /// Timeout setting for socket write operations (in milliseconds).
        /// </summary>
        public int RecconectCount { get; set; } = 5;

        /// <summary>
        /// The type mapping list onto RabbitMQ entities.
        /// </summary>
        public List<RabbitMQTypeMap> Mapper { get; } = new List<RabbitMQTypeMap>();

        /// <summary>
        /// The default type mapping.
        /// </summary>
        public RabbitMQTypeMap DefaultMap { get; set; } = new RabbitMQTypeMap
        {

            TypePrefix = "*",
            ExchangeName = "",
            ExchangeDurable = true,
        };
    }


}
