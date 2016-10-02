// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Text;
using EasyNetQ.MessageVersioning;
using Xunit;
using Rhino.Mocks;

namespace EasyNetQ.Tests.MessageVersioningTests
{
    public class VersionedMessageSerializationStrategyTests
    {
        private const string AlternativeMessageTypesHeaderKey = "Alternative-Message-Types";

        [Fact]
        public void When_using_the_versioned_serialization_strategy_messages_are_correctly_serialized()
        {
            const string messageType = "MyMessageTypeName";
            var serializedMessageBody = Encoding.UTF8.GetBytes("Hello world!");
            const string correlationId = "CorrelationId";
            var messageTypes = new Dictionary<string, Type> { { messageType, typeof(MyMessage) } };

            var message = new Message<MyMessage>(new MyMessage());
            var serializationStrategy = CreateSerializationStrategy(message, messageTypes, serializedMessageBody, correlationId);

            var serializedMessage = serializationStrategy.SerializeMessage(message);

            AssertMessageSerializedCorrectly(serializedMessage, serializedMessageBody, p => AssertDefaultMessagePropertiesCorrect(p, messageType, correlationId));
        }

        [Fact]
        public void When_serializing_a_message_with_a_correlationid_it_is_not_overwritten()
        {
            const string messageType = "MyMessageTypeName";
            var serializedMessageBody = Encoding.UTF8.GetBytes("Hello world!");
            const string correlationId = "CorrelationId";
            var messageTypes = new Dictionary<string, Type> { { messageType, typeof(MyMessage) } };

            var message = new Message<MyMessage>(new MyMessage())
            {
                Properties = { CorrelationId = correlationId }
            };
            var serializationStrategy = CreateSerializationStrategy(message, messageTypes, serializedMessageBody, "SomeOtherCorrelationId");

            var serializedMessage = serializationStrategy.SerializeMessage(message);

            AssertMessageSerializedCorrectly(serializedMessage, serializedMessageBody, p => AssertDefaultMessagePropertiesCorrect(p, messageType, correlationId));
        }

        [Fact]
        public void When_using_the_versioned_serialization_strategy_messages_are_correctly_deserialized()
        {
            const string messageType = "MyMessageTypeName";
            const string messageContent = "Hello world!";
            var serializedMessageBody = Encoding.UTF8.GetBytes(messageContent);
            const string correlationId = "CorrelationId";
            var messageTypes = new Dictionary<string, Type> { { messageType, typeof(MyMessage) } };

            var message = new Message<MyMessage>(new MyMessage { Text = messageContent })
            {
                Properties =
                {
                    Type = messageType,
                    CorrelationId = correlationId,
                    UserId = "Bob"
                },
            };
            var serializationStrategy = CreateDeserializationStrategy(message.Body, messageTypes, messageType, serializedMessageBody);

            var deserializedMessage = serializationStrategy.DeserializeMessage(message.Properties, serializedMessageBody);

            AssertMessageDeserializedCorrectly((Message<MyMessage>)deserializedMessage, messageContent, typeof(MyMessage), p => AssertDefaultMessagePropertiesCorrect(p, messageType, correlationId));
            Assert.Equal(message.Properties.UserId, deserializedMessage.Properties.UserId);
        }

        [Fact]
        public void When_using_the_versioned_serialization_strategy_messages_are_correctly_round_tripped()
        {
            var typeNameSerializer = new TypeNameSerializer();
            var serializer = new JsonSerializer(typeNameSerializer);

            const string correlationId = "CorrelationId";

            var serializationStrategy = new VersionedMessageSerializationStrategy(typeNameSerializer, serializer, new StaticCorrelationIdGenerationStrategy(correlationId));

            var messageBody = new MyMessage { Text = "Hello world!" };
            var message = new Message<MyMessage>(messageBody);
            var serializedMessage = serializationStrategy.SerializeMessage(message);
            var deserializedMessage = serializationStrategy.DeserializeMessage(serializedMessage.Properties, serializedMessage.Body);

            Assert.Equal(message.Body.GetType(), deserializedMessage.MessageType);
            Assert.Equal(message.Body.Text, ((Message<MyMessage>)deserializedMessage).Body.Text);
        }

