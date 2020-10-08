// Copyright © Alexander Paskhin 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Mq.Mediator.EventBus.DependencyInjection;
using Mq.Mediator.EventBus.RabbitMQ;
using Xunit;

namespace Mq.Mediator.EventBus.RubbitMQ
{
    public class EventBusSubscribeFactoryTest
    {
        ServiceProvider BuildTestServiceProvider(Action<EventBusConfigutation> configOptions)
        {
            ServiceCollection sc = new ServiceCollection();
            sc.AddEventBusPublisherRabbitMQ();
            sc.AddEventBusSubscriberRabbitMQ();
            //sc.AddLogging();
            sc.Configure<EventBusConfigutation>(configOptions);
            return sc.BuildServiceProvider();
        }

        [Fact]
        public void SmokeTestDiContainerResolved()
        {
            // setup
            void config(EventBusConfigutation _) { }
            var sp = BuildTestServiceProvider(config);


            // act
            var scf = sp.GetService<IEventBusSubscribtionFactory<EventBusConfigutation>>();

            // validate
            Assert.NotNull(scf);
        }



        [Fact]
        public void SubscribeAndPublishAndGetMessage()
        {
            EventBusConfigutation msgResponse = null;
            Exception error = null;

            // setup
            void SubscriptionDelegate(EventBusConfigutation notification)
            {
                msgResponse = notification;
            }

            void LocalErrorHandler(Exception obj)
            {
                error = obj;
            }

            void config(EventBusConfigutation c)
            {
                c.DefaultMap.ExchangeName = nameof(EventBusSubscribeFactoryTest) + "1";
                c.DefaultMap.QueueName = nameof(EventBusSubscribeFactoryTest) + "1";
            }


            var msg = new EventBusConfigutation();

            var sp = BuildTestServiceProvider(config);

            // act
            var pb = sp.GetRequiredService<IEventBusPublisherFactory<EventBusConfigutation>>().CreatePublisher();
            Assert.NotNull(pb);

            var scf = sp.GetRequiredService<IEventBusSubscribtionFactory<EventBusConfigutation>>();
            // validate
            Assert.NotNull(scf);

            // act
            var subScr = (EventBusSubscriberFactory<EventBusConfigutation>.EventBusSubscriberProvider)scf.SubscribeSync(SubscriptionDelegate);
            subScr.ErrorHandler += LocalErrorHandler;

            pb.PublishSync(msg, CancellationToken.None);

            Thread.Yield();
            Thread.Sleep(1000);

            Assert.NotNull(msgResponse);
            Assert.Null(error);

        }

        [Fact]
        public void SubscribeAutoReConnectAndPublishAndGetMessage()
        {
            EventBusConfigutation msgResponse = null;
            Exception error = null;

            // setup
            void SubscriptionDelegate(EventBusConfigutation notification)
            {
                msgResponse = notification;
            }

            void LocalErrorHandler(Exception obj)
            {
                error = obj;
            }

            void config(EventBusConfigutation c)
            {
                c.DefaultMap.ExchangeName = nameof(EventBusSubscribeFactoryTest) + "2";
                c.DefaultMap.QueueName = nameof(EventBusSubscribeFactoryTest) + "2";
            }


            var msg = new EventBusConfigutation();

            var sp = BuildTestServiceProvider(config);

            // act
            var pb = sp.GetRequiredService<IEventBusPublisherFactory<EventBusConfigutation>>().CreatePublisher();
            Assert.NotNull(pb);

            var scf = sp.GetService<IEventBusSubscribtionFactory<EventBusConfigutation>>();
            // validate
            Assert.NotNull(scf);

            // act
            var subScr = (EventBusSubscriberFactory<EventBusConfigutation>.EventBusSubscriberProvider)scf.SubscribeSync(SubscriptionDelegate);
            subScr.ErrorHandler += LocalErrorHandler;

            subScr.Connection.Close();

            for (int i = 0; i < 30; i++)
            {
                Thread.Yield();
                Thread.Sleep(1000);
                if (subScr.IsConnected) break;
            }

            Assert.True(subScr.IsConnected);
            Assert.Equal(BusConnectionState.Connected, subScr.BusConnectionState);
            Assert.Null(subScr.ConnectionError);
            //Assert.IsType<OperationCanceledException>(subScr.ConnectionError);

            pb.PublishSync(msg, CancellationToken.None);

            Thread.Yield();
            Thread.Sleep(1000);

            Assert.NotNull(msgResponse);
            Assert.Null(error);

        }

