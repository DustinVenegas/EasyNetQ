﻿using System;
using Xunit;

namespace EasyNetQ.Tests
{
    public class MessageFactoryTests
    {
        [Fact]
        public void Should_correctly_create_generic_message()
        {
            var message = new MyMessage { Text = "Hello World" };

            var genericMessage = MessageFactory.CreateInstance(typeof(MyMessage), message);

            Assert.NotNull(genericMessage);
            Assert.IsInstanceOf<Message<MyMessage>>(genericMessage);
            Assert.IsInstanceOf<MyMessage>(genericMessage.GetBody());
            Assert.True(genericMessage.MessageType == typeof(MyMessage));
            Assert.True(genericMessage.CastTo<Message<MyMessage>>().Body.Text == message.Text);

            var properties = new MessageProperties { CorrelationId = Guid.NewGuid().ToString() };
            var genericMessageWithProperties = MessageFactory.CreateInstance(typeof(MyMessage), message, properties);

            Assert.NotNull(genericMessageWithProperties);
            Assert.IsInstanceOf<Message<MyMessage>>(genericMessageWithProperties);
            Assert.IsInstanceOf<MyMessage>(genericMessageWithProperties.GetBody());
            Assert.True(genericMessageWithProperties.MessageType == typeof(MyMessage));
            Assert.True(genericMessageWithProperties.CastTo<Message<MyMessage>>().Body.Text == message.Text);
            Assert.True(genericMessageWithProperties.CastTo<Message<MyMessage>>().Properties.CorrelationId == properties.CorrelationId);
        }

        [Fact]
        public void Should_fail_to_create_generic_message_with_null_argument()
        {
            Assert.Throws<ArgumentNullException>(() => MessageFactory.CreateInstance(typeof(MyMessage), null));
            Assert.Throws<ArgumentNullException>(() => MessageFactory.CreateInstance(typeof(MyMessage), new MyMessage(), null));
        }
    }
}