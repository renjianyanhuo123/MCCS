using MCCS.Infrastructure.Communication.NamedPipe;
using MCCS.Infrastructure.Communication.NamedPipe.Models;
using MCCS.Infrastructure.Communication.NamedPipe.PubSub;
using Newtonsoft.Json;

namespace MCCS.UnitTest.NamedPipe.PubSub;

[TestClass]
public class PubSubIntegrationTests
{
    private string _pipeName = null!;

    [TestInitialize]
    public void Setup()
    {
        _pipeName = $"TestPubSub_{Guid.NewGuid():N}";
    }

    #region Connection Tests

    [TestMethod]
    public async Task Client_ConnectToServer_ShouldSucceed()
    {
        var serverOptions = new PubSubServerOptions { PipeName = _pipeName };
        var clientOptions = new PubSubClientOptions { PipeName = _pipeName };

        using var server = new PubSubServer(serverOptions);
        using var client = new PubSubClient(clientOptions);

        await server.StartAsync();
        await client.ConnectAsync();

        Assert.IsTrue(client.IsConnected);
        Assert.AreEqual(1, server.ActiveConnections);

        await client.DisconnectAsync();
        await server.StopAsync();
    }

    [TestMethod]
    public async Task Server_ClientConnectedEvent_ShouldFire()
    {
        var serverOptions = new PubSubServerOptions { PipeName = _pipeName };
        var clientOptions = new PubSubClientOptions { PipeName = _pipeName };

        using var server = new PubSubServer(serverOptions);
        using var client = new PubSubClient(clientOptions);

        string? connectedId = null;
        var eventFired = new TaskCompletionSource<bool>();

        server.ClientConnected += id =>
        {
            connectedId = id;
            eventFired.TrySetResult(true);
        };

        await server.StartAsync();
        await client.ConnectAsync();

        await Task.WhenAny(eventFired.Task, Task.Delay(2000));

        Assert.IsNotNull(connectedId);

        await client.DisconnectAsync();
        await server.StopAsync();
    }

    [TestMethod]
    public async Task Server_ClientDisconnectedEvent_ShouldFire()
    {
        var serverOptions = new PubSubServerOptions { PipeName = _pipeName };
        var clientOptions = new PubSubClientOptions { PipeName = _pipeName };

        using var server = new PubSubServer(serverOptions);
        using var client = new PubSubClient(clientOptions);

        var disconnectedEvent = new TaskCompletionSource<bool>();

        server.ClientDisconnected += _ =>
        {
            disconnectedEvent.TrySetResult(true);
        };

        await server.StartAsync();
        await client.ConnectAsync();
        await client.DisconnectAsync();

        var result = await Task.WhenAny(disconnectedEvent.Task, Task.Delay(2000));

        Assert.IsTrue(disconnectedEvent.Task.IsCompleted);

        await server.StopAsync();
    }

    #endregion

    #region Subscribe and Unsubscribe Tests

    [TestMethod]
    public async Task Client_SubscribeAsync_ShouldSucceed()
    {
        var serverOptions = new PubSubServerOptions { PipeName = _pipeName };
        var clientOptions = new PubSubClientOptions { PipeName = _pipeName };

        using var server = new PubSubServer(serverOptions);
        using var client = new PubSubClient(clientOptions);

        await server.StartAsync();
        await client.ConnectAsync();

        await client.SubscribeAsync("test/topic", _ => { });

        Assert.IsTrue(client.SubscribedTopics.Contains("test/topic"));
        Assert.IsNotNull(client.SubscriberId);

        await client.DisconnectAsync();
        await server.StopAsync();
    }

