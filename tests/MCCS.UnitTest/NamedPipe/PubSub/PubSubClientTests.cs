using MCCS.Infrastructure.Communication.NamedPipe.PubSub;

namespace MCCS.UnitTest.NamedPipe.PubSub;

[TestClass]
public class PubSubClientTests
{
    #region Options Tests

    [TestMethod]
    public void PubSubClientOptions_DefaultValues_ShouldBeCorrect()
    {
        var options = new PubSubClientOptions();

        Assert.AreEqual("MCCS_PubSub_Pipe", options.PipeName);
        Assert.AreEqual(".", options.ServerName);
        Assert.AreEqual(5000, options.ConnectTimeoutMs);
        Assert.AreEqual(65536, options.ReceiveBufferSize);
        Assert.IsTrue(options.AutoReconnect);
        Assert.AreEqual(1000, options.ReconnectIntervalMs);
        Assert.AreEqual(5, options.MaxReconnectAttempts);
        Assert.IsNull(options.SubscriberId);
    }

    [TestMethod]
    public void PubSubClientOptions_CustomValues_ShouldBeSet()
    {
        var options = new PubSubClientOptions
        {
            PipeName = "CustomPipe",
            ServerName = "RemoteServer",
            ConnectTimeoutMs = 10000,
            ReceiveBufferSize = 131072,
            AutoReconnect = false,
            ReconnectIntervalMs = 2000,
            MaxReconnectAttempts = 10,
            SubscriberId = "custom-subscriber"
        };

        Assert.AreEqual("CustomPipe", options.PipeName);
        Assert.AreEqual("RemoteServer", options.ServerName);
        Assert.AreEqual(10000, options.ConnectTimeoutMs);
        Assert.AreEqual(131072, options.ReceiveBufferSize);
        Assert.IsFalse(options.AutoReconnect);
        Assert.AreEqual(2000, options.ReconnectIntervalMs);
        Assert.AreEqual(10, options.MaxReconnectAttempts);
        Assert.AreEqual("custom-subscriber", options.SubscriberId);
    }

    #endregion

    #region Client Initialization Tests

    [TestMethod]
    public void PubSubClient_Create_ShouldInitializeCorrectly()
    {
        using var client = new PubSubClient();

        Assert.IsFalse(client.IsConnected);
        Assert.IsNull(client.SubscriberId);
        Assert.AreEqual(0, client.SubscribedTopics.Count);
    }

    [TestMethod]
    public void PubSubClient_CreateWithOptions_ShouldUseCustomOptions()
    {
        var options = new PubSubClientOptions { SubscriberId = "custom-id" };
        using var client = new PubSubClient(options);

        // SubscriberId is set but connection not established
        Assert.IsFalse(client.IsConnected);
    }

    #endregion

    #region Connection Tests

    [TestMethod]
    public async Task PubSubClient_ConnectAsync_WithoutServer_ShouldTimeout()
    {
        var options = new PubSubClientOptions
        {
            PipeName = $"NonExistentPipe_{Guid.NewGuid():N}",
            ConnectTimeoutMs = 500
        };
        using var client = new PubSubClient(options);

        await Assert.ThrowsExceptionAsync<TimeoutException>(async () =>
        {
            await client.ConnectAsync();
        });
    }

    [TestMethod]
    public void PubSubClient_ConnectAsync_AfterDispose_ShouldThrow()
    {
        var options = new PubSubClientOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        var client = new PubSubClient(options);
        client.Dispose();

        Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () =>
        {
            await client.ConnectAsync();
        });
    }

    #endregion

    #region Subscribe Tests (Without Connection)

    [TestMethod]
    public async Task PubSubClient_SubscribeAsync_NotConnected_ShouldThrow()
    {
        var options = new PubSubClientOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        using var client = new PubSubClient(options);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await client.SubscribeAsync("test/topic", _ => { });
        });
    }

    [TestMethod]
    public async Task PubSubClient_UnsubscribeAsync_NotConnected_ShouldThrow()
    {
        var options = new PubSubClientOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        using var client = new PubSubClient(options);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await client.UnsubscribeAsync("test/topic");
        });
    }

    #endregion

    #region Dispose Tests

    [TestMethod]
    public void PubSubClient_Dispose_ShouldCleanupResources()
    {
        var options = new PubSubClientOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        var client = new PubSubClient(options);

        client.Dispose();

        // Should not throw when disposed multiple times
        client.Dispose();
    }

    [TestMethod]
    public void PubSubClient_Dispose_ShouldNotThrowOnMultipleCalls()
    {
        var options = new PubSubClientOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        var client = new PubSubClient(options);

        // Multiple dispose calls should be safe
        client.Dispose();
        client.Dispose();
        client.Dispose();

        // No exception expected
        Assert.IsFalse(client.IsConnected);
    }

    #endregion

    #region Event Tests

    [TestMethod]
    public void PubSubClient_ConnectionStateChanged_ShouldBeSubscribable()
    {
        var options = new PubSubClientOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        using var client = new PubSubClient(options);

        var eventFired = false;
        client.ConnectionStateChanged += connected => eventFired = true;

        // Event handler should be registered without error
        Assert.IsFalse(eventFired); // Not fired yet since no connection attempt
    }

    [TestMethod]
    public void PubSubClient_MessageReceived_ShouldBeSubscribable()
    {
        var options = new PubSubClientOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        using var client = new PubSubClient(options);

        var eventFired = false;
        client.MessageReceived += message => eventFired = true;

        // Event handler should be registered without error
        Assert.IsFalse(eventFired);
    }

    [TestMethod]
    public void PubSubClient_Error_ShouldBeSubscribable()
    {
        var options = new PubSubClientOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        using var client = new PubSubClient(options);

        var eventFired = false;
        client.Error += exception => eventFired = true;

        // Event handler should be registered without error
        Assert.IsFalse(eventFired);
    }

    #endregion

    #region SubscribedTopics Tests

    [TestMethod]
    public void PubSubClient_SubscribedTopics_InitiallyEmpty()
    {
        var options = new PubSubClientOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        using var client = new PubSubClient(options);

        Assert.AreEqual(0, client.SubscribedTopics.Count);
    }

    #endregion
}
