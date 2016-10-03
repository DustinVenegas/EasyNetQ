// ReSharper disable InconsistentNaming

using System;
using System.Reflection;
using System.Threading;
using EasyNetQ.AutoSubscribe;
using Xunit;

namespace EasyNetQ.Tests.Integration
{
    [Trait("RabbitMQ", "Localhost")]
    public class AutoSubscriberIntegrationTests
    {
        private IBus bus;

        public AutoSubscriberIntegrationTests()
        {
            bus = RabbitHutch.CreateBus("host=localhost");
            var subscriber = new AutoSubscriber(bus, "autosub.integration");

            subscriber.Subscribe(Assembly.GetExecutingAssembly());
        }

        [TearDown]
        public void TearDown()
        {
            // give the message a chance to get devlivered
            Thread.Sleep(500);
            bus.Dispose();
        }

        [Fact]
        [Trait("RabbitMQ", "Localhost")]
        public void PublishWithTopic()
        {
            bus.Publish(new AutoSubMessage{ Text = "With topic" }, "mytopic");
        }

        [Fact]
        [Trait("RabbitMQ", "Localhost")]
        public void PublishWithoutTopic()
        {
            bus.Publish(new AutoSubMessage{ Text = "Without topic" });
        }
    }

    public class AutoSubMessage
    {
        public string Text { get; set; }
    }

    public class MyConsumer : IConsume<AutoSubMessage>
    {
        [ForTopic("mytopic")]
        public void Consume(AutoSubMessage message)
        {
            Console.Out.WriteLine("Autosubscriber got message: {0}", message.Text);
        }
    }
}

// ReSharper restore InconsistentNaming