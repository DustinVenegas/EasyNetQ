// ReSharper disable InconsistentNaming

using EasyNetQ.Scheduling;
using EasyNetQ.Tests.Interception;
using Xunit;
using Rhino.Mocks;

namespace EasyNetQ.Tests.Scheduling
{
    [TestFixture]
    public class InterceptionExtensionsTests : UnitTestBase
    {
        [Fact]
        public void When_using_EnableDelayedExchangeScheduler_extension_method_required_services_are_registered()
        {
            var serviceRegister = NewMock<IServiceRegister>();
            serviceRegister.Expect(x => x.Register<IScheduler, DelayedExchangeScheduler>()).TentativeReturn();
            serviceRegister.EnableDelayedExchangeScheduler();
        }

        [Fact]
        public void When_using_EnableDeadLetterExchangeAndMessageTtlScheduler_extension_method_required_services_are_registered()
        {
            var serviceRegister = NewMock<IServiceRegister>();
            serviceRegister.Expect(x => x.Register<IScheduler, DeadLetterExchangeAndMessageTtlScheduler>()).TentativeReturn();
            serviceRegister.EnableDeadLetterExchangeAndMessageTtlScheduler();
        }
    }
}