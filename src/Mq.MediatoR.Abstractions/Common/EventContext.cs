// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Mq.Mediator.EventBus
{
    /// <summary>
    /// The container for sending a message context
    /// </summary>
    /// <typeparam name="TContext">The type of context.</typeparam>
    public class EventContext<TContext>
    {

        /// <summary>
        /// The container Id.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The creation date of the container.
        /// </summary>
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The container context.
        /// </summary>
        public TContext Context { get; set; }

    }
}
