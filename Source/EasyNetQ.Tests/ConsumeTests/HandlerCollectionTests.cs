﻿// ReSharper disable InconsistentNaming

using EasyNetQ.Consumer;
using EasyNetQ.Loggers;
using Xunit;

namespace EasyNetQ.Tests.ConsumeTests
{
    public class HandlerCollectionTests
    {
        private IHandlerCollection handlerCollection;

        private bool myMessageHandlerExecuted = false;
        private bool animalHandlerExecuted = false;

        [SetUp]
        public void SetUp()
        {
            handlerCollection = new HandlerCollection(new NullLogger());

            handlerCollection.Add<MyMessage>((message, info) => 
                {
                    myMessageHandlerExecuted = true;
                });
            handlerCollection.Add<IAnimal>((message, info) =>
                {
                    animalHandlerExecuted = true;
                });
        }

        [Fact]
        public void Should_return_matching_handler()
        {
            var handler = handlerCollection.GetHandler<MyMessage>();

            handler(new Message<MyMessage>(new MyMessage()), null);
            myMessageHandlerExecuted.ShouldBeTrue();
        }

        [Fact]
        public void Should_return_supertype_handler()
        {
            var handler = handlerCollection.GetHandler<Dog>();

            handler(new Message<Dog>(new Dog()), null);
            animalHandlerExecuted.ShouldBeTrue();
        }

        [Fact]
        [ExpectedException(typeof(EasyNetQException))]
        public void Should_throw_if_handler_is_not_found()
        {
            handlerCollection.GetHandler<MyOtherMessage>();
        }

        [Fact]
        public void Should_return_matching_handler_by_type()
        {
            var handler = handlerCollection.GetHandler(typeof(MyMessage));

            handler(new Message<MyMessage>(new MyMessage()), null);
            myMessageHandlerExecuted.ShouldBeTrue();
        }

        [Fact]
        public void Should_return_supertype_handler_by_type()
        {
            var handler = handlerCollection.GetHandler(typeof(Dog));

            handler(new Message<Dog>(new Dog()), null);
            animalHandlerExecuted.ShouldBeTrue();
        }

        [Fact]
        public void Should_return_a_null_logger_if_ThrowOnNoMatchingHandler_is_false()
        {
            handlerCollection.ThrowOnNoMatchingHandler = false;
            var handler = handlerCollection.GetHandler<MyOtherMessage>();

            handler(new Message<MyOtherMessage>(new MyOtherMessage()), null);
            myMessageHandlerExecuted.ShouldBeFalse();
            animalHandlerExecuted.ShouldBeFalse();
        }

        [Fact]
        [ExpectedException(typeof(EasyNetQException))]
        public void Should_not_be_able_to_register_multiple_handlers_for_the_same_type()
        {
            handlerCollection.Add<MyMessage>((message, info) => { });
        }
    }
}

// ReSharper restore InconsistentNaming