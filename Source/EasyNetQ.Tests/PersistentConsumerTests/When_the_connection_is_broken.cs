﻿// ReSharper disable InconsistentNaming

using EasyNetQ.Events;
using Xunit;
using Rhino.Mocks;

namespace EasyNetQ.Tests.PersistentConsumerTests
{
    public class When_the_connection_is_broken : Given_a_PersistentConsumer
    {
        public override void AdditionalSetup()
        {
            persistentConnection.Stub(x => x.IsConnected).Return(true);
            consumer.StartConsuming();
            eventBus.Publish(new ConnectionCreatedEvent());
        }

        [Fact]
        public void Should_re_create_internal_consumer()
        {
            internalConsumerFactory.AssertWasCalled(x => x.CreateConsumer());
            createConsumerCalled.ShouldEqual(2);

            internalConsumers.Count.ShouldEqual(2);
        }
    }
}