// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Mq.Mediator.EventBus
{
    /// <summary>
    /// Defines the common subscription interface to Event Bus.
    /// </summary>
    public interface IEventBusSubscription : IDisposable
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
        /// The event that handles subscription errors.
        /// </summary>
        event Action<Exception> ErrorHandler;

    }
}
