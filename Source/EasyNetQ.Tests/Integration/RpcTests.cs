﻿using System;
using System.Threading;
using EasyNetQ.Loggers;
using EasyNetQ.Tests.ProducerTests.Very.Long.Namespace.Certainly.Longer.Than.The255.Char.Length.That.RabbitMQ.Likes.That.Will.Certainly.Cause.An.AMQP.Exception.If.We.Dont.Do.Something.About.It.And.Stop.It.From.Happening;
using Xunit;

namespace EasyNetQ.Tests.Integration
{
    [Explicit("Requires a RabbitMQ instance on localhost")]
    public class RpcTests
    {
        private class RpcRequest
        {
            public int Value { get; set; }
        }

        private class RpcResponse
        {
            public int Value { get; set; }
        }

        private IBus bus;

        [SetUp]
        public void SetUp()
        {
            bus = RabbitHutch.CreateBus("host=localhost", x => x.Register<IEasyNetQLogger>(_ => new ConsoleLogger()));
        }

        [TearDown]
        public void TearDown()
        {
            bus.Dispose();
        }

        [Fact]
        public void Should_be_able_to_publish_and_receive_response()
        {
            bus.Respond<RpcRequest, RpcResponse>(req => new RpcResponse { Value = req.Value });
            var request = new RpcRequest { Value = 5 };
            var response = bus.Request<RpcRequest, RpcResponse>(request);

            Assert.NotNull(response);
            Assert.True(request.Value == response.Value);
        }

        [Fact]
        public void Should_throw_when_requesting_over_long_message()
        {
            Exception actualEx = null;
            try
            {
                bus.Respond<MessageWithVeryVEryVEryLongNameThatWillMostCertainlyBreakAmqpsSilly255CharacterNameLimitThatIsAlmostCertainToBeReachedWithGenericTypes, RpcRequest>(
                    req => new RpcRequest());

                bus.Request<MessageWithVeryVEryVEryLongNameThatWillMostCertainlyBreakAmqpsSilly255CharacterNameLimitThatIsAlmostCertainToBeReachedWithGenericTypes, RpcRequest>(
                    new MessageWithVeryVEryVEryLongNameThatWillMostCertainlyBreakAmqpsSilly255CharacterNameLimitThatIsAlmostCertainToBeReachedWithGenericTypes());

                Thread.Sleep(2000);
            }
            catch(Exception ex)
            {
                // This is strange. Wouldn't the second call never throw in this test? Should these be separate tests?
                Assert.IsType<EasyNetQException>(ex);
                actualEx = ex;
            }
            Assert.NotNull(actualEx); // Expected an exception and never got one
        }

        [Fact]
        public void Should_throw_when_responding_to_over_long_message()
        {
            Exception actualEx = null;
            try
            {
                bus.Respond<RpcRequest, MessageWithVeryVEryVEryLongNameThatWillMostCertainlyBreakAmqpsSilly255CharacterNameLimitThatIsAlmostCertainToBeReachedWithGenericTypes>(
                    req => new MessageWithVeryVEryVEryLongNameThatWillMostCertainlyBreakAmqpsSilly255CharacterNameLimitThatIsAlmostCertainToBeReachedWithGenericTypes());

                bus.Request<RpcRequest, MessageWithVeryVEryVEryLongNameThatWillMostCertainlyBreakAmqpsSilly255CharacterNameLimitThatIsAlmostCertainToBeReachedWithGenericTypes>(
                    new RpcRequest());

                Thread.Sleep(2000);
            }
            catch(Exception ex)
            {
                // This is strange. Wouldn't the second call never throw in this test? Should these be separate tests?
                Assert.IsType<EasyNetQException>(ex);
                actualEx = ex;
            }
            Assert.NotNull(actualEx); // Expected an exception and never got one
        }
    }
}