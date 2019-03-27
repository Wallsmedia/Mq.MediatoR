// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mq.Mediator.Abstractions;

namespace Mq.Mediator.Notification.InMem
{
    /// <summary>
    /// Default implementation of the <see cref="INotificationMediatorFactory{TNotification}"/>.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    public class InMemNotificationMediatorFactory<TNotification> : INotificationMediatorFactory<TNotification> where TNotification : class
    {
        readonly IEnumerable<INotificationHandler<TNotification>> _handlers;
        INotificationMediator<TNotification> _cached;

        /// <summary>
        /// Constructs the instance of the class.
        /// </summary>
        /// <param name="handlers">The list of notification handlers.</param>
        public InMemNotificationMediatorFactory(IEnumerable<INotificationHandler<TNotification>> handlers)
        {
            _handlers = handlers;
        }

        /// <summary>
        /// Creates the <see cref="INotificationMediator{TNotification}"/> instance.
        /// </summary>
        /// <returns>The <see cref="INotificationMediator{TNotification}"/> instance.</returns>
        public INotificationMediator<TNotification> CreateMqMediator()
        {
            if (_cached == null)
            {
                int publishSize = 0;
                SortedList<ServicingOrder, List<INotificationHandler<TNotification>>> executionPublishSequence = new SortedList<ServicingOrder, List<INotificationHandler<TNotification>>>(_handlers.Count());
                foreach (var item in _handlers)
                {
                    publishSize++;
                    if (executionPublishSequence.ContainsKey(item.OrderInTheGroup))
                    {
                        executionPublishSequence[item.OrderInTheGroup].Add(item);
                    }
                    else
                    {
                        executionPublishSequence[item.OrderInTheGroup] = new List<INotificationHandler<TNotification>>() { item };
                    }
                }
                _cached = new MqPublishMediatorProvider(executionPublishSequence, publishSize);
            }

            return _cached;
        }

        private class MqPublishMediatorProvider : INotificationMediator<TNotification>
        {
            private readonly CancellationTokenSource cancelled = new CancellationTokenSource();
            readonly SortedList<ServicingOrder, List<INotificationHandler<TNotification>>> _executionPublishSequence;
            readonly int _publishSize;
            public MqPublishMediatorProvider(SortedList<ServicingOrder, List<INotificationHandler<TNotification>>> executionPublishSequence, int publishSize)
            {
                _publishSize = publishSize;
                _executionPublishSequence = executionPublishSequence;
                cancelled.Cancel();
            }

            /// <summary>
            /// Publish the notification for processing by the abstract set of <see cref="INotificationHandler{TRequest}"/>
            /// handlers. It returns the array of the completion tasks.
            /// The published notifications have been processed in the grouped <see cref="ServicingOrder"/> order.
            /// If more than one handler's group, so groups completed synchronously; i.e. there is a wait between groups.
            /// There is no timeout processing, so it should be provided in <see cref="INotificationHandler{TRequest}"/> implementation.
            /// </summary>
            /// <param name="notification">The send notification.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>The list of tasks for notification publish handlers.</returns>
            public Task[] PublishAsync(TNotification notification, CancellationToken cancellationToken)
            {
                if (_publishSize == 0)
                {
                    return new Task[] { };
                }

                var result = new Task[_publishSize];

                int indexRes = 0;
                int indexGrp = 0;
                int iG = 0;
                bool forceTheCancellation = false;

                foreach (var group in _executionPublishSequence)
                {
                    var list = group.Value;
                    // execute in parallel
                    foreach (var handler in group.Value)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            result[indexRes] = Task.FromCanceled(cancellationToken);
                        }
                        else if (forceTheCancellation)
                        {
                            result[indexRes] = Task.FromCanceled(cancelled.Token);
                        }
                        else
                        {
                            try
                            {
                                result[indexRes] = handler.ProcessNotification(notification, cancellationToken);
                            }
                            catch (Exception ae)
                            {
                                result[indexRes] = Task.FromException(ae);
                                forceTheCancellation = true;
                            }
                        }
                        indexRes++;
                    }

                    if (++iG == _executionPublishSequence.Count)
                    {
                        // In most cases it is a single group, so we are to return the tasks
                        break;
                    }

                    // Have to wait before the processing the next group in the execution sequence 

                    if (!cancellationToken.IsCancellationRequested && !forceTheCancellation)
                    {
                        if (list.Count > 1)
                        {
                            ArraySegment<Task> waitList = new ArraySegment<Task>(result, indexGrp, list.Count);
                            try
                            {
                                Task.WhenAll(waitList).Wait(cancellationToken);
                            }
                            catch
                            {
                                forceTheCancellation = true;
                            }
                        }
                        else
                        {

                            try
                            {
                                result[indexGrp].Wait(cancellationToken);
                            }
                            catch
                            {
                                forceTheCancellation = true;
                            }
                        }
                    }
                    indexGrp += list.Count;
                }

                return result;
            }
        }
    }
}

