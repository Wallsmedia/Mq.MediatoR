// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mq.Mediator.Notification.DependencyInjection;
using Mq.Mediator.Notification.InMem;
using Mq.Mediator.Request.DependencyInjection;
using Mq.Mediator.Request.InMem;
using Xunit;


namespace Mq.Mediator.Abstractions.Test
{
    public class UnitTestOfDefaultMqSendMediatorFactory
    {

        ServiceProvider BuildTestServiceProviderNormal()
        {
            ServiceCollection sc = new ServiceCollection();
            sc.AddSendProcessingHandler<TestSendRequest, TestSendResponse > (TestSendDelegates.Process_Complete,ServicingOrder.Complete);
            sc.AddSendProcessingHandler<TestSendRequest, TestSendResponse > (TestSendDelegates.Process_Processing,ServicingOrder.Processing);
            sc.AddSendProcessingHandler<TestSendRequest, TestSendResponse > (TestSendDelegates.Process_Initialization,ServicingOrder.Initialization);
            sc.AddSendProcessingHandler<TestSendRequest, TestSendResponse > (TestSendDelegates.Process_PostProcessing,ServicingOrder.PostProcessing);
            sc.AddSendProcessingHandler<TestSendRequest, TestSendResponse > (TestSendDelegates.Process_PreProcessing,ServicingOrder.PreProcessing);
            sc.AddMqNotificationMediator();
            sc.AddMqRequestMediator();
            return sc.BuildServiceProvider();
        }

        ServiceProvider BuildTestServiceProviderException()
        {
            ServiceCollection sc = new ServiceCollection();
            sc.AddSendProcessingHandler<TestSendRequest, TestSendResponse>(TestSendDelegates.Process_Complete, ServicingOrder.Complete);
            sc.AddSendProcessingHandler<TestSendRequest, TestSendResponse>(TestSendDelegates.Process_Processing_Exception, ServicingOrder.Processing);
            sc.AddSendProcessingHandler<TestSendRequest, TestSendResponse>(TestSendDelegates.Process_Initialization, ServicingOrder.Initialization);
            sc.AddSendProcessingHandler<TestSendRequest, TestSendResponse>(TestSendDelegates.Process_PostProcessing, ServicingOrder.PostProcessing);
            sc.AddSendProcessingHandler<TestSendRequest, TestSendResponse>(TestSendDelegates.Process_PreProcessing, ServicingOrder.PreProcessing);
            sc.AddMqNotificationMediator();
            sc.AddMqRequestMediator();
            return sc.BuildServiceProvider();
        }

        ServiceProvider BuildTestServiceProviderCancel()
        {
            ServiceCollection sc = new ServiceCollection();
            sc.AddSendProcessingHandler<TestSendRequest, TestSendResponse>(TestSendDelegates.Process_Complete10, ServicingOrder.Complete);
            sc.AddSendProcessingHandler<TestSendRequest, TestSendResponse>(TestSendDelegates.Process_Processing, ServicingOrder.Processing);
            sc.AddMqNotificationMediator();
            sc.AddMqRequestMediator();
            return sc.BuildServiceProvider();
        }

        [Fact]
        public void TestOrderOfRequestSendProcessingOfExistedHandler()
        {
            // Setup
            ServiceProvider sp = BuildTestServiceProviderNormal();
            var publishMediator = sp.GetService<IRequestMediator<TestSendRequest, TestSendResponse>>();
            var rq = new TestSendRequest();

            // Action
            var tsks = publishMediator.SendAsync(rq, CancellationToken.None);

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
        public void TestOrderOfRequestSendProcessingOfExistedHandlerWithException()
        {
            // Setup
            ServiceProvider sp = BuildTestServiceProviderException();
            var publishMediator = sp.GetService<IRequestMediator<TestSendRequest, TestSendResponse>>();
            var rq = new TestSendRequest();

            // Action
            var tsks = publishMediator.SendAsync(rq, CancellationToken.None);

            Assert.Throws<AggregateException>(() => Task.WaitAll(tsks));

            // Asserts
            Assert.Equal(5, tsks.Length);
            Assert.Equal(3, rq.Visitor.Count);
            Assert.Equal(ServicingOrder.Initialization.ToString(), rq.Visitor[0]);
            Assert.Equal(ServicingOrder.PreProcessing.ToString(), rq.Visitor[1]);
            Assert.Equal(ServicingOrder.Processing.ToString(), rq.Visitor[2]);
        }

        [Fact]
        public void TestOrderOfRequestSendProcessingOfNonExistedHandler()
        {
            // Setup
            ServiceProvider sp = BuildTestServiceProviderNormal();
            var publishMediator = sp.GetService<IRequestMediator<string, string>>();
            var rq = "Test";

            // Action
            var tsks = publishMediator.SendAsync(rq, CancellationToken.None);

            Task.WaitAll(tsks);

            // Assert
            Assert.Empty(tsks);
        }

        [Fact]
        public void TestOrderOfRequestSendProcessingAndCancelled()
        {
            // Setup
            ServiceProvider sp = BuildTestServiceProviderCancel();
            var publishMediator = sp.GetService<IRequestMediator<TestSendRequest, TestSendResponse>>();
            var rq = new TestSendRequest();
            CancellationTokenSource ts = new CancellationTokenSource();

            // Action
            var tsks = publishMediator.SendAsync(rq, ts.Token);

            ts.Cancel();
            Assert.Throws<AggregateException>(() => Task.WaitAll(tsks));
            Assert.Equal(2, tsks.Length);
        }

    }

}
