// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Mq.Mediator.EventBus.DependencyInjection;
using Mq.Mediator.EventBus.RabbitMQ;
using RabbitMQ.Client.Exceptions;
using Xunit;

namespace Mq.Mediator.EventBus.RubbitMQ
{

    public class EventBusPublisherFactoryTest
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
        public void SmokeTestOfDiResolved()
        {
            // setup
            void config(EventBusConfigutation _) { }
            var sp = BuildTestServiceProvider(config);

            // act
            var sc = sp.GetService<IEventBusPublisherFactory<EventBusConfigutation>>();

            // validate
            Assert.NotNull(sc);
        }

        [Fact]
        public void NotConnectedPublishThowsException()
        {
            // setup
            var msg = new EventBusConfigutation();
            void config(EventBusConfigutation c)
            {
                c.Port = 11111;
                c.RecconectCount = 6;
            }
            var sp = BuildTestServiceProvider(config);

            // act
            var sc = sp.GetService<IEventBusPublisherFactory<EventBusConfigutation>>();

            var unwrapSc = (EventBusPublisherFactory<EventBusConfigutation>.EventBusPublishProvider)sc.CreatePublisher();
            CancellationTokenSource ts = new CancellationTokenSource();
            // validate
            try
            {
                var result = unwrapSc.PublishAsync(msg, ts.Token);
                Thread.Yield();
                Thread.Sleep(5000);
                ts.Cancel();
                result.Wait();
            }
            catch (Exception ex)
            {
                if (!(ex is AggregateException) && !(ex is AggregateException))
                {
                    Assert.False(true);
                }
            }
            Assert.False(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.ErrorDisconnected, unwrapSc.BusConnectionState);
            Assert.NotNull(unwrapSc.ConnectionError);
        }

        [Fact]
        public void NotConnectedCancellationTokenThowsException()
        {
            // setup
            var msg = new EventBusConfigutation();
            void config(EventBusConfigutation c)
            {
                c.Port = 11111;
                c.RecconectCount = 1;
            }
            var sp = BuildTestServiceProvider(config);

            // act
            var sc = sp.GetService<IEventBusPublisherFactory<EventBusConfigutation>>();

            var unwrapSc = (EventBusPublisherFactory<EventBusConfigutation>.EventBusPublishProvider)sc.CreatePublisher();

            // validate
            Assert.Throws<BrokerUnreachableException>(() =>
                {
                    var result = unwrapSc.PublishSync(msg, CancellationToken.None);
                });
            Assert.False(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.ErrorDisconnected, unwrapSc.BusConnectionState);
            Assert.IsType<BrokerUnreachableException>(unwrapSc.ConnectionError);
        }

        [Fact]
        public void NotConnectedTryToConnectThowsException()
        {
            // setup
            var msg = new EventBusConfigutation();
            void config(EventBusConfigutation c)
            {
                c.Port = 11111;
                c.RecconectCount = 1;
            }
            var sp = BuildTestServiceProvider(config);

            // act
            var sc = sp.GetService<IEventBusPublisherFactory<EventBusConfigutation>>();

            var unwrapSc = (EventBusPublisherFactory<EventBusConfigutation>.EventBusPublishProvider)sc.CreatePublisher();

            // validate
            Assert.Throws<BrokerUnreachableException>(() =>
            {
                var result = unwrapSc.TryToConnect();
            });
            Assert.False(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.ErrorDisconnected, unwrapSc.BusConnectionState);
            Assert.IsType<BrokerUnreachableException>(unwrapSc.ConnectionError);
        }

        [Fact]
        public void NotConnectedPublishReturnsFlag()
        {
            // setup
            var msg = new EventBusConfigutation();
            void config(EventBusConfigutation c)
            {
                c.Port = 11111;
                c.RecconectCount = 1;
            }
            var sp = BuildTestServiceProvider(config);

            // act
            var sc = sp.GetService<IEventBusPublisherFactory<EventBusConfigutation>>();

            var unwrapSc = (EventBusPublisherFactory<EventBusConfigutation>.EventBusPublishProvider)sc.CreatePublisher();

            // validate
            var result = unwrapSc.PublishSync(msg, CancellationToken.None, true);
            Assert.False(result);
            Assert.False(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.ErrorDisconnected, unwrapSc.BusConnectionState);
            Assert.IsType<BrokerUnreachableException>(unwrapSc.ConnectionError);
        }

