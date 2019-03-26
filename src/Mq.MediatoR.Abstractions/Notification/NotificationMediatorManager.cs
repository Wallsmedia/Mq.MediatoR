// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Mq.Mediator.Abstractions
{

    /// <summary>
    /// Implements the generic mediator manager. It is an abstraction to the implementation
    /// that will be provided by <see cref="INotificationMediatorFactory{TNotification}"/> factory.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    public class NotificationMediatorManager<TNotification > : INotificationMediator<TNotification> where TNotification : class 
    {
        readonly INotificationMediator<TNotification> _mediator;

        /// <summary>
        /// Initialize instance with specifics configuration.
        /// </summary>
        /// <param name="factory">The mediator factory.</param>
        public NotificationMediatorManager(INotificationMediatorFactory<TNotification> factory)
        {
            _mediator = factory.CreateMqMediator();
        }

        /// <summary>
        /// Publish the notification for processing by the abstract set of <see cref="INotificationHandler{TRequest}"/>
        /// handlers. It returns the array of the completion tasks.
        /// The published notification has been processed in the grouped by <see cref="ServicingOrder"/> order.
        /// If there is more than one handler's group, so groups completed synchronously; i.e. there is a wait between groups.
        /// There is no timeout processing, so it should be provided in <see cref="INotificationHandler{TRequest}"/> implementation.
        /// </summary>
        /// <param name="notification">The send notification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The array of tasks that indicates processing completion.</returns>
        public Task[] PublishAsync(TNotification notification, CancellationToken cancellationToken)
        {
            return _mediator.PublishAsync(notification, cancellationToken);
        }

      
    }

}
