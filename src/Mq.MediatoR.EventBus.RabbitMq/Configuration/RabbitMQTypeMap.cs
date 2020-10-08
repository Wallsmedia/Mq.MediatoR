// Copyright © Alexander Paskhin 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using RabbitMQ.Client;

namespace Mq.Mediator.EventBus.RabbitMQ
{
    /// <summary>
    /// Used for mapping a type to RabbitMQ broker entities.
    /// </summary>
    public class RabbitMQTypeMap
    {
        /// <summary>
        /// The type name prefix, without a namespace.
        /// </summary>
        public string TypePrefix { get; set; }

        /// <summary>
        /// Exchange http://www.rabbitmq.com/tutorials/amqp-concepts.html#exchanges
        /// Default: http://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-default
        ///     - delivers messages to queues based on the message routing key.
        /// Direct:  http://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-direct
        ///     - delivers messages to queues based on the message routing key.
        /// Fanout:  http://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-fanout
        ///     -  routes messages to all of the queues that are bound to it and the routing key is ignored.
        /// Topic:   http://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-topic
        ///     - exchanges route messages to one or many queues based on matching between a message routing
        ///       key and the pattern that was used to bind a queue to an exchange
        /// </summary>

        /// <summary>
        /// The Name of the exchange.
        /// </summary>
        public string ExchangeName { get; set; } = "";

        /// <summary>
        /// The type of exchange.
        /// </summary>
        public string ExchangeStyle { get; set; } = ExchangeType.Direct;

        /// <summary>
        /// Durability (exchanges survive broker restart)
        /// </summary>
        public bool ExchangeDurable { get; set; }

        /// <summary>
        /// Auto-delete (exchange is deleted when last queue is unbound from it)
        /// </summary>
        public bool ExchangeAutoDelete { get; set; }

        /// <summary>
        /// Queue http://www.rabbitmq.com/queues.html
        /// </summary>

        /// <summary>
        /// The name of the queue.
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// Durable (the queue will survive a broker restart)
        /// </summary>
        public bool QueueDurable { get; set; }

        /// <summary>
        /// Exclusive (used by only one connection and the queue will be deleted when that connection closes)
        /// </summary>
        public bool QueueExclusive { get; set; }

        /// <summary>
        /// Auto-delete (queue that has had at least one consumer is deleted when last consumer unsubscribes)
        /// </summary>
        public bool QueueAutoDelete { get; set; }

        /// <summary>
        /// Queue binding routing key.
        /// </summary>
        public string QueueRoutingKey { get; set; } = "";

        /// <summary>
        /// Basic publish flag.i.e.  consumer should exist.
        /// </summary>
        public bool PublishMandatory { get; set; }

        /// <summary>
        /// The consumer auto ask flag.
        /// </summary>
        public bool ConsumerAutoAsk { get; set; }

        /// <summary>
        /// The consumer special tag.
        /// </summary>
        public string ConsumerTag { get; set; } = "";

        /// <summary>
        /// The consumer no local flag.
        /// </summary>
        public bool ConsumerNoLocal { get; set; }

        /// <summary>
        /// The consumer exclusive flag.
        /// </summary>
        public bool ConsumerExclusive { get; set; }

    }
}
