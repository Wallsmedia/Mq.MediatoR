// Copyright © Alexander Paskhin 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Mq.Mediator.Abstractions
{
    /// <summary>
    /// Defines the priority of processing requests with the same closed generic type.
    /// </summary>
    public enum ServicingOrder
    {

        Initialization = -2,
        PreProcessing = -1,
        Processing = 0,
        PostProcessing = 1,
        Complete = 2
    }

}
