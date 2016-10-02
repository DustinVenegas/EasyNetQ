using System.Collections.Generic;
using RabbitMQ.Client.Framing;
// ReSharper disable InconsistentNaming
using RabbitMQ.Client;
using Rhino.Mocks;
using System;
using System.Text;
using EasyNetQ.Tests.Mocking;
using Xunit;
using System.Threading.Tasks;

namespace EasyNetQ.Tests.ProducerTests
{
    public class When_a_request_is_sent_but_an_exception_is_thrown_by_responder
    {
        private MockBuilder mockBuilder;
        private TestRequestMessage requestMessage;
        private string _correlationId;

        [SetUp]
        public void SetUp()
        {
            mockBuilder = new MockBuilder();

            requestMessage = new TestRequestMessage();

            mockBuilder.NextModel.Stub(x => x.BasicPublish(null, null, false, null, null))
                       .IgnoreArguments()
                       .WhenCalled(invocation =>
                       {
                           var properties = (IBasicProperties)invocation.Arguments[3];
                           _correlationId = properties.CorrelationId;
                       });
        }

        [Fact]
        public async Task Should_throw_an_EasyNetQResponderException()
        {
            var task = mockBuilder.Bus.RequestAsync<TestRequestMessage, TestResponseMessage>(requestMessage);
            DeliverMessage(_correlationId, null);
            await Assert.ThrowsAsync<EasyNetQResponderException>(() => task);
        }

        [Fact]
        public async Task Should_throw_an_EasyNetQResponderException_with_a_specific_exception_message()
        {
            var task = mockBuilder.Bus.RequestAsync<TestRequestMessage, TestResponseMessage>(requestMessage);
            DeliverMessage(_correlationId, "Why you are so bad with me?");
            var actualEx = await Assert.ThrowsAsync<EasyNetQResponderException>(() => task);
            Assert.Equal("Why you are so bad with me?", actualEx.Message);
        }

        protected void DeliverMessage(string correlationId, string exceptionMessage)
        {
            var properties = new BasicProperties
            {
                Type = "EasyNetQ.Tests.TestResponseMessage:EasyNetQ.Tests.Messages",
                CorrelationId = correlationId,
                Headers = new Dictionary<string, object>
                {
                    { "IsFaulted", true }
                }
            };

            if (exceptionMessage != null)
            {
                // strings are implicitly convertered in byte[] from RabbitMQ client
                // but not convertered back in string
                // check the source code in the class RabbitMQ.Client.Impl.WireFormatting
                properties.Headers.Add("ExceptionMessage", Encoding.UTF8.GetBytes(exceptionMessage));
            }

            var body = Encoding.UTF8.GetBytes("{}");

            mockBuilder.Consumers[0].HandleBasicDeliver(
                "consumer_tag",
                0,
                false,
                "the_exchange",
                "the_routing_key",
                properties,
                body
                );
        }
    }
}

// ReSharper restore InconsistentNaming