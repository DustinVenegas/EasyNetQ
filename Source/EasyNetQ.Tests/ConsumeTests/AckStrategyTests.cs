﻿using EasyNetQ.Consumer;
using EasyNetQ.Events;
using Xunit;
using RabbitMQ.Client;
using Rhino.Mocks;

namespace EasyNetQ.Tests.ConsumeTests
{
    public class Ack_strategy
    {
        private IModel model;
        private AckResult result;
        private const ulong deliveryTag = 1234;

        public Ack_strategy()
        {
            model = MockRepository.GenerateStrictMock<IModel>();
            model.Expect(m => m.BasicAck(deliveryTag, false));

            result = AckStrategies.Ack(model, deliveryTag);
        }


        [Fact]
        public void Should_ack_message()
        {
            model.VerifyAllExpectations();
        }

        [Fact]
        public void Should_return_Ack()
        {
            Assert.Equal(AckResult.Ack, result);
        } 
    }

    public class NackWithoutRequeue_strategy
    {
        private IModel model;
        private AckResult result;
        private const ulong deliveryTag = 1234;

        public NackWithoutRequeue_strategy()
        {
            model = MockRepository.GenerateStrictMock<IModel>();
            model.Expect(m => m.BasicNack(deliveryTag, false, false));

            result = AckStrategies.NackWithoutRequeue(model, deliveryTag);
        }


        [Fact]
        public void Should_nack_message_and_not_requeue()
        {
            model.VerifyAllExpectations();   
        }

        [Fact]
        public void Should_return_Nack()
        {
            Assert.Equal(AckResult.Nack, result);
        }
    }

    public class NackWithRequeue_strategy
    {
        private IModel model;
        private AckResult result;
        private const ulong deliveryTag = 1234;

        public NackWithRequeue_strategy()
        {
            model = MockRepository.GenerateStrictMock<IModel>();
            model.Expect(m => m.BasicNack(deliveryTag, false, true));

            result = AckStrategies.NackWithRequeue(model, deliveryTag);
        }


        [Fact]
        public void Should_nack_message_and_requeue()
        {
            model.VerifyAllExpectations();
        }

        [Fact]
        public void Should_return_Nack()
        {
            Assert.Equal(AckResult.Nack, result);
        }
    }

    public class Nothing_strategy
    {
        private IModel model;
        private AckResult result;
        private const ulong deliveryTag = 1234;

        public Nothing_strategy()
        {
            model = MockRepository.GenerateStrictMock<IModel>();

            result = AckStrategies.Nothing(model, deliveryTag);
        }

        [Fact]
        public void Should_have_no_interaction_with_model()
        {
            model.VerifyAllExpectations();
        }

        [Fact]
        public void Should_return_Nothing()
        {
            Assert.Equal(AckResult.Nothing, result);
        }
    }
}