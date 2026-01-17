using MCCS.Infrastructure.Communication.NamedPipe.Models;
using MCCS.Infrastructure.Communication.NamedPipe.PubSub;
using Newtonsoft.Json;

namespace MCCS.UnitTest.NamedPipe.PubSub;

[TestClass]
public class TopicHandlerTests
{
    #region DelegateTopicHandler Tests

    [TestMethod]
    public void DelegateTopicHandler_Create_ShouldSetTopic()
    {
        var handler = new DelegateTopicHandler("test/topic", _ => { });

        Assert.AreEqual("test/topic", handler.Topic);
    }

    [TestMethod]
    public async Task DelegateTopicHandler_AsyncHandler_ShouldExecute()
    {
        var executed = false;
        PublishMessage? receivedMessage = null;

        var handler = new DelegateTopicHandler("test/topic", async (msg, ct) =>
        {
            await Task.Delay(10, ct);
            executed = true;
            receivedMessage = msg;
        });

        var message = PublishMessage.Create("test/topic", "test payload");
        await handler.HandleAsync(message);

        Assert.IsTrue(executed);
        Assert.IsNotNull(receivedMessage);
        Assert.AreEqual("test/topic", receivedMessage.Topic);
        Assert.AreEqual("test payload", receivedMessage.Payload);
    }

    [TestMethod]
    public async Task DelegateTopicHandler_SyncHandler_ShouldExecute()
    {
        var executed = false;
        PublishMessage? receivedMessage = null;

        var handler = new DelegateTopicHandler("test/topic", msg =>
        {
            executed = true;
            receivedMessage = msg;
        });

        var message = PublishMessage.Create("test/topic", "test payload");
        await handler.HandleAsync(message);

        Assert.IsTrue(executed);
        Assert.IsNotNull(receivedMessage);
    }

    [TestMethod]
    public async Task DelegateTopicHandler_WithCancellationToken_ShouldPassToken()
    {
        var tokenReceived = false;
        var handler = new DelegateTopicHandler("test/topic", (_, ct) =>
        {
            tokenReceived = ct != CancellationToken.None;
            return Task.CompletedTask;
        });

        var message = PublishMessage.Create("test/topic", "test");
        using var cts = new CancellationTokenSource();
        await handler.HandleAsync(message, cts.Token);

        Assert.IsTrue(tokenReceived);
    }

    #endregion

    #region Custom TopicHandler Implementation Tests

    [TestMethod]
    public async Task CustomTopicHandler_ShouldHandleMessage()
    {
        var handler = new TestSensorHandler();
        var message = PublishMessage.Create("sensor/temperature", "{\"value\":25.5,\"unit\":\"C\"}");

        await handler.HandleAsync(message);

        Assert.IsTrue(handler.HandlerCalled);
        Assert.AreEqual("sensor/temperature", handler.LastMessageTopic);
    }

    [TestMethod]
    public async Task GenericTopicHandler_ShouldDeserializeAndHandle()
    {
        var handler = new TestGenericSensorHandler();
        var sensorData = new SensorData { Value = 42.5, Unit = "°C" };
        var message = PublishMessage.Create("sensor/data", JsonConvert.SerializeObject(sensorData));

        await handler.HandleAsync(message);

        Assert.IsTrue(handler.HandlerCalled);
        Assert.IsNotNull(handler.LastData);
        Assert.AreEqual(42.5, handler.LastData.Value);
        Assert.AreEqual("°C", handler.LastData.Unit);
    }

    [TestMethod]
    public async Task GenericTopicHandler_WithNullPayload_ShouldNotCallTypedHandler()
    {
        var handler = new TestGenericSensorHandler();
        var message = PublishMessage.Create("sensor/data", null);

        await handler.HandleAsync(message);

        // Handler should not be called for null data
        Assert.IsFalse(handler.HandlerCalled);
    }

    #endregion

    #region ITopicHandler Interface Tests

    [TestMethod]
    public void ITopicHandler_Route_ShouldReturnCorrectTopic()
    {
        ITopicHandler handler = new DelegateTopicHandler("my/custom/topic", _ => { });

        Assert.AreEqual("my/custom/topic", handler.Topic);
    }

    [TestMethod]
    public async Task ITopicHandler_HandleAsync_ShouldBeCallable()
    {
        ITopicHandler handler = new DelegateTopicHandler("test/topic", _ => { });
        var message = PublishMessage.Create("test/topic", "data");

        // Should not throw
        await handler.HandleAsync(message);
    }

    #endregion

    #region Multiple Handlers Same Topic Tests

    [TestMethod]
    public async Task MultipleHandlers_SameTopic_ShouldAllExecute()
    {
        var handler1Executed = false;
        var handler2Executed = false;
        var handler3Executed = false;

        var handler1 = new DelegateTopicHandler("shared/topic", _ => handler1Executed = true);
        var handler2 = new DelegateTopicHandler("shared/topic", _ => handler2Executed = true);
        var handler3 = new DelegateTopicHandler("shared/topic", _ => handler3Executed = true);

        var message = PublishMessage.Create("shared/topic", "data");

        await handler1.HandleAsync(message);
        await handler2.HandleAsync(message);
        await handler3.HandleAsync(message);

        Assert.IsTrue(handler1Executed);
        Assert.IsTrue(handler2Executed);
        Assert.IsTrue(handler3Executed);
    }

    #endregion

    #region Error Handling Tests

    [TestMethod]
    public async Task DelegateTopicHandler_WithException_ShouldPropagate()
    {
        var handler = new DelegateTopicHandler("test/topic", (_, _) =>
        {
            throw new InvalidOperationException("Test exception");
        });

        var message = PublishMessage.Create("test/topic", "data");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await handler.HandleAsync(message);
        });
    }

    [TestMethod]
    public async Task DelegateTopicHandler_WithCancellation_ShouldThrowCancelledException()
    {
        var handler = new DelegateTopicHandler("test/topic", async (_, ct) =>
        {
            await Task.Delay(5000, ct); // Long delay
        });

        var message = PublishMessage.Create("test/topic", "data");
        using var cts = new CancellationTokenSource(100); // Cancel after 100ms

        await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () =>
        {
            await handler.HandleAsync(message, cts.Token);
        });
    }

    #endregion

    #region Helper Classes

    private class SensorData
    {
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    private class TestSensorHandler : TopicHandlerBase
    {
        public bool HandlerCalled { get; private set; }
        public string? LastMessageTopic { get; private set; }

        public override string Topic => "sensor/temperature";

        public override Task HandleAsync(PublishMessage message, CancellationToken cancellationToken = default)
        {
            HandlerCalled = true;
            LastMessageTopic = message.Topic;
            return Task.CompletedTask;
        }
    }

    private class TestGenericSensorHandler : TopicHandlerBase<SensorData>
    {
        public bool HandlerCalled { get; private set; }
        public SensorData? LastData { get; private set; }

        public TestGenericSensorHandler()
            : base(payload => string.IsNullOrEmpty(payload) ? null : JsonConvert.DeserializeObject<SensorData>(payload))
        {
        }

        public override string Topic => "sensor/data";

        public override Task HandleAsync(SensorData data, CancellationToken cancellationToken = default)
        {
            HandlerCalled = true;
            LastData = data;
            return Task.CompletedTask;
        }
    }

    #endregion
}
