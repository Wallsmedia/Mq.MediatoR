﻿// Copyright © Alexander Paskhin 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using Mq.Mediator.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Mq.MediatoR.Abstractions.Test.NotificationHandlers.Mock
{
    class MqPublishHandler_Processing : INotificationHandler<TestPublishNotication>
    {
        public ServicingOrder OrderInTheGroup => ServicingOrder.Processing;

        public Task ProcessNotification(TestPublishNotication request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(OrderInTheGroup.ToString());
            return Task.Delay(1000, cancellationToken);
        }
    }

}
