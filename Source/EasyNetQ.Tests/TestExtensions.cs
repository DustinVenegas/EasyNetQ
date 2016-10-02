using System;
using System.Linq;
using Xunit;
using System.Collections.Generic;

namespace EasyNetQ.Tests
{
    public static class TestExtensions
    {
        public static T ShouldNotBeNull<T>(this T obj)
        {
            Assert.NotNull(obj);
            return obj;
        }

        public static T ShouldEqual<T>(this T actual, object expected)
        {
            Assert.Equal(expected, actual);
            return actual;
        }

        public static void ShouldBe<T>(this object actual)
        {
            Assert.IsAssignableFrom<T>(actual);
        }

        public static void ShouldBeNull(this object actual)
        {
            Assert.Null(actual);
        }

        public static void ShouldBeTheSameAs(this object actual, object expected)
        {
            Assert.Same(expected, actual);
        }

        public static T CastTo<T>(this object source)
        {
            return (T)source;
        }

        public static void ShouldBeTrue(this bool source)
        {
            Assert.True(source);
        }

        public static void ShouldBeTrue(this bool source, string message)
        {
            Assert.True(source, message);
        }

        public static void ShouldBeFalse(this bool source)
        {
            Assert.False(source);
        }

        public static void ShouldBeEmpty<T>(this IEnumerable<T> collection)
        {
            Assert.Empty(collection);
        }
    }
}