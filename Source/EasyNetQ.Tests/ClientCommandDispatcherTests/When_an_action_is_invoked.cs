// ReSharper disable InconsistentNaming

using System;
using System.Threading;
using EasyNetQ.ConnectionString;
using EasyNetQ.Producer;
using Xunit;
using RabbitMQ.Client;
using Rhino.Mocks;

namespace EasyNetQ.Tests.ClientCommandDispatcherTests
{
    public class When_an_action_is_invoked : IDisposable
    {
        private IClientCommandDispatcher dispatcher;
        private IPersistentChannel channel;
        private bool actionWasInvoked;
        private string actionThreadName;

        public When_an_action_is_invoked()
        {
            actionWasInvoked = false;
            actionThreadName = "Not set";

            var parser = new ConnectionStringParser();
            var configuration = parser.Parse("host=localhost");
            var connection = MockRepository.GenerateStub<IPersistentConnection>();
            var channelFactory = MockRepository.GenerateStub<IPersistentChannelFactory>();
            channel = MockRepository.GenerateStub<IPersistentChannel>();

            Action<IModel> action = x =>
                {
                    actionWasInvoked = true;
                    actionThreadName = Thread.CurrentThread.Name;
                };

            channelFactory.Stub(x => x.CreatePersistentChannel(connection)).Return(channel);
            channel.Stub(x => x.InvokeChannelAction(null)).IgnoreArguments().WhenCalled(
                x => ((Action<IModel>)x.Arguments[0])(null));

            dispatcher = new ClientCommandDispatcher(configuration, connection, channelFactory);

            dispatcher.InvokeAsync(action).Wait();
        }

        public void Dispose()
        {
            dispatcher.Dispose();
        }

        [Fact]
        public void Should_create_a_persistent_channel()
        {
            channel.ShouldNotBeNull();
        }

        [Fact]
        public void Should_invoke_the_action()
        {
            actionWasInvoked.ShouldBeTrue();
        }

        [Fact]
        public void Should_invoke_the_action_on_the_dispatcher_thread()
        {
            actionThreadName.ShouldEqual("Client Command Dispatcher Thread");
        }
    }
}

// ReSharper restore InconsistentNaming