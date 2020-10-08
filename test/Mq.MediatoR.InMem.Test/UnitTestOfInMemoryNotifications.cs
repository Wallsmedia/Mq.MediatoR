// Copyright © Alexander Paskhin 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mq.Mediator.Notification.DependencyInjection;
using Mq.Mediator.Request.DependencyInjection;
using Mq.MediatoR.Abstractions.Test.NotificationHandlers.Mock;
using Xunit;

namespace Mq.Mediator.Abstractions.Test
{



    public class UnitTestOfInMemoryNotifications
    {

        ServiceProvider BuildTestServiceProviderNormal()
        {
            ServiceCollection sc = new ServiceCollection();
            sc.AddSingleton<INotificationHandler<TestPublishNotication>, MqPublishHandler_Processing>();
            sc.AddSingleton<INotificationHandler<TestPublishNotication>, MqPublishHandler_PreProcessing>();
            sc.AddSingleton<INotificationHandler<TestPublishNotication>, MqPublishHandler_Complete>();
            sc.AddSingleton<INotificationHandler<TestPublishNotication>, MqPublishHandler_Initilization>();
            sc.AddSingleton<INotificationHandler<TestPublishNotication>, MqPublishHandler_PostProcessing>();
            sc.AddMqNotificationMediator();
            sc.AddMqRequestMediator();
            return sc.BuildServiceProvider();
        }

        ServiceProvider BuildTestServiceProviderException()
        {
            ServiceCollection sc = new ServiceCollection();
            sc.AddSingleton<INotificationHandler<TestPublishNotication>, MqPublishHandler_Processing_Exception>();
            sc.AddSingleton<INotificationHandler<TestPublishNotication>, MqPublishHandler_PreProcessing>();
            sc.AddSingleton<INotificationHandler<TestPublishNotication>, MqPublishHandler_Complete>();
            sc.AddSingleton<INotificationHandler<TestPublishNotication>, MqPublishHandler_Initilization>();
            sc.AddSingleton<INotificationHandler<TestPublishNotication>, MqPublishHandler_PostProcessing>();
            sc.AddMqNotificationMediator();
            sc.AddMqRequestMediator();
            return sc.BuildServiceProvider();
        }

        ServiceProvider BuildTestServiceProviderCancel()
        {
            ServiceCollection sc = new ServiceCollection();
            sc.AddSingleton<INotificationHandler<TestPublishNotication>, MqPublishHandler_Processing>();
            sc.AddSingleton<INotificationHandler<TestPublishNotication>, MqPublishHandler_Complete10>();
            sc.AddMqNotificationMediator();
            sc.AddMqRequestMediator();
            return sc.BuildServiceProvider();
        }


        [Fact]
        public void TestOrderOfRequestPublishProcessingOfExistedHandler()
        {
            // Setup
            ServiceProvider sp = BuildTestServiceProviderNormal();
            var publishMediator = sp.GetService<INotificationMediator<TestPublishNotication>>();
            var rq = new TestPublishNotication();

            // Action
            var tsks = publishMediator.PublishAsync(rq, CancellationToken.None);

            Task.WaitAll(tsks);

            // Asserts
            Assert.Equal(5, tsks.Length);
            Assert.Equal(5, rq.Visitor.Count);
            Assert.Equal(ServicingOrder.Initialization.ToString(), rq.Visitor[0]);
            Assert.Equal(ServicingOrder.PreProcessing.ToString(), rq.Visitor[1]);
            Assert.Equal(ServicingOrder.Processing.ToString(), rq.Visitor[2]);
            Assert.Equal(ServicingOrder.PostProcessing.ToString(), rq.Visitor[3]);
            Assert.Equal(ServicingOrder.Complete.ToString(), rq.Visitor[4]);
        }

        [Fact]
        public void TestOrderOfRequestPublishProcessingOfExistedHandlerWithException()
        {
            // Setup
            ServiceProvider sp = BuildTestServiceProviderException();
            var publishMediator = sp.GetService<INotificationMediator<TestPublishNotication>>();
            var rq = new TestPublishNotication();

            // Action
            var tsks = publishMediator.PublishAsync(rq, CancellationToken.None);

            Assert.Throws<AggregateException>(() => Task.WaitAll(tsks));

            // Asserts
            Assert.Equal(5, tsks.Length);
            Assert.Equal(3, rq.Visitor.Count);
            Assert.Equal(ServicingOrder.Initialization.ToString(), rq.Visitor[0]);
            Assert.Equal(ServicingOrder.PreProcessing.ToString(), rq.Visitor[1]);
            Assert.Equal(ServicingOrder.Processing.ToString(), rq.Visitor[2]);
        }

        [Fact]
        public void TestOrderOfRequestPublishProcessingOfNonExistedHandler()
        {
            // Setup
            ServiceProvider sp = BuildTestServiceProviderNormal();
            var publishMediator = sp.GetService<INotificationMediator<string>>();
            var rq = "test";

            // Action
            var tsks = publishMediator.PublishAsync(rq, CancellationToken.None);
            Task.WaitAll(tsks);

            // Assert
            Assert.Empty(tsks);
        }

        [Fact]
        public void TestOrderOfRequestPublishProcessingAndCancelled()
        {
            // Setup
            ServiceProvider sp = BuildTestServiceProviderCancel();
            var publishMediator = sp.GetService<INotificationMediator<TestPublishNotication>>();
            var rq = new TestPublishNotication();
            CancellationTokenSource ts = new CancellationTokenSource();

            // Action
            var tsks = publishMediator.PublishAsync(rq, ts.Token);

            ts.Cancel();
            Assert.Throws<AggregateException>(() => Task.WaitAll(tsks));
            Assert.Equal(2, tsks.Length);
        }

    }

}