        [Fact]
        public void When_using_the_versioned_serialization_strategy_versioned_messages_are_correctly_serialized()
        {
            const string messageType = "MyMessageV2TypeName";
            const string supersededMessageType = "MyMessageTypeName";
            var serializedMessageBody = Encoding.UTF8.GetBytes("Hello world!");
            const string correlationId = "CorrelationId";
            var messageTypes = new Dictionary<string, Type>
                {
                    {messageType, typeof( MyMessageV2 )},
                    {supersededMessageType, typeof( MyMessage )}
                };

            var message = new Message<MyMessageV2>(new MyMessageV2());
            var serializationStrategy = CreateSerializationStrategy(message, messageTypes, serializedMessageBody, correlationId);

            var serializedMessage = serializationStrategy.SerializeMessage(message);

            AssertMessageSerializedCorrectly(serializedMessage, serializedMessageBody, p => AssertVersionedMessagePropertiesCorrect(p, messageType, correlationId, supersededMessageType));
        }

        [Fact]
        public void When_serializing_a_versioned_message_with_a_correlationid_it_is_not_overwritten()
        {
            const string messageType = "MyMessageV2TypeName";
            const string supersededMessageType = "MyMessageTypeName";
            var serializedMessageBody = Encoding.UTF8.GetBytes("Hello world!");
            const string correlationId = "CorrelationId";
            var messageTypes = new Dictionary<string, Type>
                {
                    {messageType, typeof( MyMessageV2 )},
                    {supersededMessageType, typeof( MyMessage )}
                };

            var message = new Message<MyMessageV2>(new MyMessageV2())
                {
                    Properties = { CorrelationId = correlationId }
                };
            var serializationStrategy = CreateSerializationStrategy(message, messageTypes, serializedMessageBody, "SomeOtherCorrelationId");

            var serializedMessage = serializationStrategy.SerializeMessage(message);

            AssertMessageSerializedCorrectly(serializedMessage, serializedMessageBody, p => AssertVersionedMessagePropertiesCorrect(p, messageType, correlationId, supersededMessageType));
        }

        [Fact]
        public void When_using_the_versioned_serialization_strategy_versioned_messages_are_correctly_deserialized()
        {
            const string messageType = "MyMessageV2TypeName";
            const string supersededMessageType = "MyMessageTypeName";
            const string messageContent = "Hello world!";
            var serializedMessageBody = Encoding.UTF8.GetBytes(messageContent);
            const string correlationId = "CorrelationId";
            var messageTypes = new Dictionary<string, Type>
                {
                    {messageType, typeof( MyMessageV2 )},
                    {supersededMessageType, typeof( MyMessage )}
                };

            var message = new Message<MyMessageV2>(new MyMessageV2 { Text = messageContent })
            {
                Properties =
                {
                    Type = messageType,
                    CorrelationId = correlationId,
                    UserId = "Bob",
                },
            };
            message.Properties.Headers.Add("Alternative-Message-Types", Encoding.UTF8.GetBytes(supersededMessageType));
            var serializationStrategy = CreateDeserializationStrategy(message.Body, messageTypes, messageType, serializedMessageBody);

            var deserializedMessage = serializationStrategy.DeserializeMessage(message.Properties, serializedMessageBody);

            AssertMessageDeserializedCorrectly((Message<MyMessageV2>)deserializedMessage, messageContent, typeof(MyMessageV2), p => AssertVersionedMessagePropertiesCorrect(p, messageType, correlationId, supersededMessageType));
        }

        [Fact]
        public void When_using_the_versioned_serialization_strategy_versioned_messages_are_correctly_round_tripped()
        {
            var typeNameSerializer = new TypeNameSerializer();
            var serializer = new JsonSerializer(typeNameSerializer);
            const string correlationId = "CorrelationId";

            var serializationStrategy = new VersionedMessageSerializationStrategy(typeNameSerializer, serializer, new StaticCorrelationIdGenerationStrategy(correlationId));

            var messageBody = new MyMessageV2 { Text = "Hello world!", Number = 5 };
            var message = new Message<MyMessageV2>(messageBody);
            var serializedMessage = serializationStrategy.SerializeMessage(message);

            // RMQ converts the Header values into a byte[] so mimic the translation here
            var alternativeMessageHeader = (string)serializedMessage.Properties.Headers[AlternativeMessageTypesHeaderKey];
            serializedMessage.Properties.Headers[AlternativeMessageTypesHeaderKey] = Encoding.UTF8.GetBytes(alternativeMessageHeader);

            var deserializedMessage = serializationStrategy.DeserializeMessage(serializedMessage.Properties, serializedMessage.Body);

            Assert.Equal(message.Body.GetType(), deserializedMessage.MessageType);
            Assert.Equal(message.Body.Text, ((Message<MyMessageV2>)deserializedMessage).Body.Text);
            Assert.Equal(message.Body.Number, ((Message<MyMessageV2>)deserializedMessage).Body.Number);
        }