        [Fact]
        public void NotConnectedPublishCancellationTokenReturnsFlag()
        {
            // setup
            var msg = new EventBusConfigutation();
            void config(EventBusConfigutation c)
            {
                c.Port = 11111;
                c.RecconectCount = 6;
            }
            var sp = BuildTestServiceProvider(config);

            // act
            var sc = sp.GetService<IEventBusPublisherFactory<EventBusConfigutation>>();

            var unwrapSc = (EventBusPublisherFactory<EventBusConfigutation>.EventBusPublishProvider)sc.CreatePublisher();
            CancellationTokenSource ts = new CancellationTokenSource();

            // validate
            var result = unwrapSc.PublishAsync(msg, ts.Token, true);
            Thread.Yield();
            Thread.Sleep(5000);
            ts.Cancel();
            Assert.False(result.Result);
            Assert.False(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.ErrorDisconnected, unwrapSc.BusConnectionState);
            Assert.IsType<OperationCanceledException>(unwrapSc.ConnectionError);
        }

        [Fact]
        public void NotConnectedTryToConnectReturnsFlag()
        {
            // setup
            var msg = new EventBusConfigutation();
            void config(EventBusConfigutation c)
            {
                c.Port = 11111;
                c.RecconectCount = 1;
            }
            var sp = BuildTestServiceProvider(config);

            var sc = sp.GetService<IEventBusPublisherFactory<EventBusConfigutation>>();

            var unwrapSc = (EventBusPublisherFactory<EventBusConfigutation>.EventBusPublishProvider)sc.CreatePublisher();

            // validate
            var result = unwrapSc.TryToConnect(true);
            Assert.False(result);
            Assert.False(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.ErrorDisconnected, unwrapSc.BusConnectionState);
            Assert.IsType<BrokerUnreachableException>(unwrapSc.ConnectionError);
        }

        [Fact]
        public void ConnectedServiceAndPublish()
        {
            // setup
            var msg = new EventBusConfigutation();
            void config(EventBusConfigutation c)
            {
                c.DefaultMap.ExchangeName = nameof(EventBusPublisherFactoryTest);
            }

            var sp = BuildTestServiceProvider(config);

            var sc = sp.GetService<IEventBusPublisherFactory<EventBusConfigutation>>();

            var unwrapSc = (EventBusPublisherFactory<EventBusConfigutation>.EventBusPublishProvider)sc.CreatePublisher();
            CancellationTokenSource ts = new CancellationTokenSource();

            var result = unwrapSc.PublishSync(msg, ts.Token);

            // validate
            Assert.True(result);
            Assert.True(unwrapSc.IsConnected);
            Assert.Null(unwrapSc.ConnectionError);
            Assert.Equal(BusConnectionState.Connected, unwrapSc.BusConnectionState);

        }

        [Fact]
        public void ConnectedTryConnectAndPublish()
        {
            // setup
            var msg = new EventBusConfigutation();
            void config(EventBusConfigutation c)
            {
                c.DefaultMap.ExchangeName = nameof(EventBusPublisherFactoryTest);
            }

            var sp = BuildTestServiceProvider(config);
            // act

            var sc = sp.GetService<IEventBusPublisherFactory<EventBusConfigutation>>();


            var unwrapSc = (EventBusPublisherFactory<EventBusConfigutation>.EventBusPublishProvider)sc.CreatePublisher();
            CancellationTokenSource ts = new CancellationTokenSource();
            var result = unwrapSc.TryToConnect();

            // validate
            Assert.True(result);
            Assert.Null(unwrapSc.ConnectionError);
            Assert.True(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.Connected, unwrapSc.BusConnectionState);

            result = unwrapSc.PublishSync(msg, ts.Token);

            // validate
            Assert.True(result);
            Assert.Null(unwrapSc.ConnectionError);
            Assert.True(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.Connected, unwrapSc.BusConnectionState);

        }

