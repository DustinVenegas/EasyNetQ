﻿// ReSharper disable InconsistentNaming

using System.Linq;
using EasyNetQ.ConnectionString;
using Xunit;

namespace EasyNetQ.Tests
{
    [TestFixture]
    public class ConnectionStringTests
    {
        const string connectionStringValue =
            "host=192.168.1.1:1001,my.little.host:1002;virtualHost=Copa;username=Copa;" + 
            "password=abc_xyz;port=12345;requestedHeartbeat=3";
        private ConnectionConfiguration connectionString;

        private ConnectionConfiguration defaults;

        [SetUp]
        public void SetUp()
        {
            connectionString = new ConnectionStringParser().Parse(connectionStringValue);
            defaults = new ConnectionStringParser().Parse("host=localhost");
        }

        [Fact]
        public void Should_parse_host()
        {
            connectionString.Hosts.First().Host.ShouldEqual("192.168.1.1");
        }

        [Fact]
        public void Should_parse_host_port()
        {
            connectionString.Hosts.First().Port.ShouldEqual(1001);
        }

        [Fact]
        public void Should_parse_second_host()
        {
            connectionString.Hosts.Last().Host.ShouldEqual("my.little.host");
        }

        [Fact]
        public void Should_parse_seond_port()
        {
            connectionString.Hosts.Last().Port.ShouldEqual(1002);
        }

        [Fact]
        public void Should_parse_virtualHost()
        {
            connectionString.VirtualHost.ShouldEqual("Copa");
        }

        [Fact]
        public void Should_parse_username()
        {
            connectionString.UserName.ShouldEqual("Copa");
        }

        [Fact]
        public void Should_parse_password()
        {
            connectionString.Password.ShouldEqual("abc_xyz");
        }

        [Fact, ExpectedException(typeof(EasyNetQException))]
        public void Should_throw_on_malformed_string()
        {
            new ConnectionStringParser().Parse("not a well formed name value pair;");
        }

        [Fact, ExpectedException(typeof(EasyNetQException))]
        public void Should_fail_if_host_is_not_present()
        {
            new ConnectionStringParser().Parse(
                "virtualHost=Copa;username=Copa;password=abc_xyz;port=12345;requestedHeartbeat=3");
        }

        [Fact]
        public void Should_parse_port()
        {
            connectionString.Port.ShouldEqual(12345);
        }

        [Fact]
        public void Should_parse_heartbeat()
        {
            connectionString.RequestedHeartbeat.ShouldEqual(3);
        }

        [Fact]
        public void Should_parse_host_only()
        {
            defaults.Hosts.First().Host.ShouldEqual("localhost");
        }

        [Fact]
        public void Should_set_default_port()
        {
            defaults.Port.ShouldEqual(5672);
        }

        [Fact]
        public void Should_set_default_virtual_host()
        {
            defaults.VirtualHost.ShouldEqual("/");
        }

        [Fact]
        public void Should_set_default_username()
        {
            defaults.UserName.ShouldEqual("guest");

        }

        [Fact]
        public void Should_set_default_password()
        {
            defaults.Password.ShouldEqual("guest");
        }

        [Fact]
        public void Should_set_default_requestHeartbeat()
        {
            defaults.RequestedHeartbeat.ShouldEqual(10);
        }

        [Fact]
        public void Should_not_have_case_sensitive_keys()
        {
            const string connectionStringAlternateCasing =
                "Host=192.168.1.1:1001,my.little.host:1002;VirtualHost=Copa;UserName=Copa;" +
                "Password=abc_xyz;Port=12345;RequestedHeartbeat=3";

            var parsed = new ConnectionStringParser().Parse(connectionStringAlternateCasing);
            parsed.Hosts.First().Host.ShouldEqual("192.168.1.1");
            parsed.Hosts.First().Port.ShouldEqual(1001);
            parsed.VirtualHost.ShouldEqual("Copa");
            parsed.UserName.ShouldEqual("Copa");
            parsed.Password.ShouldEqual("abc_xyz");
            parsed.Port.ShouldEqual(12345);
            parsed.RequestedHeartbeat.ShouldEqual(3);
        }
    }
}

// ReSharper restore InconsistentNaming