// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Mq.Mediator.Abstractions
{
    /// <summary>
    /// The delegate that handles a publish notification.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    /// <param name="notification">The notification.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task which is completed when a notification has been processed.</returns>
    public delegate Task NotificationDelegateAsync<TNotification>(TNotification notification, CancellationToken cancellationToken);
}