﻿// ReSharper disable InconsistentNaming

using System;
using EasyNetQ.ConnectionString;
using EasyNetQ.Producer;
using Xunit;
using RabbitMQ.Client;
using Rhino.Mocks;

namespace EasyNetQ.Tests.ClientCommandDispatcherTests
{
    public class When_an_action_is_invoked_that_throws : IDisposable
    {
        private IClientCommandDispatcher dispatcher;
        private IPersistentChannel channel;

        public When_an_action_is_invoked_that_throws()
        {
            var parser = new ConnectionStringParser();
            var configuration = parser.Parse("host=localhost");
            var connection = MockRepository.GenerateStub<IPersistentConnection>();
            var channelFactory = MockRepository.GenerateStub<IPersistentChannelFactory>();
            channel = MockRepository.GenerateStub<IPersistentChannel>();

            channelFactory.Stub(x => x.CreatePersistentChannel(connection)).Return(channel);
            channel.Stub(x => x.InvokeChannelAction(null)).IgnoreArguments().WhenCalled(
                x => ((Action<IModel>)x.Arguments[0])(null));

            dispatcher = new ClientCommandDispatcher(configuration, connection, channelFactory);

        }

        public void Dispose()
        {
            dispatcher.Dispose();
        }

        [Fact]
        public void Should_raise_the_exception_on_the_calling_thread()
        {
            var exception = new CrazyTestOnlyException();
            
            var task = dispatcher.InvokeAsync(x =>
            {
                throw exception;
            });

            try
            {
                task.Wait();
            }
            catch (AggregateException aggregateException)
            {
                aggregateException.InnerException.ShouldBeTheSameAs(exception);
            }
        }

        private class CrazyTestOnlyException : Exception { }
    }
}

// ReSharper restore InconsistentNaming