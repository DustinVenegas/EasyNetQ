﻿// ReSharper disable InconsistentNaming

using System.Collections.Generic;
using EasyNetQ.Tests.Mocking;
using Xunit;
using RabbitMQ.Client;
using Rhino.Mocks;

namespace EasyNetQ.Tests.ProducerTests
{
    public class When_a_message_is_sent
    {
        private MockBuilder mockBuilder;
        private const string queueName = "the_queue_name";

        public When_a_message_is_sent()
        {
            mockBuilder = new MockBuilder();

            mockBuilder.Bus.Send(queueName, new MyMessage { Text = "Hello World" });
        }

        [Fact]
        public void Should_publish_the_message()
        {
            mockBuilder.Channels[0].AssertWasCalled(x => x.BasicPublish(
                Arg<string>.Is.Equal(""),
                Arg<string>.Is.Equal(queueName),
                Arg<bool>.Is.Equal(false),
                Arg<IBasicProperties>.Is.Anything,
                Arg<byte[]>.Is.Anything));
        }

        [Fact]
        public void Should_declare_the_queue()
        {
            mockBuilder.Channels[0].AssertWasCalled(x => x.QueueDeclare(
                Arg<string>.Is.Equal(queueName),
                Arg<bool>.Is.Equal(true),
                Arg<bool>.Is.Equal(false),
                Arg<bool>.Is.Equal(false),
                Arg<IDictionary<string, object>>.Is.Anything));
        }
    }
}

// ReSharper restore InconsistentNaming