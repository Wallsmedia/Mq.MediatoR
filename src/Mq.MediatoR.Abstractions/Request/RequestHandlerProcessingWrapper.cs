// Copyright © Alexander Paskhin 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mq.Mediator.Abstractions
{
    /// <summary>
    /// The wrappers to the request processing delegate.
    /// </summary>
    /// <typeparam name="TNotification">The requesttype.</typeparam>
    public class RequestHandlerProcessingWrapper<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : class where TResponse : class
    {
        public ServicingOrder OrderInTheGroup { get; }
        private readonly RequestResponseDelegateAsync<TRequest,TResponse> _delegate;

        /// <summary>
        /// Constructs the wrapper class.
        /// </summary>
        /// <param name="requestDelegate">The processing delegate.</param>
        /// <param name="servicingOrder">The processing priority.</param>
        public RequestHandlerProcessingWrapper(RequestResponseDelegateAsync<TRequest,TResponse> requestDelegate, ServicingOrder servicingOrder = ServicingOrder.Processing)
        {
            _delegate = requestDelegate ?? throw new ArgumentNullException(nameof(requestDelegate));
            OrderInTheGroup = servicingOrder;
        }

        /// <summary>
        /// Handles the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task with a response result.</returns>
        public Task<TResponse> ProcessAsync(TRequest request, CancellationToken cancellationToken)
        {
            return _delegate(request, cancellationToken);
        }
    }
}