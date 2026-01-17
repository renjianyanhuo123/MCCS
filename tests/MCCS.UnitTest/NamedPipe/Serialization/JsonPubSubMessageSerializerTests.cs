using System.Text;
using MCCS.Infrastructure.Communication.NamedPipe.Models;
using MCCS.Infrastructure.Communication.NamedPipe.Serialization;

using Newtonsoft.Json;

namespace MCCS.UnitTest.NamedPipe.Serialization;

[TestClass]
public class JsonPubSubMessageSerializerTests
{
    private JsonPubSubMessageSerializer _serializer = null!;

    [TestInitialize]
    public void Setup()
    {
        _serializer = new JsonPubSubMessageSerializer();
    }

    #region Envelope Serialization Tests

    [TestMethod]
    public void SerializeEnvelope_ShouldReturnValidBytes()
    {
        var envelope = PipeMessageEnvelope.Create(PipeMessageType.Publish, "{\"topic\":\"test\"}");

        var bytes = _serializer.SerializeEnvelope(envelope);

        Assert.IsNotNull(bytes);
        Assert.IsTrue(bytes.Length > 0);
        var json = Encoding.UTF8.GetString(bytes);
        Assert.IsTrue(json.Contains("\"MessageType\":4")); // Publish = 4
        Assert.IsTrue(json.Contains("\"Payload\":\"{\\\"topic\\\":\\\"test\\\"}\""));
    }

    [TestMethod]
    public void DeserializeEnvelope_ShouldReturnValidEnvelope()
    {
        var originalEnvelope = PipeMessageEnvelope.Create(PipeMessageType.Subscribe, "{\"topic\":\"test/topic\"}");
        var bytes = _serializer.SerializeEnvelope(originalEnvelope);

        var deserializedEnvelope = _serializer.DeserializeEnvelope(bytes);

        Assert.AreEqual(originalEnvelope.MessageType, deserializedEnvelope.MessageType);
        Assert.AreEqual(originalEnvelope.Payload, deserializedEnvelope.Payload);
    }

    [TestMethod]
    public void SerializeDeserialize_Envelope_ShouldPreserveAllMessageTypes()
    {
        var messageTypes = new[]
        {
            PipeMessageType.Request,
            PipeMessageType.Response,
            PipeMessageType.Subscribe,
            PipeMessageType.Unsubscribe,
            PipeMessageType.Publish,
            PipeMessageType.SubscribeAck,
            PipeMessageType.UnsubscribeAck
        };

        foreach (var messageType in messageTypes)
        {
            var envelope = PipeMessageEnvelope.Create(messageType, "{}");
            var bytes = _serializer.SerializeEnvelope(envelope);
            var deserialized = _serializer.DeserializeEnvelope(bytes);

            Assert.AreEqual(messageType, deserialized.MessageType, $"MessageType {messageType} was not preserved");
        }
    }

    [TestMethod]
    [ExpectedException(typeof(JsonReaderException))]
    public void DeserializeEnvelope_WithInvalidJson_ShouldThrowException()
    {
        var invalidBytes = Encoding.UTF8.GetBytes("not valid json");
        _serializer.DeserializeEnvelope(invalidBytes);
    }

    #endregion

    #region Generic Serialization Tests

    [TestMethod]
    public void Serialize_ShouldSerializeObjectToJson()
    {
        var data = new TestData { Id = 42, Name = "Test Name", Value = 3.14 };

        var json = _serializer.Serialize(data);

        Assert.IsTrue(json.Contains("\"Id\":42"));
        Assert.IsTrue(json.Contains("\"Name\":\"Test Name\""));
        Assert.IsTrue(json.Contains("\"Value\":3.14"));
    }

    [TestMethod]
    public void Deserialize_ShouldDeserializeJsonToObject()
    {
        var json = "{\"Id\":42,\"Name\":\"Test Name\",\"Value\":3.14}";

        var data = _serializer.Deserialize<TestData>(json);

        Assert.IsNotNull(data);
        Assert.AreEqual(42, data.Id);
        Assert.AreEqual("Test Name", data.Name);
        Assert.AreEqual(3.14, data.Value);
    }

    [TestMethod]
    public void SerializeObject_ShouldSerializeNonGenericObject()
    {
        object data = new TestData { Id = 1, Name = "Test" };

        var json = _serializer.SerializeObject(data);

        Assert.IsTrue(json.Contains("\"Id\":1"));
        Assert.IsTrue(json.Contains("\"Name\":\"Test\""));
    }

