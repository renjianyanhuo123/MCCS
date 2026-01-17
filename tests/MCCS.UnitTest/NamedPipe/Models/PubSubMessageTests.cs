using MCCS.Infrastructure.Communication.NamedPipe.Models;

namespace MCCS.UnitTest.NamedPipe.Models;

[TestClass]
public class PubSubMessageTests
{
    #region PipeMessageType Tests

    [TestMethod]
    public void PipeMessageType_ShouldHaveExpectedValues()
    {
        Assert.AreEqual(0, (int)PipeMessageType.Request);
        Assert.AreEqual(1, (int)PipeMessageType.Response);
        Assert.AreEqual(2, (int)PipeMessageType.Subscribe);
        Assert.AreEqual(3, (int)PipeMessageType.Unsubscribe);
        Assert.AreEqual(4, (int)PipeMessageType.Publish);
        Assert.AreEqual(5, (int)PipeMessageType.SubscribeAck);
        Assert.AreEqual(6, (int)PipeMessageType.UnsubscribeAck);
    }

    #endregion

    #region SubscribeMessage Tests

    [TestMethod]
    public void SubscribeMessage_Create_ShouldSetTopicAndSubscriberId()
    {
        var message = SubscribeMessage.Create("test/topic", "subscriber-1");

        Assert.AreEqual("test/topic", message.Topic);
        Assert.AreEqual("subscriber-1", message.SubscriberId);
        Assert.AreEqual(PipeMessageType.Subscribe, message.MessageType);
        Assert.IsFalse(string.IsNullOrEmpty(message.MessageId));
        Assert.IsTrue(message.Timestamp > 0);
    }

    [TestMethod]
    public void SubscribeMessage_Create_WithoutSubscriberId_ShouldHaveNullSubscriberId()
    {
        var message = SubscribeMessage.Create("test/topic");

        Assert.AreEqual("test/topic", message.Topic);
        Assert.IsNull(message.SubscriberId);
    }

    [TestMethod]
    public void SubscribeMessage_MessageId_ShouldBeUnique()
    {
        var message1 = SubscribeMessage.Create("topic1");
        var message2 = SubscribeMessage.Create("topic2");

        Assert.AreNotEqual(message1.MessageId, message2.MessageId);
    }

    #endregion

    #region UnsubscribeMessage Tests

    [TestMethod]
    public void UnsubscribeMessage_Create_ShouldSetTopicAndSubscriberId()
    {
        var message = UnsubscribeMessage.Create("test/topic", "subscriber-1");

        Assert.AreEqual("test/topic", message.Topic);
        Assert.AreEqual("subscriber-1", message.SubscriberId);
        Assert.AreEqual(PipeMessageType.Unsubscribe, message.MessageType);
    }

    [TestMethod]
    public void UnsubscribeMessage_Create_WithoutParameters_ShouldHaveNullValues()
    {
        var message = UnsubscribeMessage.Create();

        Assert.IsNull(message.Topic);
        Assert.IsNull(message.SubscriberId);
    }

    #endregion

    #region PublishMessage Tests

    [TestMethod]
    public void PublishMessage_Create_ShouldSetTopicAndPayload()
    {
        var message = PublishMessage.Create("test/topic", "{\"value\": 42}");

        Assert.AreEqual("test/topic", message.Topic);
        Assert.AreEqual("{\"value\": 42}", message.Payload);
        Assert.AreEqual(PipeMessageType.Publish, message.MessageType);
    }

    [TestMethod]
    public void PublishMessage_Create_WithSerializer_ShouldSerializeData()
    {
        var data = new TestData { Id = 1, Name = "Test" };
        var message = PublishMessage.Create("test/topic", data, obj => $"{{\"Id\":{obj.Id},\"Name\":\"{obj.Name}\"}}");

        Assert.AreEqual("test/topic", message.Topic);
        Assert.IsTrue(message.Payload!.Contains("\"Id\":1"));
        Assert.IsTrue(message.Payload.Contains("\"Name\":\"Test\""));
    }

