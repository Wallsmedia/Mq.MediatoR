// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Mq.Mediator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mq.MediatoR.Request.InMem
{
    /// <summary>
    /// The default implementation of the <see cref="IRequestMediatorFactory{TRequest, TResponse}"/>.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    public class InMemRequestMediatorFactory<TRequest, TResponse> : IRequestMediatorFactory<TRequest, TResponse> where TRequest : class where TResponse : class
    {
        readonly IEnumerable<IRequestHandler<TRequest, TResponse>> _handlers;
        IRequestMediator<TRequest, TResponse> _cached;


        /// <summary>
        /// Constructs the instance of the class.
        /// </summary>
        /// <param name="handlers">The list of handlers.</param>
        public InMemRequestMediatorFactory(IEnumerable<IRequestHandler<TRequest, TResponse>> handlers)
        {
            _handlers = handlers;
        }

        /// <summary>
        /// Creates the <see cref="IRequestMediator{TRequest, TResponse}"/> instance.
        /// </summary>
        /// <returns>The <see cref="IRequestMediator{TRequest, TResponse}"/> instance.</returns>
        public IRequestMediator<TRequest, TResponse> CreateMqMediator()
        {
            if (_cached == null)
            {
                int sendSize = 0;
                SortedList<ServicingOrder, List<IRequestHandler<TRequest, TResponse>>> executionSendSequence = new SortedList<ServicingOrder, List<IRequestHandler<TRequest, TResponse>>>(_handlers.Count());
                foreach (var item in _handlers)
                {
                    sendSize++;
                    if (executionSendSequence.ContainsKey(item.OrderInTheGroup))
                    {
                        executionSendSequence[item.OrderInTheGroup].Add(item);
                    }
                    else
                    {
                        executionSendSequence[item.OrderInTheGroup] = new List<IRequestHandler<TRequest, TResponse>>() { item };
                    }

                }
                _cached = new MqSendMediatorProvider(executionSendSequence, sendSize);
            }

            return _cached;
        }

        private class MqSendMediatorProvider : IRequestMediator<TRequest, TResponse>
        {
            private readonly CancellationTokenSource cancelled = new CancellationTokenSource();
            readonly SortedList<ServicingOrder, List<IRequestHandler<TRequest, TResponse>>> _executionSendSequence;
            readonly int _sendSize;

            public MqSendMediatorProvider(SortedList<ServicingOrder, List<IRequestHandler<TRequest, TResponse>>> executionSendSequence, int sendSize)
            {
                _sendSize = sendSize;
                _executionSendSequence = executionSendSequence;
                cancelled.Cancel();
            }

            /// <summary>
            /// Sends the request for processing by the abstract set of <see cref="IRequestHandler{TRequest, TResponse}"/>
            /// handlers. It returns the array of the responses.
            /// The request has been processed in the grouped by <see cref="ServicingOrder"/> order.
            /// If there is more than one priority group, so groups completed synchronously; i.e. there is a wait between groups.
            /// There is no timeout processing, so it should be provided in <see cref="IRequestHandler{TRequest, TResponse}"/> implementation.
            /// </summary>
            /// <param name="request">The send request.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>The array of tasks that indicates processing completion.</returns>
            public Task<TResponse>[] SendAsync(TRequest request, CancellationToken cancellationToken)
            {
                if (_sendSize == 0)
                {
                    return new Task<TResponse>[] { };
                }

                var result = new Task<TResponse>[_sendSize];

                int indexRes = 0;
                int indexGrp = 0;
                int iG = 0;
                bool forceTheCancellation = false;

                foreach (var group in _executionSendSequence)
                {
                    var list = group.Value;
                    /* execute in parallel */
                    foreach (var handler in group.Value)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            result[indexRes] = Task.FromCanceled<TResponse>(cancellationToken);
                        }
                        else if (forceTheCancellation)
                        {
                            result[indexRes] = Task.FromCanceled<TResponse>(cancelled.Token);
                        }
                        else
                        {
                            try
                            {
                                result[indexRes] = handler.ProcessAsync(request, cancellationToken);
                            }
                            catch (Exception ae)
                            {
                                result[indexRes] = Task.FromException<TResponse>(ae);
                                forceTheCancellation = true;
                            }
                        }
                        indexRes++;
                    }

                    if (++iG == _executionSendSequence.Count)
                    {
                        // In most cases it is a single group, so we are to return the tasks
                        break;
                    }

                    if (!cancellationToken.IsCancellationRequested && !forceTheCancellation)
                    {
                        if (list.Count > 1)
                        {
                            ArraySegment<Task<TResponse>> waitList = new ArraySegment<Task<TResponse>>(result, indexGrp, list.Count);
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
