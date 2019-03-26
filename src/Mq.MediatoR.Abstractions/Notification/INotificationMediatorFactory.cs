// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Mq.Mediator.Abstractions
{
    /// <summary>
    /// Defines a factory the used to create <see cref="INotificationMediator{TNotification}"/> instance.
    /// </summary>
    public interface INotificationMediatorFactory<TNotification> where TNotification : class
    {
        /// <summary>
        /// Creates the <see cref="INotificationMediator{TNotification}"/> instance.
        /// </summary>
        /// <returns>The <see cref="INotificationMediator{TNotification}"/> instance.</returns>
        INotificationMediator<TNotification> CreateMqMediator();
    }

}
