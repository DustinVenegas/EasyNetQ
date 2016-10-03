using System;
using Xunit;
using Rhino.Mocks;

namespace EasyNetQ.Tests.Interception
{
    public class UnitTestBase : IDisposable
    {
        private static MockRepository mockRepository;

        public UnitTestBase()
        {
            mockRepository = new MockRepository();
            mockRepository.ReplayAll();
        }


        public void Dispose()
        {
            mockRepository.VerifyAll();
        }

        protected static T NewMock<T>() where T : class
        {
            var mock = mockRepository.StrictMock<T>();
            mock.Replay();
            return mock;
        }
    }
}