// ReSharper disable InconsistentNaming

using System;
using System.Threading.Tasks;
using EasyNetQ.Tests.Mocking;
using Xunit;

namespace EasyNetQ.Tests.ProducerTests
{
    public class When_a_request_is_sent_but_no_reply_is_received
    {
        private MockBuilder mockBuilder;

        [SetUp]
        public void SetUp()
        {
            mockBuilder = new MockBuilder("host=localhost;timeout=1");
        }

        [Fact]
        public async Task Should_throw_a_timeout_exception()
        {
            await Assert.ThrowsAsync<TimeoutException>(() => mockBuilder.Bus.RequestAsync<TestRequestMessage, TestResponseMessage>(new TestRequestMessage()));
        }         
    }
}

// ReSharper restore InconsistentNaming