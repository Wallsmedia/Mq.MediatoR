// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Mq.Mediator.EventBus
{
    /// <summary>
    /// Used to create <see cref="IEventBusSubscription"/> instance.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    public interface IEventBusSubscribtionFactory<TNotification> where TNotification : class
    {
        /// <summary>
        /// Subscribe for notification events.
        /// </summary>
        /// <param name="notificationDelegate">The delegate that handlers subscription.</param>
        /// <returns>The task <see cref="IEventBusSubscription"/> 
        /// with <see cref="IDisposable"/> that can be used to remove the subscription.</returns>
        Task<IEventBusSubscription> SubscribeAsync(SubscriptionDelegate<TNotification> notificationDelegate);

        /// <summary>
        /// Subscribe for notification events.
        /// </summary>
        /// <param name="notificationDelegate">The delegate that handlers subscription.</param>
        /// <returns>The <see cref="IEventBusSubscription"/> 
        /// with <see cref="IDisposable"/> that can be used to remove the subscription.</returns>
        IEventBusSubscription SubscribeSync(SubscriptionDelegate<TNotification> notificationDelegate);
    }
}