        [Fact]
        public void ConnectedAutoReconnectConnectionAndPublish()
        {
            // setup
            var msg = new EventBusConfigutation();
            void config(EventBusConfigutation c)
            {
                c.DefaultMap.ExchangeName = nameof(EventBusPublisherFactoryTest);
            }

            var sp = BuildTestServiceProvider(config);
            // act

            var sc = sp.GetService<IEventBusPublisherFactory<EventBusConfigutation>>();

            var unwrapSc = (EventBusPublisherFactory<EventBusConfigutation>.EventBusPublishProvider)sc.CreatePublisher();
            CancellationTokenSource ts = new CancellationTokenSource();
            var result = unwrapSc.TryToConnect();

            // validate
            Assert.True(result);
            Assert.Null(unwrapSc.ConnectionError);
            Assert.True(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.Connected, unwrapSc.BusConnectionState);

            unwrapSc.Connection.Close();
            Thread.Yield();
            Thread.Sleep(5000);

            // validate
            Assert.True(result);
            Assert.Null(unwrapSc.ConnectionError);
            Assert.True(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.Connected, unwrapSc.BusConnectionState);


            result = unwrapSc.PublishSync(msg, ts.Token);

            // validate
            Assert.True(result);
            Assert.Null(unwrapSc.ConnectionError);
            Assert.True(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.Connected, unwrapSc.BusConnectionState);

        }

        [Fact]
        public void ConnectedAutoReconnectChannelAndPublish()
        {
            // setup
            var msg = new EventBusConfigutation();
            void config(EventBusConfigutation c)
            {
                c.DefaultMap.ExchangeName = nameof(EventBusPublisherFactoryTest);
            }

            var sp = BuildTestServiceProvider(config);
            // act
            var sc = sp.GetService<IEventBusPublisherFactory<EventBusConfigutation>>();

            var unwrapSc = (EventBusPublisherFactory<EventBusConfigutation>.EventBusPublishProvider)sc.CreatePublisher();
            CancellationTokenSource ts = new CancellationTokenSource();
            var result = unwrapSc.TryToConnect();

            // validate
            Assert.True(result);
            Assert.Null(unwrapSc.ConnectionError);
            Assert.True(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.Connected, unwrapSc.BusConnectionState);

            unwrapSc.Channel.Close();
            Thread.Yield();
            Thread.Sleep(5000);

            // validate
            Assert.Null(unwrapSc.ConnectionError);
            Assert.True(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.Connected, unwrapSc.BusConnectionState);


            result = unwrapSc.PublishSync(msg, ts.Token);

            // validate
            Assert.True(result);
            Assert.Null(unwrapSc.ConnectionError);
            Assert.True(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.Connected, unwrapSc.BusConnectionState);

        }

        [Fact]
        public void ConnectedAutoFailedReconnectChannelAndPublish()
        {
            // setup
            var msg = new EventBusConfigutation();
            void config(EventBusConfigutation c)
            {
                c.DefaultMap.ExchangeName = nameof(EventBusPublisherFactoryTest);
            }

            var sp = BuildTestServiceProvider(config);
            // act
            var sc = sp.GetService<IEventBusPublisherFactory<EventBusConfigutation>>();

            var unwrapSc = (EventBusPublisherFactory<EventBusConfigutation>.EventBusPublishProvider)sc.CreatePublisher();
            CancellationTokenSource ts = new CancellationTokenSource();
            var result = unwrapSc.TryToConnect();

            // validate
            Assert.True(result);
            Assert.Null(unwrapSc.ConnectionError);
            Assert.True(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.Connected, unwrapSc.BusConnectionState);

            // setup simulate hard disconnection
            unwrapSc.ConfigutationRabbitMQ.Port = 11111;
            unwrapSc.ConnectionFactory.GetConnectionFactory().Port = 11111;
            unwrapSc.ConfigutationRabbitMQ.RecconectCount = 2;

            try
            {
                unwrapSc.Channel.Close();
            }
            catch {/*channel disconnection simulation*/ }
            Thread.Yield();
            Thread.Sleep(5000);

            // validate
            Assert.False(unwrapSc.IsConnected);
            Assert.NotEqual(BusConnectionState.Connected, unwrapSc.BusConnectionState);

            result = unwrapSc.PublishSync(msg, ts.Token, true);

            // validate
            Assert.False(result);
            Assert.NotNull(unwrapSc.ConnectionError);
            Assert.False(unwrapSc.IsConnected);
            Assert.Equal(BusConnectionState.ErrorDisconnected, unwrapSc.BusConnectionState);

        }
    }
}