        [Fact]
        public void SubscribeAutoReConnectChannelAndPublishAndGetMessage()
        {
            EventBusConfigutation msgResponse = null;
            Exception error = null;

            // setup
            void SubscriptionDelegate(EventBusConfigutation notification)
            {
                msgResponse = notification;
            }

            void LocalErrorHandler(Exception obj)
            {
                error = obj;
            }

            void config(EventBusConfigutation c)
            {
                c.DefaultMap.ExchangeName = nameof(EventBusSubscribeFactoryTest) + "3";
                c.DefaultMap.QueueName = nameof(EventBusSubscribeFactoryTest) + "3";
            }


            var msg = new EventBusConfigutation();

            var sp = BuildTestServiceProvider(config);

            // act
            var pb = sp.GetRequiredService<IEventBusPublisherFactory<EventBusConfigutation>>().CreatePublisher();
            Assert.NotNull(pb);

            var scf = sp.GetService<IEventBusSubscribtionFactory<EventBusConfigutation>>();
            // validate
            Assert.NotNull(scf);

            // act
            var subScr = (EventBusSubscriberFactory<EventBusConfigutation>.EventBusSubscriberProvider)scf.SubscribeSync(SubscriptionDelegate);
            subScr.ErrorHandler += LocalErrorHandler;

            subScr.Channel.Close();

            for (int i = 0; i < 30; i++)
            {
                Thread.Yield();
                Thread.Sleep(1000);
                if (subScr.IsConnected) break;
            }

            Assert.True(subScr.IsConnected);
            Assert.Equal(BusConnectionState.Connected, subScr.BusConnectionState);
            Assert.Null(subScr.ConnectionError);
            //Assert.IsType<OperationCanceledException>(subScr.ConnectionError);

            pb.PublishSync(msg, CancellationToken.None);

            Thread.Yield();
            Thread.Sleep(1000);

            Assert.NotNull(msgResponse);
            Assert.Null(error);

        }


        [Fact]
        public void SubscribeDisConnectChannelAndPublishAndGetMessage()
        {
            EventBusConfigutation msgResponse = null;
            Exception error = null;

            // setup
            void SubscriptionDelegate(EventBusConfigutation notification)
            {
                msgResponse = notification;
            }

            void LocalErrorHandler(Exception obj)
            {
                error = obj;
            }

            void config(EventBusConfigutation c)
            {
                c.DefaultMap.ExchangeName = nameof(EventBusSubscribeFactoryTest) + "4";
                c.DefaultMap.QueueName = nameof(EventBusSubscribeFactoryTest) + "4";
                c.RecconectCount = 2;
            }


            var msg = new EventBusConfigutation();

            var sp = BuildTestServiceProvider(config);

            // act
            var pb = sp.GetRequiredService<IEventBusPublisherFactory<EventBusConfigutation>>().CreatePublisher();
            Assert.NotNull(pb);

            var scf = sp.GetService<IEventBusSubscribtionFactory<EventBusConfigutation>>();
            // validate
            Assert.NotNull(scf);

            // act
            var subScr = (EventBusSubscriberFactory<EventBusConfigutation>.EventBusSubscriberProvider)scf.SubscribeSync(SubscriptionDelegate);
            subScr.ErrorHandler += LocalErrorHandler;

            subScr.ConfigutationRabbitMQ.Port = 11111;
            subScr.ConnectionFactory.GetConnectionFactory().Port = 11111;
            subScr.ConfigutationRabbitMQ.RecconectCount = 1;

            try
            {
                subScr.Channel.Close();
            }
            catch {/*channel disconnection simulation*/ }

            Thread.Yield();
            Thread.Sleep(5000);

            Assert.False(subScr.IsConnected);
            Assert.Equal(BusConnectionState.ErrorDisconnected, subScr.BusConnectionState);
            Assert.NotNull(subScr.ConnectionError);

            Assert.NotNull(error);

        }


    }
}
