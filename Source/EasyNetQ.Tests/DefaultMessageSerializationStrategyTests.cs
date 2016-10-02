// ReSharper disable InconsistentNaming

using System;
using System.Text;
using Xunit;
using Rhino.Mocks;

namespace EasyNetQ.Tests
{
    public class DefaultMessageSerializationStrategyTests
    {
        [Fact]
        public void When_using_the_default_serialization_strategy_messages_are_correctly_serialized()
        {
            const string messageType = "MyMessageTypeName";
            var serializedMessageBody = Encoding.UTF8.GetBytes("Hello world!");
            const string correlationId = "CorrelationId";

            var message = new Message<MyMessage>(new MyMessage());
            var serializationStrategy = CreateSerializationStrategy(message, messageType, serializedMessageBody, correlationId);

            var serializedMessage = serializationStrategy.SerializeMessage(message);

            AssertMessageSerializedCorrectly(serializedMessage, serializedMessageBody, messageType, correlationId);
        }

        [Fact]
        public void When_serializing_a_message_with_a_correlationid_it_is_not_overwritten()
        {
            const string messageType = "MyMessageTypeName";
            var serializedMessageBody = Encoding.UTF8.GetBytes("Hello world!");
            const string correlationId = "CorrelationId";

            var message = new Message<MyMessage>(new MyMessage())
                {
                    Properties = { CorrelationId = correlationId }
                };
            var serializationStrategy = CreateSerializationStrategy(message, messageType, serializedMessageBody, "SomeOtherCorrelationId");

            var serializedMessage = serializationStrategy.SerializeMessage(message);

            AssertMessageSerializedCorrectly(serializedMessage, serializedMessageBody, messageType, correlationId);
        }

        [Fact]
        public void When_using_the_default_serialization_strategy_messages_are_correctly_deserialized()
        {
            const string messageType = "MyMessageTypeName";
            const string messageContent = "Hello world!";
            var serializedMessageBody = Encoding.UTF8.GetBytes(messageContent);
            const string correlationId = "CorrelationId";

            var message = new Message<MyMessage>(new MyMessage { Text = messageContent })
            {
                Properties =
                    {
                        Type = messageType,
                        CorrelationId = correlationId,
                        UserId = "Bob"
                    },
            };
            var serializationStrategy = CreateDeserializationStrategy(message, serializedMessageBody, correlationId);

            var deserializedMessage = serializationStrategy.DeserializeMessage(message.Properties, serializedMessageBody);

            AssertMessageDeserializedCorrectly((Message<MyMessage>)deserializedMessage, messageContent, typeof(MyMessage), message.Properties.ToString());
        }

        [Fact]
        public void When_using_the_default_serialization_strategy_messages_are_correctly_round_tripped()
        {
            var typeNameSerializer = new TypeNameSerializer();
            var serializer = new JsonSerializer(typeNameSerializer);
            const string correlationId = "CorrelationId";

            var serializationStrategy = new DefaultMessageSerializationStrategy(typeNameSerializer, serializer, new StaticCorrelationIdGenerationStrategy(correlationId));

            var messageBody = new MyMessage { Text = "Hello world!" };
            var message = new Message<MyMessage>(messageBody);
            var serializedMessage = serializationStrategy.SerializeMessage(message);
            var deserializedMessage = serializationStrategy.DeserializeMessage(serializedMessage.Properties, serializedMessage.Body);

            Assert.IsType<MyMessage>(message.Body);
            Assert.Equal(message.Body.Text, ((Message<MyMessage>)deserializedMessage).Body.Text);
        }

        private void AssertMessageSerializedCorrectly(SerializedMessage message, byte[] expectedBody, string expectedMessageType, string expectedCorrelationId)
        {
            Assert.Equal(expectedBody, message.Body);
            Assert.Equal(expectedMessageType, message.Properties.Type);
            Assert.Equal(expectedCorrelationId, message.Properties.CorrelationId);
        }

        private void AssertMessageDeserializedCorrectly(IMessage<MyMessage> message, string expectedBodyText, Type expectedMessageType, string expectedMessageProperties)
        {
            Assert.Equal(expectedBodyText, message.Body.Text);
            Assert.IsType(expectedMessageType, message.MessageType);
            Assert.Equal(expectedMessageProperties, message.Properties.ToString());
        }

        private DefaultMessageSerializationStrategy CreateSerializationStrategy(IMessage<MyMessage> message, string messageType, byte[] messageBody, string correlationId)
        {
            var typeNameSerializer = MockRepository.GenerateStub<ITypeNameSerializer>();
            typeNameSerializer.Stub(s => s.Serialize(message.MessageType)).Return(messageType);

            var serializer = MockRepository.GenerateStub<ISerializer>();
            serializer.Stub(s => s.MessageToBytes(message.GetBody())).Return(messageBody);

            return new DefaultMessageSerializationStrategy(typeNameSerializer, serializer, new StaticCorrelationIdGenerationStrategy(correlationId));
        }

        private DefaultMessageSerializationStrategy CreateDeserializationStrategy(IMessage<MyMessage> message, byte[] messageBody, string correlationId)
        {
            var typeNameSerializer = MockRepository.GenerateStub<ITypeNameSerializer>();
            typeNameSerializer.Stub(s => s.DeSerialize(message.Properties.Type)).Return(message.Body.GetType());

            var serializer = MockRepository.GenerateStub<ISerializer>();
            serializer.Stub(s => s.BytesToMessage(message.Properties.Type, messageBody)).Return(message.Body);

            return new DefaultMessageSerializationStrategy(typeNameSerializer, serializer, new StaticCorrelationIdGenerationStrategy(correlationId));
        }
    }
}