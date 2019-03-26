// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Mq.Mediator.EventBus
{
    /// <summary>
    /// Factory interface that used to create <see cref="IEventBusPublisher{TNotification}"/> instance.
    /// </summary>
    public interface IEventBusPublisherFactory<TNotification> where TNotification : class
    {
        /// <summary>
        /// Creates the <see cref="IEventBusPublisher{TNotification}"/> instance.
        /// </summary>
        /// <returns>The <see cref="IEventBusPublisher{TNotification}"/> instance.</returns>
        IEventBusPublisher<TNotification> CreatePublisher();
    }

}