        [Fact]
        public void When_deserializing_versioned_message_use_first_available_message_type()
        {
            var typeNameSerializer = new TypeNameSerializer();
            var serializer = new JsonSerializer(typeNameSerializer);
            const string correlationId = "CorrelationId";

            var serializationStrategy = new VersionedMessageSerializationStrategy(typeNameSerializer, serializer, new StaticCorrelationIdGenerationStrategy(correlationId));

            var messageBody = new MyMessageV2 { Text = "Hello world!", Number = 5 };
            var message = new Message<MyMessageV2>(messageBody);
            var serializedMessage = serializationStrategy.SerializeMessage(message);

            // Mess with the properties to mimic a message serialised as MyMessageV3
            var messageType = serializedMessage.Properties.Type;
            serializedMessage.Properties.Type = messageType.Replace("MyMessageV2", "SomeCompletelyRandomType");
            var alternativeMessageHeader = (string)serializedMessage.Properties.Headers[AlternativeMessageTypesHeaderKey];
            alternativeMessageHeader = string.Concat(messageType, ";", alternativeMessageHeader);
            serializedMessage.Properties.Headers[AlternativeMessageTypesHeaderKey] = Encoding.UTF8.GetBytes(alternativeMessageHeader);

            var deserializedMessage = serializationStrategy.DeserializeMessage(serializedMessage.Properties, serializedMessage.Body);

            Assert.Equal(typeof(MyMessageV2), deserializedMessage.MessageType);
            Assert.Equal(message.Body.Text, ((Message<MyMessageV2>)deserializedMessage).Body.Text);
            Assert.Equal(message.Body.Number, ((Message<MyMessageV2>)deserializedMessage).Body.Number);
        }

        private void AssertMessageSerializedCorrectly(SerializedMessage message, byte[] expectedBody, Action<MessageProperties> assertMessagePropertiesCorrect)
        {
            Assert.Equal(expectedBody, message.Body);
            assertMessagePropertiesCorrect(message.Properties);
        }

        private void AssertMessageDeserializedCorrectly(IMessage<MyMessage> message, string expectedBodyText, Type expectedMessageType, Action<MessageProperties> assertMessagePropertiesCorrect)
        {
            Assert.Equal(expectedBodyText, message.Body.Text);
            Assert.Equal(expectedMessageType, message.MessageType);

            assertMessagePropertiesCorrect(message.Properties);
        }

        private void AssertDefaultMessagePropertiesCorrect(MessageProperties properties, string expectedType, string expectedCorrelationId)
        {
            Assert.Equal(expectedType, properties.Type);
            Assert.Equal(expectedCorrelationId, properties.CorrelationId);
        }

        private void AssertVersionedMessagePropertiesCorrect(MessageProperties properties, string expectedType, string expectedCorrelationId, string alternativeTypes)
        {
            AssertDefaultMessagePropertiesCorrect(properties, expectedType, expectedCorrelationId);
            Assert.Equal(alternativeTypes, properties.Headers[AlternativeMessageTypesHeaderKey]);
        }

        private VersionedMessageSerializationStrategy CreateSerializationStrategy<T>(IMessage<T> message, IEnumerable<KeyValuePair<string, Type>> messageTypes, byte[] messageBody, string correlationId) where T : class
        {
            var typeNameSerializer = MockRepository.GenerateStub<ITypeNameSerializer>();
            foreach (var messageType in messageTypes)
            {
                var localMessageType = messageType;
                typeNameSerializer.Stub(s => s.Serialize(localMessageType.Value)).Return(localMessageType.Key);
            }

            var serializer = MockRepository.GenerateStub<ISerializer>();
            serializer.Stub(s => s.MessageToBytes(message.GetBody())).Return(messageBody);

            return new VersionedMessageSerializationStrategy(typeNameSerializer, serializer, new StaticCorrelationIdGenerationStrategy(correlationId));
        }

        private VersionedMessageSerializationStrategy CreateDeserializationStrategy<T>(T message, IEnumerable<KeyValuePair<string, Type>> messageTypes, string expectedMessageType, byte[] messageBody) where T : class
        {
            var typeNameSerializer = MockRepository.GenerateStub<ITypeNameSerializer>();
            foreach (var messageType in messageTypes)
            {
                var localMessageType = messageType;
                typeNameSerializer.Stub(s => s.DeSerialize(localMessageType.Key)).Return(localMessageType.Value);
            }


            var serializer = MockRepository.GenerateStub<ISerializer>();
            serializer.Stub(s => s.BytesToMessage(expectedMessageType, messageBody)).Return(message);

            return new VersionedMessageSerializationStrategy(typeNameSerializer, serializer, new StaticCorrelationIdGenerationStrategy(String.Empty));
        }
    }
}