    [TestMethod]
    public async Task Client_SubscribeMultipleTopics_ShouldSucceed()
    {
        var serverOptions = new PubSubServerOptions { PipeName = _pipeName };
        var clientOptions = new PubSubClientOptions { PipeName = _pipeName };

        using var server = new PubSubServer(serverOptions);
        using var client = new PubSubClient(clientOptions);

        await server.StartAsync();
        await client.ConnectAsync();

        await client.SubscribeAsync("topic1", _ => { });
        await client.SubscribeAsync("topic2", _ => { });
        await client.SubscribeAsync("topic3", _ => { });

        Assert.AreEqual(3, client.SubscribedTopics.Count);

        await client.DisconnectAsync();
        await server.StopAsync();
    }

    [TestMethod]
    public async Task Client_UnsubscribeAsync_ShouldSucceed()
    {
        var serverOptions = new PubSubServerOptions { PipeName = _pipeName };
        var clientOptions = new PubSubClientOptions { PipeName = _pipeName };

        using var server = new PubSubServer(serverOptions);
        using var client = new PubSubClient(clientOptions);

        await server.StartAsync();
        await client.ConnectAsync();

        await client.SubscribeAsync("test/topic", _ => { });
        await client.UnsubscribeAsync("test/topic");

        Assert.IsFalse(client.SubscribedTopics.Contains("test/topic"));

        await client.DisconnectAsync();
        await server.StopAsync();
    }

    [TestMethod]
    public async Task Client_UnsubscribeAll_ShouldRemoveAllSubscriptions()
    {
        var serverOptions = new PubSubServerOptions { PipeName = _pipeName };
        var clientOptions = new PubSubClientOptions { PipeName = _pipeName };

        using var server = new PubSubServer(serverOptions);
        using var client = new PubSubClient(clientOptions);

        await server.StartAsync();
        await client.ConnectAsync();

        await client.SubscribeAsync("topic1", _ => { });
        await client.SubscribeAsync("topic2", _ => { });
        await client.UnsubscribeAsync();

        Assert.AreEqual(0, client.SubscribedTopics.Count);

        await client.DisconnectAsync();
        await server.StopAsync();
    }

    #endregion

    #region Publish Tests

    [TestMethod]
    public async Task Server_PublishAsync_ClientShouldReceiveMessage()
    {
        var serverOptions = new PubSubServerOptions { PipeName = _pipeName };
        var clientOptions = new PubSubClientOptions { PipeName = _pipeName };

        using var server = new PubSubServer(serverOptions);
        using var client = new PubSubClient(clientOptions);

        PublishMessage? receivedMessage = null;
        var messageReceived = new TaskCompletionSource<bool>();

        await server.StartAsync();
        await client.ConnectAsync();

        await client.SubscribeAsync("sensor/temperature", msg =>
        {
            receivedMessage = msg;
            messageReceived.TrySetResult(true);
        });

        // Give time for subscription to be processed
        await Task.Delay(100);

        var publishCount = await server.PublishAsync("sensor/temperature", "{\"value\":25.5}");

        await Task.WhenAny(messageReceived.Task, Task.Delay(2000));

        Assert.AreEqual(1, publishCount);
        Assert.IsNotNull(receivedMessage);
        Assert.AreEqual("sensor/temperature", receivedMessage.Topic);
        Assert.AreEqual("{\"value\":25.5}", receivedMessage.Payload);

        await client.DisconnectAsync();
        await server.StopAsync();
    }

