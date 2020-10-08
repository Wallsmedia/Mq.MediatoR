// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Mq.Mediator.EventBus
{
    /// <summary>
    /// Delegate handles the publish notification.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    /// <param name="notification">The notification.</param>
    public delegate void SubscriptionDelegate<TNotification>(TNotification notification);
    
}
