// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mq.Mediator.Abstractions;

namespace Mq.MediatoR.Request.InMem
{
    /// <summary>
    /// Extension methods for adding Mq.Mediator services to the DI container.
    /// </summary>
    public static class MqMediatoRRequestInMemExtensions
    {
       

        /// <summary>
        /// Adds services required for using of Request Mq.Mediator.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddMqRequestMediator(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAdd(ServiceDescriptor.Singleton(typeof(IRequestMediator<,>), typeof(RequestMediatorManager<,>)));
            services.TryAdd(ServiceDescriptor.Singleton(typeof(IRequestMediatorFactory<,>), typeof(InMemRequestMediatorFactory<,>)));
            return services;
        }


        /// <summary>
        /// Add delegate of <see cref="RequestResponseDelegateAsync{TRequest, TResponse}"/> to the services collection.
        /// </summary>
        /// <typeparam name="TRequest">The request type</typeparam>
        /// <typeparam name="TResponse">The response time.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="requestDelegate">The instance of the publish processing delegate.</param>
        /// <param name="servicingOrder">The order of the processing.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddSendProcessingHandler<TRequest, TResponse>(this IServiceCollection services, RequestResponseDelegateAsync<TRequest, TResponse> requestDelegate, ServicingOrder servicingOrder = ServicingOrder.Processing) where TRequest : class where TResponse : class
        {
            if (requestDelegate == null)
            {
                throw new ArgumentNullException(nameof(requestDelegate));
            }
            services.AddSingleton<IRequestHandler<TRequest, TResponse>>(new RequestHandlerProcessingWrapper<TRequest, TResponse>(requestDelegate, servicingOrder));
            return services;
        }

    }
}
