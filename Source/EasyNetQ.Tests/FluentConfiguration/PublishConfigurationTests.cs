﻿using System;
using EasyNetQ.FluentConfiguration;
using Xunit;

namespace EasyNetQ.Tests.FluentConfiguration
{
    public class PublishConfigurationTests
    {
        [Fact]
        public void Should_throw_if_default_topic_is_null()
        {
            Assert.That(() => new PublishConfiguration(null), Throws.InstanceOf<ArgumentNullException>());
        }

        [Fact]
        public void Should_return_default_topic()
        {
            var configuration = new PublishConfiguration("default");

            configuration.WithTopic(null);

            Assert.AreEqual(configuration.Topic, "default");
        }

        [Fact]
        public void Should_return_custom_topic()
        {
            var configuration = new PublishConfiguration("default");

            configuration.WithTopic("custom");

            Assert.AreEqual(configuration.Topic, "custom");
        }
    }
}