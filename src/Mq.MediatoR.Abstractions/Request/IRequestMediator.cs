// Copyright © Alexander Paskhin 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Mq.Mediator.Abstractions
{
    /// <summary>
    /// The request "send" mediator interface.
    /// It provides an abstraction for an internal message exchange broker.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    public interface IRequestMediator<TRequest, TResponse> where TRequest : class where TResponse : class
    {
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
        Task<TResponse>[] SendAsync(TRequest request, CancellationToken cancellationToken);
    }

}