    [TestMethod]
    public void PublishMessage_Create_WithNullPayload_ShouldAllowNull()
    {
        var message = PublishMessage.Create("test/topic", null);

        Assert.AreEqual("test/topic", message.Topic);
        Assert.IsNull(message.Payload);
    }

    #endregion

    #region SubscribeAckMessage Tests

    [TestMethod]
    public void SubscribeAckMessage_Succeeded_ShouldSetCorrectProperties()
    {
        var ack = SubscribeAckMessage.Succeeded("test/topic", "subscriber-1", "msg-123");

        Assert.IsTrue(ack.Success);
        Assert.AreEqual("test/topic", ack.Topic);
        Assert.AreEqual("subscriber-1", ack.SubscriberId);
        Assert.AreEqual("msg-123", ack.MessageId);
        Assert.IsNull(ack.ErrorMessage);
        Assert.AreEqual(PipeMessageType.SubscribeAck, ack.MessageType);
    }

    [TestMethod]
    public void SubscribeAckMessage_Failed_ShouldSetErrorMessage()
    {
        var ack = SubscribeAckMessage.Failed("test/topic", "Subscription failed");

        Assert.IsFalse(ack.Success);
        Assert.AreEqual("test/topic", ack.Topic);
        Assert.AreEqual("Subscription failed", ack.ErrorMessage);
    }

    [TestMethod]
    public void SubscribeAckMessage_Succeeded_WithoutMessageId_ShouldGenerateNewId()
    {
        var ack = SubscribeAckMessage.Succeeded("test/topic", "subscriber-1");

        Assert.IsFalse(string.IsNullOrEmpty(ack.MessageId));
    }

    #endregion

    #region UnsubscribeAckMessage Tests

    [TestMethod]
    public void UnsubscribeAckMessage_Succeeded_ShouldSetCorrectProperties()
    {
        var ack = UnsubscribeAckMessage.Succeeded("test/topic", "msg-123");

        Assert.IsTrue(ack.Success);
        Assert.AreEqual("test/topic", ack.Topic);
        Assert.AreEqual("msg-123", ack.MessageId);
        Assert.IsNull(ack.ErrorMessage);
        Assert.AreEqual(PipeMessageType.UnsubscribeAck, ack.MessageType);
    }

    [TestMethod]
    public void UnsubscribeAckMessage_Failed_ShouldSetErrorMessage()
    {
        var ack = UnsubscribeAckMessage.Failed("Unsubscribe failed", "test/topic");

        Assert.IsFalse(ack.Success);
        Assert.AreEqual("test/topic", ack.Topic);
        Assert.AreEqual("Unsubscribe failed", ack.ErrorMessage);
    }

    [TestMethod]
    public void UnsubscribeAckMessage_Succeeded_WithNullTopic_ShouldAllowNull()
    {
        var ack = UnsubscribeAckMessage.Succeeded();

        Assert.IsTrue(ack.Success);
        Assert.IsNull(ack.Topic);
    }

    #endregion

    #region PipeMessageEnvelope Tests

    [TestMethod]
    public void PipeMessageEnvelope_Create_ShouldSetTypeAndPayload()
    {
        var envelope = PipeMessageEnvelope.Create(PipeMessageType.Publish, "{\"topic\":\"test\"}");

        Assert.AreEqual(PipeMessageType.Publish, envelope.MessageType);
        Assert.AreEqual("{\"topic\":\"test\"}", envelope.Payload);
    }

    [TestMethod]
    public void PipeMessageEnvelope_Create_WithSubscribeType_ShouldSetCorrectType()
    {
        var envelope = PipeMessageEnvelope.Create(PipeMessageType.Subscribe, "{}");

        Assert.AreEqual(PipeMessageType.Subscribe, envelope.MessageType);
    }

    #endregion

    #region Helper Classes

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}
