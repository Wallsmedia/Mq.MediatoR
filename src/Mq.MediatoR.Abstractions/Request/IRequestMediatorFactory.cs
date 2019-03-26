// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Mq.Mediator.Abstractions
{
    /// <summary>
    /// Used to create <see cref="IRequestMediator{TRequest, TResponse}"/> instance.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    public interface IRequestMediatorFactory<TRequest, TResponse> where TRequest : class where TResponse : class
    {
        /// <summary>
        /// Creates the <see cref="IRequestMediator{TRequest, TResponse}"/> instance.
        /// </summary>
        /// <returns>The <see cref="IRequestMediator{TRequest, TResponse}"/> instance.</returns>
        IRequestMediator<TRequest, TResponse> CreateMqMediator();
    }

}
