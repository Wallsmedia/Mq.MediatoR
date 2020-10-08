// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using Mq.Mediator.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mq.MediatoR.InMem.Test.RequestHandlers.Mock
{

    public static class TestSendDelegates
    {
        public static async Task<TestSendResponse> Process_Initialization(TestSendRequest request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(ServicingOrder.Initialization.ToString());
            await Task.Delay(1000, cancellationToken);
            return new TestSendResponse() { Response = ServicingOrder.Initialization.ToString() };
        }

        public static async Task<TestSendResponse> Process_PreProcessing(TestSendRequest request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(ServicingOrder.PreProcessing.ToString());
            await Task.Delay(1000, cancellationToken);
            return new TestSendResponse() { Response = ServicingOrder.PreProcessing.ToString() };
        }
        public static async Task<TestSendResponse> Process_Processing(TestSendRequest request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(ServicingOrder.Processing.ToString());
            await Task.Delay(1000, cancellationToken);
            return new TestSendResponse() { Response = ServicingOrder.Processing.ToString() };
        }
        public static async Task<TestSendResponse> Process_Processing_Exception(TestSendRequest request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(ServicingOrder.Processing.ToString());
            throw new NullReferenceException("Test");
        }
        public static async Task<TestSendResponse> Process_PostProcessing(TestSendRequest request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(ServicingOrder.PostProcessing.ToString());
            await Task.Delay(1000, cancellationToken);
            return new TestSendResponse() { Response = ServicingOrder.PostProcessing.ToString() };
        }
        public static async Task<TestSendResponse> Process_Complete(TestSendRequest request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(ServicingOrder.Complete.ToString());
            await Task.Delay(1000, cancellationToken);
            return new TestSendResponse() { Response = ServicingOrder.Complete.ToString() };
        }
        public static async Task<TestSendResponse> Process_Complete10(TestSendRequest request, CancellationToken cancellationToken)
        {
            request.Visitor.Add(ServicingOrder.Complete.ToString());
            await Task.Delay(10 * 1000, cancellationToken);
            return new TestSendResponse() { Response = ServicingOrder.Complete.ToString() };
        }

    }

}
