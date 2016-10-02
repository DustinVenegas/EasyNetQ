// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using EasyNetQ.MessageVersioning;
using EasyNetQ.Topology;
using Xunit;
using Rhino.Mocks;

namespace EasyNetQ.Tests.MessageVersioningTests
{
    public class VersionedPublishExchangeDeclareStrategyTests
    {
        [Fact]
        public void Should_declare_exchange_again_if_first_attempt_failed()
        {
            var exchangeDeclareCount = 0;
            var exchangeName = "exchangeName";

            var advancedBus = MockRepository.GenerateStrictMock<IAdvancedBus>();
            IExchange exchange = new Exchange(exchangeName);

            advancedBus
                .Expect(x => x.ExchangeDeclare(exchangeName, "topic"))
                .Throw(new Exception())
                .WhenCalled(x => exchangeDeclareCount++);

            advancedBus
                .Expect(x => x.ExchangeDeclare(exchangeName, "topic"))
                .Return(exchange)
                .WhenCalled(x => exchangeDeclareCount++);

            var publishExchangeDeclareStrategy = new VersionedPublishExchangeDeclareStrategy();
            try
            {
                publishExchangeDeclareStrategy.DeclareExchange(advancedBus, exchangeName, ExchangeType.Topic);
            }
            catch (Exception)
            {
            }
            var declaredExchange = publishExchangeDeclareStrategy.DeclareExchange(advancedBus, exchangeName, ExchangeType.Topic);
            advancedBus.AssertWasCalled(x => x.ExchangeDeclare(exchangeName, "topic"));
            advancedBus.AssertWasCalled(x => x.ExchangeDeclare(exchangeName, "topic"));
            declaredExchange.ShouldBeTheSameAs(exchange);
            exchangeDeclareCount.ShouldEqual(1);
        }


        // Unversioned message - exchange declared
        // Versioned message - superceded exchange declared, then superceding, then bind
        [Fact]
        public void When_declaring_exchanges_for_unversioned_message_one_exchange_created()
        {
            var exchanges = new List<ExchangeStub>();
            var bus = CreateAdvancedBusMock( exchanges.Add, BindExchanges, t => t.Name );

            var publishExchangeStrategy = new VersionedPublishExchangeDeclareStrategy();

            publishExchangeStrategy.DeclareExchange( bus, typeof( MyMessage ), ExchangeType.Topic );

            Assert.Collection(
                exchanges,
                (es) =>
                {
                    Assert.Equal("MyMessage", es.Name);
                    Assert.Null(es.BoundTo);
                 });
        }

        [Fact]
        public void When_declaring_exchanges_for_versioned_message_exchange_per_version_created_and_bound_to_superceding_version()
        {
            var exchanges = new List<ExchangeStub>();
            var bus = CreateAdvancedBusMock( exchanges.Add, BindExchanges, t => t.Name );
            var publishExchangeStrategy = new VersionedPublishExchangeDeclareStrategy();

            publishExchangeStrategy.DeclareExchange( bus, typeof( MyMessageV2 ), ExchangeType.Topic );

            // Two exchanges should have been created
            Assert.Collection(
                exchanges,
                (es) =>
                {
                    Assert.Equal("MyMessage", es.Name); // Superseded message exchange should been created first
                    Assert.Null(es.BoundTo); // Superseded message exchange should route messages anywhere
                },
                (es) =>
                {
                    Assert.Equal("MyMessageV2", es.Name); // Superseding message exchange should been created second
                    Assert.Equal(exchanges[0], es.BoundTo); // Superseding message exchange should route message to superseded exchange
                });
        }

        private IAdvancedBus CreateAdvancedBusMock( Action<ExchangeStub> exchangeCreated, Action<ExchangeStub, ExchangeStub> exchangeBound, Func<Type,string> nameExchange  )
        {
            var advancedBus = MockRepository.GenerateStub<IAdvancedBus>();
            advancedBus.Stub( b => b.ExchangeDeclare(null, null, false, true, false, false, null ) )
                       .IgnoreArguments()
                       .Return(null)
                       .WhenCalled( mi =>
                           {
                               var exchange = new ExchangeStub {Name = (string) mi.Arguments[ 0 ]};
                               exchangeCreated( exchange );
                               mi.ReturnValue = exchange;
                           } );

            advancedBus.Stub( b => b.Bind(Arg<IExchange>.Is.Anything, Arg<IExchange>.Is.Anything, Arg<string>.Is.Equal( "#" ) ) )
                       .Return( null )
                       .WhenCalled( mi =>
                           {
                               var source = (ExchangeStub) mi.Arguments[ 0 ];
                               var destination = (ExchangeStub) mi.Arguments[ 1 ];
                               exchangeBound( source, destination );
                               mi.ReturnValue = MockRepository.GenerateStub<IBinding>();
                           } );

            var conventions = MockRepository.GenerateStub<IConventions>();
            conventions.ExchangeNamingConvention = t => nameExchange( t );

            var container = MockRepository.GenerateStub<IContainer>();
            container.Stub( c => c.Resolve<IConventions>() ).Return( conventions );

            advancedBus.Stub( b => b.Container ).Return( container );

            return advancedBus;
        }

        private void BindExchanges( ExchangeStub source, ExchangeStub destination )
        {
            source.BoundTo = destination;
        }

        private class ExchangeStub : IExchange
        {
            public string Name { get; set; }
            public ExchangeStub BoundTo { get; set; }
        }
    }
}