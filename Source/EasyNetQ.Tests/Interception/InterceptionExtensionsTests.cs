// ReSharper disable InconsistentNaming

using System;
using Xunit;
using EasyNetQ.Interception;
using Rhino.Mocks;

namespace EasyNetQ.Tests.Interception
{
    public partial class InterceptionExtensionsTests : UnitTestBase
    {
        

        [Fact]
        public void When_using_EnableInterception_extension_method_required_services_are_registered()
        {
            var serviceRegister = NewMock<IServiceRegister>();
            serviceRegister.Expect(x => x.Register(Arg<Func<IServiceProvider, IProduceConsumeInterceptor>>.Is.Anything)).TentativeReturn();
            serviceRegister.EnableInterception(x => { });
        }

        [Fact]
        public void When_using_EnableGZipCompression_extension_method_required_interceptor_is_added()
        {
            var interceptorRegistrator = NewMock<IInterceptorRegistrator>();
            interceptorRegistrator.Expect(x => x.Add(Arg<GZipInterceptor>.Is.TypeOf)).TentativeReturn();
            interceptorRegistrator.EnableGZipCompression();
        }


        [Fact]
        public void When_using_EnableTripleDESEncryption_extension_method_required_interceptor_is_added()
        {
            var interceptorRegistrator = NewMock<IInterceptorRegistrator>();
            interceptorRegistrator.Expect(x => x.Add(Arg<TripleDESInterceptor>.Is.TypeOf)).TentativeReturn();
            interceptorRegistrator.EnableTripleDESEncryption(new byte[0], new byte[0]);
        }
    }
}