    [TestMethod]
    public void Deserialize_WithNullValues_ShouldHandleGracefully()
    {
        var json = "{\"Id\":1,\"Name\":null,\"Value\":0}";

        var data = _serializer.Deserialize<TestData>(json);

        Assert.IsNotNull(data);
        Assert.AreEqual(1, data.Id);
        Assert.IsNull(data.Name);
    }

    #endregion

    #region PubSub Message Serialization Tests

    [TestMethod]
    public void Serialize_SubscribeMessage_ShouldSerializeCorrectly()
    {
        var message = SubscribeMessage.Create("test/topic", "subscriber-1");

        var json = _serializer.Serialize(message);

        Assert.IsTrue(json.Contains("\"Topic\":\"test/topic\""));
        Assert.IsTrue(json.Contains("\"SubscriberId\":\"subscriber-1\""));
    }

    [TestMethod]
    public void Deserialize_SubscribeMessage_ShouldDeserializeCorrectly()
    {
        var original = SubscribeMessage.Create("test/topic", "subscriber-1");
        var json = _serializer.Serialize(original);

        var deserialized = _serializer.Deserialize<SubscribeMessage>(json);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.Topic, deserialized.Topic);
        Assert.AreEqual(original.SubscriberId, deserialized.SubscriberId);
        Assert.AreEqual(original.MessageId, deserialized.MessageId);
    }

    [TestMethod]
    public void Serialize_PublishMessage_ShouldSerializeCorrectly()
    {
        var message = PublishMessage.Create("sensor/temperature", "{\"value\":25.5}");

        var json = _serializer.Serialize(message);

        Assert.IsTrue(json.Contains("\"Topic\":\"sensor/temperature\""));
        Assert.IsTrue(json.Contains("\"Payload\":\"{\\\"value\\\":25.5}\""));
    }

    [TestMethod]
    public void Deserialize_PublishMessage_ShouldDeserializeCorrectly()
    {
        var original = PublishMessage.Create("sensor/temperature", "{\"value\":25.5}");
        var json = _serializer.Serialize(original);

        var deserialized = _serializer.Deserialize<PublishMessage>(json);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.Topic, deserialized.Topic);
        Assert.AreEqual(original.Payload, deserialized.Payload);
    }

    [TestMethod]
    public void Serialize_SubscribeAckMessage_ShouldSerializeCorrectly()
    {
        var ack = SubscribeAckMessage.Succeeded("test/topic", "subscriber-1");

        var json = _serializer.Serialize(ack);

        Assert.IsTrue(json.Contains("\"Success\":true"));
        Assert.IsTrue(json.Contains("\"Topic\":\"test/topic\""));
        Assert.IsTrue(json.Contains("\"SubscriberId\":\"subscriber-1\""));
    }

    [TestMethod]
    public void Deserialize_SubscribeAckMessage_ShouldDeserializeCorrectly()
    {
        var original = SubscribeAckMessage.Succeeded("test/topic", "subscriber-1");
        var json = _serializer.Serialize(original);

        var deserialized = _serializer.Deserialize<SubscribeAckMessage>(json);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.Success, deserialized.Success);
        Assert.AreEqual(original.Topic, deserialized.Topic);
        Assert.AreEqual(original.SubscriberId, deserialized.SubscriberId);
    }

    [TestMethod]
    public void Serialize_UnsubscribeMessage_ShouldSerializeCorrectly()
    {
        var message = UnsubscribeMessage.Create("test/topic", "subscriber-1");

        var json = _serializer.Serialize(message);

        Assert.IsTrue(json.Contains("\"Topic\":\"test/topic\""));
        Assert.IsTrue(json.Contains("\"SubscriberId\":\"subscriber-1\""));
    }

    [TestMethod]
    public void Serialize_UnsubscribeAckMessage_ShouldSerializeCorrectly()
    {
        var ack = UnsubscribeAckMessage.Succeeded("test/topic");

        var json = _serializer.Serialize(ack);

        Assert.IsTrue(json.Contains("\"Success\":true"));
        Assert.IsTrue(json.Contains("\"Topic\":\"test/topic\""));
    }

    #endregion

    #region Null Handling Tests

    [TestMethod]
    public void Serialize_WithNullIgnored_ShouldOmitNullValues()
    {
        var message = SubscribeMessage.Create("test/topic"); // SubscriberId is null

        var json = _serializer.Serialize(message);

        // With NullValueHandling.Ignore, null values should be omitted
        Assert.IsFalse(json.Contains("\"SubscriberId\":null"));
    }

    #endregion

    #region Helper Classes

    private class TestData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double Value { get; set; }
    }

    #endregion
}
