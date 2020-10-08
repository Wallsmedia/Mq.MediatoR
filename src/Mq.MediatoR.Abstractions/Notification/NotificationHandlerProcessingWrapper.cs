// Copyright © Alexander Paskhin 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mq.Mediator.Abstractions
{
    /// <summary>
    /// The wrappers to the notification processing delegate.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    public class NotificationHandlerProcessingWrapper<TNotification> : INotificationHandler<TNotification> where TNotification : class
    {
        public ServicingOrder OrderInTheGroup { get; }
        private readonly NotificationDelegateAsync<TNotification> _delegate;

        /// <summary>
        /// Constructs the wrapper class.
        /// </summary>
        /// <param name="notificationDelegate">The processing delegate.</param>
        /// <param name="servicingOrder">The processing priority.</param>
        public NotificationHandlerProcessingWrapper(NotificationDelegateAsync<TNotification> notificationDelegate, ServicingOrder servicingOrder = ServicingOrder.Processing)
        {
            _delegate = notificationDelegate ?? throw new ArgumentNullException(nameof(notificationDelegate));
            OrderInTheGroup = servicingOrder;
        }


        /// <summary>
        /// Handles the notification.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task which is completed when a notification has been processed.</returns>
        public Task ProcessNotification(TNotification request, CancellationToken cancellationToken)
        {
            return _delegate(request, cancellationToken);
        }
    }
}