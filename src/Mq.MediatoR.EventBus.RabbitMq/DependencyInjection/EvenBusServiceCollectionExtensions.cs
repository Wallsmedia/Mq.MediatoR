// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mq.Mediator.EventBus.RabbitMQ;
using System;

namespace Mq.Mediator.EventBus.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding Event Bus to the DI container.
    /// </summary>
    public static class EvenBusServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required for using of the Event Bus Publisher.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddEventBusPublisherRabbitMQ(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            services.TryAdd(ServiceDescriptor.Singleton(typeof(RabbitMQConnectionFactory), typeof(RabbitMQConnectionFactory)));
            services.TryAdd(ServiceDescriptor.Singleton(typeof(IEventBusPublisherFactory<>), typeof(EventBusPublisherFactory<>)));
            services.AddOptions();
            return services;
        }

        /// <summary>
        /// Adds services required for using of the Event Bus Subscriber.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddEventBusSubscriberRabbitMQ(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            services.TryAdd(ServiceDescriptor.Singleton(typeof(RabbitMQConnectionFactory), typeof(RabbitMQConnectionFactory)));
            services.TryAdd(ServiceDescriptor.Singleton(typeof(IEventBusSubscribtionFactory<>), typeof(EventBusSubscriberFactory<>)));
            services.AddOptions();
            return services;
        }

    }
}
