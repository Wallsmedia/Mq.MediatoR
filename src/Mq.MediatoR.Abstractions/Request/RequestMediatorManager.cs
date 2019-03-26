// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Mq.Mediator.Abstractions
{
    /// <summary>
    /// Implements the generic mediator manager. It is an abstraction to the implementation
    /// that will be provided by <see cref="IRequestMediatorFactory{TRequest, TResponse}"/> factory.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    public class RequestMediatorManager<TRequest, TResponse> : IRequestMediator<TRequest, TResponse> where TRequest : class where TResponse : class
    {
        readonly IRequestMediator<TRequest, TResponse> _mediator;

        /// <summary>
        /// Initialize instance with specifics configuration.
        /// </summary>
        /// <param name="factory">The mediator factory.</param>
        public RequestMediatorManager(IRequestMediatorFactory<TRequest, TResponse> factory)
        {
            _mediator = factory.CreateMqMediator();
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
        /// <returns>The array of tasks that indicates a processing completion.</returns>
        public Task<TResponse>[] SendAsync(TRequest request, CancellationToken cancellationToken)
        {
            return _mediator.SendAsync(request, cancellationToken);
        }
    }

}
