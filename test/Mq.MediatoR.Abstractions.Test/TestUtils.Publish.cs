// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mq.Mediator.Abstractions.Test
{
    class TestPublishNotication
    {
        public string Text { get; set; } = nameof(TestPublishNotication);
        public List<string> Visitor { get; } = new List<string>();
    }

    class MqPublishHandler_Processing10 : INotificationHandler<TestPublishNotication>
    {
        public ServicingOrder OrderInTheGroup => ServicingOrder.Processing;

        public Task ProcessNotification(TestPublishNotication request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(OrderInTheGroup.ToString());
            return Task.Delay(10 * 1000, cancellationToken);
        }
    }

    class MqPublishHandler_Processing : INotificationHandler<TestPublishNotication>
    {
        public ServicingOrder OrderInTheGroup => ServicingOrder.Processing;

        public Task ProcessNotification(TestPublishNotication request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(OrderInTheGroup.ToString());
            return Task.Delay(1000, cancellationToken);
        }
    }

    class MqPublishHandler_Processing_Exception : INotificationHandler<TestPublishNotication>
    {
        public ServicingOrder OrderInTheGroup => ServicingOrder.Processing;

        public Task ProcessNotification(TestPublishNotication request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(OrderInTheGroup.ToString());
            throw new NullReferenceException("Test");
        }
    }

    class MqPublishHandler_Initilization : INotificationHandler<TestPublishNotication>
    {

        public ServicingOrder OrderInTheGroup => ServicingOrder.Initialization;

        public Task ProcessNotification(TestPublishNotication request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(OrderInTheGroup.ToString());
            return Task.Delay(1000, cancellationToken);
        }
    }

    class MqPublishHandler_PreProcessing : INotificationHandler<TestPublishNotication>
    {
        public ServicingOrder OrderInTheGroup => ServicingOrder.PreProcessing;

        public Task ProcessNotification(TestPublishNotication request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(OrderInTheGroup.ToString());
            return Task.Delay(1000, cancellationToken);
        }
    }

    class MqPublishHandler_PostProcessing : INotificationHandler<TestPublishNotication>
    {
        public ServicingOrder OrderInTheGroup => ServicingOrder.PostProcessing;

        public Task ProcessNotification(TestPublishNotication request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(OrderInTheGroup.ToString());
            return Task.Delay(1000, cancellationToken);
        }
    }

    class MqPublishHandler_Complete : INotificationHandler<TestPublishNotication>
    {
        public ServicingOrder OrderInTheGroup => ServicingOrder.Complete;

        public Task ProcessNotification(TestPublishNotication request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(OrderInTheGroup.ToString());
            return Task.Delay(1000, cancellationToken);
        }
    }

    class MqPublishHandler_Complete10 : INotificationHandler<TestPublishNotication>
    {
        public ServicingOrder OrderInTheGroup => ServicingOrder.Complete;

        public Task ProcessNotification(TestPublishNotication request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(OrderInTheGroup.ToString());
            return Task.Delay(10*1000, cancellationToken);
        }
    }

}
