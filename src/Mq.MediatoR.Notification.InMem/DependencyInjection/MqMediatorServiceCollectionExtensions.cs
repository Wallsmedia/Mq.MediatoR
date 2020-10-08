// Copyright © Alexander Paskhin 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Mq.Mediator.Abstractions;
using Mq.Mediator.Notification.InMem;

namespace Mq.Mediator.Notification.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding Mq.Mediator services to the DI container.
    /// </summary>
    public static class MqMediatoRNotificationInMemExtensions
    {
        /// <summary>
        /// Adds services required for using of Notification Mq.Mediator.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddMqNotificationMediator(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAdd(ServiceDescriptor.Singleton(typeof(INotificationMediator<>), typeof(NotificationMediatorManager<>)));
            services.TryAdd(ServiceDescriptor.Singleton(typeof(INotificationMediatorFactory<>), typeof(InMemNotificationMediatorFactory<>)));
            return services;
        }

        /// <summary>
        /// Add delegate of <see cref="NotificationDelegateAsync{TRequest}"/> to the services collection.
        /// </summary>
        /// <typeparam name="TNotification">The request type</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="notificationDelegate">The instance of the publish processing delegate.</param>
        /// <param name="servicingOrder">The order of the processing.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddNotificationProcessingHandler<TNotification>(this IServiceCollection services, NotificationDelegateAsync<TNotification> notificationDelegate, ServicingOrder servicingOrder = ServicingOrder.Processing) where TNotification : class
        {
            if (notificationDelegate == null)
            {
                throw new ArgumentNullException(nameof(notificationDelegate));
            }
            services.AddSingleton<INotificationHandler<TNotification>>(new NotificationHandlerProcessingWrapper<TNotification>(notificationDelegate, servicingOrder));
            return services;
        }

    }
}
