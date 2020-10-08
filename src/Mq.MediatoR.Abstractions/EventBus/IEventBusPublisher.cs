// Copyright © Alexander Paskhin 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mq.Mediator.EventBus
{

    /// <summary>
    /// The notification event bus publishing interface with monitoring extensions.
    /// </summary>
    /// <typeparam name="TNotification"></typeparam>
    public interface IEventBusPublisher<TNotification> : IDisposable where TNotification : class
    {
        /// <summary>
        /// The current connection state to event bus.
        /// </summary>
        BusConnectionState BusConnectionState { get; }

        /// <summary>
        /// The connection error.
        /// </summary>
        Exception ConnectionError { get; }

        /// <summary>
        /// Tries to connect to the event bus.
        /// </summary>
        /// <param name="returnErrorFlag">If it's true the method returns error flag but not throw an exception.</param>
        /// <exception>The wide range.</exception>
        /// <returns>The connection success flag.</returns>
        bool TryToConnect(bool returnErrorFlag = false);

        /// <summary>
        /// Publishes a notification over the bus.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="returnErrorFlag">If it's true the method returns error flag but not throw an exception.</param>
        /// <exception>The wide range.</exception>
        /// <returns>The task of notification to publish with flag of success.
        /// Returns Task with false if there is no a subscription handler.</returns>
        Task<bool> PublishAsync(TNotification notification, CancellationToken cancellationToken, bool returnErrorFlag = false);

        /// <summary>
        /// Publishes a notification over the bus.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="returnErrorFlag">If it's true the method returns error flag but not throw an exception.</param>
        /// <exception>The wide range.</exception>
        /// <returns>The task of notification to publish with flag of success.
        /// Returns Task with false if there is no a subscription handler.</returns>
        bool PublishSync(TNotification notification, CancellationToken cancellationToken, bool returnErrorFlag = false);
    }
}
