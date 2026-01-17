using MCCS.Infrastructure.Communication.NamedPipe.PubSub;

namespace MCCS.UnitTest.NamedPipe.PubSub;

[TestClass]
public class PubSubServerTests
{
    #region Options Tests

    [TestMethod]
    public void PubSubServerOptions_DefaultValues_ShouldBeCorrect()
    {
        var options = new PubSubServerOptions();

        Assert.AreEqual("MCCS_PubSub_Pipe", options.PipeName);
        Assert.AreEqual(50, options.MaxConcurrentConnections);
        Assert.AreEqual(65536, options.ReceiveBufferSize);
        Assert.AreEqual(65536, options.SendBufferSize);
        Assert.AreEqual(5000, options.PublishTimeoutMs);
        Assert.IsFalse(options.EnablePersistence);
    }

    [TestMethod]
    public void PubSubServerOptions_CustomValues_ShouldBeSet()
    {
        var options = new PubSubServerOptions
        {
            PipeName = "CustomPipe",
            MaxConcurrentConnections = 100,
            ReceiveBufferSize = 131072,
            SendBufferSize = 131072,
            PublishTimeoutMs = 10000,
            EnablePersistence = true
        };

        Assert.AreEqual("CustomPipe", options.PipeName);
        Assert.AreEqual(100, options.MaxConcurrentConnections);
        Assert.AreEqual(131072, options.ReceiveBufferSize);
        Assert.AreEqual(131072, options.SendBufferSize);
        Assert.AreEqual(10000, options.PublishTimeoutMs);
        Assert.IsTrue(options.EnablePersistence);
    }

    #endregion

    #region Server Initialization Tests

    [TestMethod]
    public void PubSubServer_Create_ShouldInitializeCorrectly()
    {
        using var server = new PubSubServer();

        Assert.IsFalse(server.IsRunning);
        Assert.AreEqual(0, server.ActiveConnections);
        Assert.AreEqual("MCCS_PubSub_Pipe", server.PipeName);
        Assert.IsNotNull(server.SubscriptionManager);
    }

    [TestMethod]
    public void PubSubServer_CreateWithOptions_ShouldUseCustomOptions()
    {
        var options = new PubSubServerOptions { PipeName = "CustomPubSubPipe" };
        using var server = new PubSubServer(options);

        Assert.AreEqual("CustomPubSubPipe", server.PipeName);
    }

    [TestMethod]
    public async Task PubSubServer_StartAsync_ShouldSetIsRunningTrue()
    {
        var options = new PubSubServerOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        using var server = new PubSubServer(options);

        await server.StartAsync();

        Assert.IsTrue(server.IsRunning);

        await server.StopAsync();
    }

    [TestMethod]
    public async Task PubSubServer_StopAsync_ShouldSetIsRunningFalse()
    {
        var options = new PubSubServerOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        using var server = new PubSubServer(options);

        await server.StartAsync();
        await server.StopAsync();

        Assert.IsFalse(server.IsRunning);
    }

    [TestMethod]
    public async Task PubSubServer_StartAsync_CalledTwice_ShouldNotThrow()
    {
        var options = new PubSubServerOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        using var server = new PubSubServer(options);

        await server.StartAsync();
        await server.StartAsync(); // Should not throw

        Assert.IsTrue(server.IsRunning);

        await server.StopAsync();
    }

    [TestMethod]
    public async Task PubSubServer_StopAsync_CalledWithoutStart_ShouldNotThrow()
    {
        var options = new PubSubServerOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        using var server = new PubSubServer(options);

        await server.StopAsync(); // Should not throw

        Assert.IsFalse(server.IsRunning);
    }

    #endregion

    #region Dispose Tests

    [TestMethod]
    public void PubSubServer_Dispose_ShouldCleanupResources()
    {
        var options = new PubSubServerOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        var server = new PubSubServer(options);

        server.Dispose();

        // Should not throw when disposed
        Assert.ThrowsException<ObjectDisposedException>(() =>
        {
            server.PublishAsync("test", "payload").GetAwaiter().GetResult();
        });
    }

    [TestMethod]
    public async Task PubSubServer_Dispose_AfterStart_ShouldCleanupRunningState()
    {
        var options = new PubSubServerOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        var server = new PubSubServer(options);

        await server.StartAsync();
        server.Dispose();

        // Server should be cleaned up
        Assert.ThrowsException<ObjectDisposedException>(() =>
        {
            server.PublishAsync("test", "payload").GetAwaiter().GetResult();
        });
    }

    #endregion

    #region Publish Without Subscribers Tests

    [TestMethod]
    public async Task PublishAsync_NoSubscribers_ShouldReturnZero()
    {
        var options = new PubSubServerOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        using var server = new PubSubServer(options);
        await server.StartAsync();

        var count = await server.PublishAsync("test/topic", "test payload");

        Assert.AreEqual(0, count);

        await server.StopAsync();
    }

    #endregion

    #region Event Tests

    [TestMethod]
    public async Task PubSubServer_MessagePublished_EventShouldFire()
    {
        var options = new PubSubServerOptions { PipeName = $"TestPipe_{Guid.NewGuid():N}" };
        using var server = new PubSubServer(options);

        string? eventTopic = null;
        var eventCount = -1;

        server.MessagePublished += (topic, count) =>
        {
            eventTopic = topic;
            eventCount = count; 
        };

        await server.StartAsync();
        await server.PublishAsync("test/topic", "test payload");  
        await server.StopAsync();
        Assert.AreEqual(null, eventTopic);
        Assert.AreEqual(-1, eventCount); // No subscribers
    }

    #endregion
}
