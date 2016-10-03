// ReSharper disable InconsistentNaming

using System;
using EasyNetQ.Tests.Mocking;
using Xunit;
using Rhino.Mocks;
using System.Threading.Tasks;

namespace EasyNetQ.Tests.ProducerTests
{
    public class When_a_request_is_sent_but_the_connection_closes_before_a_reply_is_received
    {
        private MockBuilder mockBuilder;

        public When_a_request_is_sent_but_the_connection_closes_before_a_reply_is_received()
        {
            mockBuilder = new MockBuilder();
        }

        [Fact]
        public async Task Should_throw_an_EasyNetQException()
        {
            var task = mockBuilder.Bus.RequestAsync<TestRequestMessage, TestResponseMessage>(new TestRequestMessage());
            mockBuilder.Connection.Raise(x => x.ConnectionShutdown += null, null, null);
            await Assert.ThrowsAsync<EasyNetQException>(() => task);
        }         
    }
}

// ReSharper restore InconsistentNaming