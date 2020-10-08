// Copyright © Alexander Paskhin 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Mq.Mediator.EventBus
{
    /// <summary>
    /// Defines the event  bus connection states.
    /// </summary>
    public enum BusConnectionState
    {
        NotCalledOrInitalized,
        Connecting,
        Connected,
        ErrorDisconnected,
        Disposed
    }
}