    [TestMethod]
    public async Task Server_PublishAsync_MultipleSubscribersShouldReceive()
    {
        var serverOptions = new PubSubServerOptions { PipeName = _pipeName };

        using var server = new PubSubServer(serverOptions);
        using var client1 = new PubSubClient(new PubSubClientOptions { PipeName = _pipeName });
        using var client2 = new PubSubClient(new PubSubClientOptions { PipeName = _pipeName });
        using var client3 = new PubSubClient(new PubSubClientOptions { PipeName = _pipeName });

        var receivedCount = 0;
        var allReceived = new TaskCompletionSource<bool>();

        await server.StartAsync();

        await client1.ConnectAsync();
        await client2.ConnectAsync();
        await client3.ConnectAsync();

        await client1.SubscribeAsync("broadcast", _ => { Interlocked.Increment(ref receivedCount); CheckAll(); });
        await client2.SubscribeAsync("broadcast", _ => { Interlocked.Increment(ref receivedCount); CheckAll(); });
        await client3.SubscribeAsync("broadcast", _ => { Interlocked.Increment(ref receivedCount); CheckAll(); });

        void CheckAll()
        {
            if (receivedCount >= 3)
                allReceived.TrySetResult(true);
        }

        await Task.Delay(100);

        var publishCount = await server.PublishAsync("broadcast", "Hello everyone!");

        await Task.WhenAny(allReceived.Task, Task.Delay(3000));

        Assert.AreEqual(3, publishCount);
        Assert.AreEqual(3, receivedCount);

        await client1.DisconnectAsync();
        await client2.DisconnectAsync();
        await client3.DisconnectAsync();
        await server.StopAsync();
    }

    [TestMethod]
    public async Task Server_PublishAsync_OnlySubscribedTopicShouldReceive()
    {
        var serverOptions = new PubSubServerOptions { PipeName = _pipeName };
        var clientOptions = new PubSubClientOptions { PipeName = _pipeName };

        using var server = new PubSubServer(serverOptions);
        using var client = new PubSubClient(clientOptions);

        var topic1Received = false;
        var topic2Received = false;

        await server.StartAsync();
        await client.ConnectAsync();

        await client.SubscribeAsync("topic1", _ => topic1Received = true);
        // Not subscribing to topic2

        await Task.Delay(100);

        await server.PublishAsync("topic1", "message1");
        await server.PublishAsync("topic2", "message2");

        await Task.Delay(500);

        Assert.IsTrue(topic1Received);
        Assert.IsFalse(topic2Received);

        await client.DisconnectAsync();
        await server.StopAsync();
    }

    [TestMethod]
    public async Task Server_PublishAsync_GenericData_ShouldSerializeCorrectly()
    {
        var serverOptions = new PubSubServerOptions { PipeName = _pipeName };
        var clientOptions = new PubSubClientOptions { PipeName = _pipeName };

        using var server = new PubSubServer(serverOptions);
        using var client = new PubSubClient(clientOptions);

        SensorData? receivedData = null;
        var messageReceived = new TaskCompletionSource<bool>();

        await server.StartAsync();
        await client.ConnectAsync();

        await client.SubscribeAsync<SensorData>("sensor/data", data =>
        {
            receivedData = data;
            messageReceived.TrySetResult(true);
        });

        await Task.Delay(100);

        var sensorData = new SensorData { SensorId = "S001", Value = 42.5, Unit = "°C" };
        await server.PublishAsync("sensor/data", sensorData);

        await Task.WhenAny(messageReceived.Task, Task.Delay(2000));

        Assert.IsNotNull(receivedData);
        Assert.AreEqual("S001", receivedData.SensorId);
        Assert.AreEqual(42.5, receivedData.Value);
        Assert.AreEqual("°C", receivedData.Unit);

        await client.DisconnectAsync();
        await server.StopAsync();
    }

    #endregion

    #region MessageReceived Event Tests

    [TestMethod]
    public async Task Client_MessageReceivedEvent_ShouldFireForAllMessages()
    {
        var serverOptions = new PubSubServerOptions { PipeName = _pipeName };
        var clientOptions = new PubSubClientOptions { PipeName = _pipeName };

        using var server = new PubSubServer(serverOptions);
        using var client = new PubSubClient(clientOptions);

        var globalMessages = new List<PublishMessage>();
        var allReceived = new TaskCompletionSource<bool>();

        client.MessageReceived += msg =>
        {
            lock (globalMessages)
            {
                globalMessages.Add(msg);
                if (globalMessages.Count >= 2)
                    allReceived.TrySetResult(true);
            }
        };

        await server.StartAsync();
        await client.ConnectAsync();

        await client.SubscribeAsync("topic1", _ => { });
        await client.SubscribeAsync("topic2", _ => { });

        await Task.Delay(100);

        await server.PublishAsync("topic1", "message1");
        await server.PublishAsync("topic2", "message2");

        await Task.WhenAny(allReceived.Task, Task.Delay(2000));

        Assert.AreEqual(2, globalMessages.Count);

        await client.DisconnectAsync();
        await server.StopAsync();
    }

    #endregion

    #region Factory Tests

    [TestMethod]
    public async Task NamedPipeFactory_CreatePubSubServer_ShouldWork()
    {
        using var server = NamedPipeFactory.CreatePubSubServer(_pipeName);

        Assert.IsNotNull(server);
        Assert.AreEqual(_pipeName, server.PipeName);

        await server.StartAsync();
        Assert.IsTrue(server.IsRunning);

        await server.StopAsync();
    }

    [TestMethod]
    public async Task NamedPipeFactory_CreatePubSubClient_ShouldWork()
    {
        using var server = NamedPipeFactory.CreatePubSubServer(_pipeName);
        using var client = NamedPipeFactory.CreatePubSubClient(_pipeName);

        Assert.IsNotNull(client);

        await server.StartAsync();
        await client.ConnectAsync();

        Assert.IsTrue(client.IsConnected);

        await client.DisconnectAsync();
        await server.StopAsync();
    }

    [TestMethod]
    public async Task NamedPipeFactory_CreatePubSubServerWithConfigure_ShouldApplyOptions()
    {
        using var server = NamedPipeFactory.CreatePubSubServer(options =>
        {
            options.PipeName = _pipeName;
            options.MaxConcurrentConnections = 100;
        });

        Assert.AreEqual(_pipeName, server.PipeName);

        await server.StartAsync();
        Assert.IsTrue(server.IsRunning);

        await server.StopAsync();
    }

    [TestMethod]
    public async Task NamedPipeFactory_CreatePubSubClientWithConfigure_ShouldApplyOptions()
    {
        using var server = NamedPipeFactory.CreatePubSubServer(_pipeName);
        using var client = NamedPipeFactory.CreatePubSubClient(options =>
        {
            options.PipeName = _pipeName;
            options.ConnectTimeoutMs = 10000;
        });

        await server.StartAsync();
        await client.ConnectAsync();

        Assert.IsTrue(client.IsConnected);

        await client.DisconnectAsync();
        await server.StopAsync();
    }

    #endregion

    #region Concurrent Publish Tests

    [TestMethod]
    public async Task Server_ConcurrentPublish_ShouldHandleCorrectly()
    {
        var serverOptions = new PubSubServerOptions { PipeName = _pipeName };
        var clientOptions = new PubSubClientOptions { PipeName = _pipeName };

        using var server = new PubSubServer(serverOptions);
        using var client = new PubSubClient(clientOptions);

        var receivedCount = 0;
        var allReceived = new TaskCompletionSource<bool>();
        const int messageCount = 50;

        await server.StartAsync();
        await client.ConnectAsync();

        await client.SubscribeAsync("stress/test", _ =>
        {
            if (Interlocked.Increment(ref receivedCount) >= messageCount)
                allReceived.TrySetResult(true);
        });

        await Task.Delay(100);

        var publishTasks = Enumerable.Range(0, messageCount)
            .Select(i => server.PublishAsync("stress/test", $"Message {i}"))
            .ToArray();

        await Task.WhenAll(publishTasks);

        await Task.WhenAny(allReceived.Task, Task.Delay(5000));

        Assert.AreEqual(messageCount, receivedCount);

        await client.DisconnectAsync();
        await server.StopAsync();
    }

    #endregion

    #region Helper Classes

    private class SensorData
    {
        public string SensorId { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    #endregion
}
