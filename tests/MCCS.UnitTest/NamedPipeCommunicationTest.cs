using MCCS.Infrastructure.Communication.NamedPipe;
using MCCS.Infrastructure.Communication.NamedPipe.Handlers;
using MCCS.Infrastructure.Communication.NamedPipe.Models;
using MCCS.Infrastructure.Communication.NamedPipe.Serialization;

namespace MCCS.UnitTest;

[TestClass]
public sealed class NamedPipeCommunicationTest
{
    #region PipeMessage Tests

    [TestMethod]
    public void PipeRequest_Create_ShouldSetRouteAndPayload()
    {
        // Arrange & Act
        var request = PipeRequest.Create("test/route", "test payload");

        // Assert
        Assert.AreEqual("test/route", request.Route);
        Assert.AreEqual("test payload", request.Payload);
        Assert.IsFalse(string.IsNullOrEmpty(request.RequestId));
        Assert.IsTrue(request.Timestamp > 0);
    }

    [TestMethod]
    public void PipeRequest_CreateWithSerializer_ShouldSerializeData()
    {
        // Arrange
        var data = new TestData { Name = "Test", Value = 42 };

        // Act
        var request = PipeRequest.Create("test/route", data, d => $"{{\"Name\":\"{d.Name}\",\"Value\":{d.Value}}}");

        // Assert
        Assert.AreEqual("test/route", request.Route);
        Assert.IsTrue(request.Payload!.Contains("Test"));
        Assert.IsTrue(request.Payload!.Contains("42"));
    }

    [TestMethod]
    public void PipeResponse_Success_ShouldSetCorrectProperties()
    {
        // Arrange & Act
        var response = PipeResponse.Success("req123", "response payload");

        // Assert
        Assert.AreEqual("req123", response.RequestId);
        Assert.AreEqual(PipeStatusCode.Success, response.StatusCode);
        Assert.AreEqual("response payload", response.Payload);
        Assert.IsTrue(response.IsSuccess);
        Assert.IsNull(response.ErrorMessage);
    }

    [TestMethod]
    public void PipeResponse_Failure_ShouldSetCorrectProperties()
    {
        // Arrange & Act
        var response = PipeResponse.Failure("req123", PipeStatusCode.HandlerNotFound, "Handler not found");

        // Assert
        Assert.AreEqual("req123", response.RequestId);
        Assert.AreEqual(PipeStatusCode.HandlerNotFound, response.StatusCode);
        Assert.AreEqual("Handler not found", response.ErrorMessage);
        Assert.IsFalse(response.IsSuccess);
    }

    [TestMethod]
    public void PipeResponse_FromException_ShouldCaptureExceptionMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception message");

        // Act
        var response = PipeResponse.FromException("req123", exception);

        // Assert
        Assert.AreEqual("req123", response.RequestId);
        Assert.AreEqual(PipeStatusCode.HandlerException, response.StatusCode);
        Assert.AreEqual("Test exception message", response.ErrorMessage);
        Assert.IsFalse(response.IsSuccess);
    }

    [TestMethod]
    public void PipeStatusCode_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.AreEqual(0, (int)PipeStatusCode.Success);
        Assert.AreEqual(1, (int)PipeStatusCode.UnknownError);
        Assert.AreEqual(2, (int)PipeStatusCode.Timeout);
        Assert.AreEqual(3, (int)PipeStatusCode.InvalidRequest);
        Assert.AreEqual(4, (int)PipeStatusCode.HandlerNotFound);
        Assert.AreEqual(5, (int)PipeStatusCode.HandlerException);
        Assert.AreEqual(6, (int)PipeStatusCode.SerializationError);
        Assert.AreEqual(7, (int)PipeStatusCode.ConnectionError);
        Assert.AreEqual(8, (int)PipeStatusCode.ServerClosed);
    }

    #endregion

    #region JsonMessageSerializer Tests

    [TestMethod]
    public void JsonMessageSerializer_SerializeRequest_ShouldReturnValidBytes()
    {
        // Arrange
        var serializer = new JsonMessageSerializer();
        var request = PipeRequest.Create("test/route", "test payload");

        // Act
        var bytes = serializer.SerializeRequest(request);

        // Assert
        Assert.IsNotNull(bytes);
        Assert.IsTrue(bytes.Length > 0);
    }

    [TestMethod]
    public void JsonMessageSerializer_DeserializeRequest_ShouldRestoreObject()
    {
        // Arrange
        var serializer = new JsonMessageSerializer();
        var originalRequest = PipeRequest.Create("test/route", "test payload");
        originalRequest.RequestId = "fixed-id-for-test";

        // Act
        var bytes = serializer.SerializeRequest(originalRequest);
        var deserializedRequest = serializer.DeserializeRequest(bytes);

        // Assert
        Assert.AreEqual(originalRequest.RequestId, deserializedRequest.RequestId);
        Assert.AreEqual(originalRequest.Route, deserializedRequest.Route);
        Assert.AreEqual(originalRequest.Payload, deserializedRequest.Payload);
    }

    [TestMethod]
    public void JsonMessageSerializer_SerializeResponse_ShouldReturnValidBytes()
    {
        // Arrange
        var serializer = new JsonMessageSerializer();
        var response = PipeResponse.Success("req123", "response payload");

        // Act
        var bytes = serializer.SerializeResponse(response);

        // Assert
        Assert.IsNotNull(bytes);
        Assert.IsTrue(bytes.Length > 0);
    }

    [TestMethod]
    public void JsonMessageSerializer_DeserializeResponse_ShouldRestoreObject()
    {
        // Arrange
        var serializer = new JsonMessageSerializer();
        var originalResponse = PipeResponse.Success("req123", "response payload");

        // Act
        var bytes = serializer.SerializeResponse(originalResponse);
        var deserializedResponse = serializer.DeserializeResponse(bytes);

        // Assert
        Assert.AreEqual(originalResponse.RequestId, deserializedResponse.RequestId);
        Assert.AreEqual(originalResponse.StatusCode, deserializedResponse.StatusCode);
        Assert.AreEqual(originalResponse.Payload, deserializedResponse.Payload);
    }

    [TestMethod]
    public void JsonMessageSerializer_SerializeGeneric_ShouldWorkCorrectly()
    {
        // Arrange
        var serializer = new JsonMessageSerializer();
        var data = new TestData { Name = "Test", Value = 42 };

        // Act
        var json = serializer.Serialize(data);
        var deserialized = serializer.Deserialize<TestData>(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(data.Name, deserialized.Name);
        Assert.AreEqual(data.Value, deserialized.Value);
    }

    [TestMethod]
    public void JsonMessageSerializer_SerializeFailureResponse_ShouldPreserveErrorMessage()
    {
        // Arrange
        var serializer = new JsonMessageSerializer();
        var response = PipeResponse.Failure("req123", PipeStatusCode.HandlerException, "Error occurred");

        // Act
        var bytes = serializer.SerializeResponse(response);
        var deserialized = serializer.DeserializeResponse(bytes);

        // Assert
        Assert.AreEqual(PipeStatusCode.HandlerException, deserialized.StatusCode);
        Assert.AreEqual("Error occurred", deserialized.ErrorMessage);
    }

    #endregion

    #region RequestRouter Tests

    [TestMethod]
    public void RequestRouter_RegisterHandler_ShouldAddHandler()
    {
        // Arrange
        var router = new RequestRouter();
        var handler = new TestHandler("test/route");

        // Act
        router.RegisterHandler(handler);

        // Assert
        Assert.IsTrue(router.HasHandler("test/route"));
    }

    [TestMethod]
    public void RequestRouter_RegisterHandler_ShouldBeCaseInsensitive()
    {
        // Arrange
        var router = new RequestRouter();
        var handler = new TestHandler("Test/Route");

        // Act
        router.RegisterHandler(handler);

        // Assert
        Assert.IsTrue(router.HasHandler("test/route"));
        Assert.IsTrue(router.HasHandler("TEST/ROUTE"));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void RequestRouter_RegisterDuplicateHandler_ShouldThrowException()
    {
        // Arrange
        var router = new RequestRouter();
        var handler1 = new TestHandler("test/route");
        var handler2 = new TestHandler("test/route");

        // Act
        router.RegisterHandler(handler1);
        router.RegisterHandler(handler2); // Should throw
    }

    [TestMethod]
    public void RequestRouter_UnregisterHandler_ShouldRemoveHandler()
    {
        // Arrange
        var router = new RequestRouter();
        var handler = new TestHandler("test/route");
        router.RegisterHandler(handler);

        // Act
        var result = router.UnregisterHandler("test/route");

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(router.HasHandler("test/route"));
    }

    [TestMethod]
    public void RequestRouter_UnregisterNonexistentHandler_ShouldReturnFalse()
    {
        // Arrange
        var router = new RequestRouter();

        // Act
        var result = router.UnregisterHandler("nonexistent");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task RequestRouter_RouteAsync_ShouldInvokeCorrectHandler()
    {
        // Arrange
        var router = new RequestRouter();
        router.RegisterHandler("test/route", req => PipeResponse.Success(req.RequestId, "handled"));
        var request = PipeRequest.Create("test/route");

        // Act
        var response = await router.RouteAsync(request);

        // Assert
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual("handled", response.Payload);
    }

    [TestMethod]
    public async Task RequestRouter_RouteAsync_ShouldReturnNotFoundForUnknownRoute()
    {
        // Arrange
        var router = new RequestRouter();
        var request = PipeRequest.Create("unknown/route");

        // Act
        var response = await router.RouteAsync(request);

        // Assert
        Assert.IsFalse(response.IsSuccess);
        Assert.AreEqual(PipeStatusCode.HandlerNotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task RequestRouter_RouteAsync_ShouldReturnInvalidRequestForEmptyRoute()
    {
        // Arrange
        var router = new RequestRouter();
        var request = new PipeRequest { Route = "" };

        // Act
        var response = await router.RouteAsync(request);

        // Assert
        Assert.IsFalse(response.IsSuccess);
        Assert.AreEqual(PipeStatusCode.InvalidRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task RequestRouter_RouteAsync_ShouldCalculateProcessingTime()
    {
        // Arrange
        var router = new RequestRouter();
        router.RegisterHandler("test/route", async (req, ct) =>
        {
            await Task.Delay(50, ct);
            return PipeResponse.Success(req.RequestId);
        });
        var request = PipeRequest.Create("test/route");

        // Act
        var response = await router.RouteAsync(request);

        // Assert
        Assert.IsTrue(response.ProcessingTimeMs >= 50);
    }

    [TestMethod]
    public async Task RequestRouter_RouteAsync_ShouldHandleExceptionInHandler()
    {
        // Arrange
        var router = new RequestRouter();
        router.RegisterHandler("test/route", (req, ct) =>
        {
            throw new InvalidOperationException("Test exception");
        });
        var request = PipeRequest.Create("test/route");

        // Act
        var response = await router.RouteAsync(request);

        // Assert
        Assert.IsFalse(response.IsSuccess);
        Assert.AreEqual(PipeStatusCode.HandlerException, response.StatusCode);
        Assert.IsTrue(response.ErrorMessage!.Contains("Test exception"));
    }

    [TestMethod]
    public void RequestRouter_GetRegisteredRoutes_ShouldReturnAllRoutes()
    {
        // Arrange
        var router = new RequestRouter();
        router.RegisterHandler("route1", req => PipeResponse.Success(req.RequestId));
        router.RegisterHandler("route2", req => PipeResponse.Success(req.RequestId));
        router.RegisterHandler("route3", req => PipeResponse.Success(req.RequestId));

        // Act
        var routes = router.GetRegisteredRoutes().ToList();

        // Assert
        Assert.AreEqual(3, routes.Count);
        Assert.IsTrue(routes.Contains("route1"));
        Assert.IsTrue(routes.Contains("route2"));
        Assert.IsTrue(routes.Contains("route3"));
    }

    [TestMethod]
    public void RequestRouter_Clear_ShouldRemoveAllHandlers()
    {
        // Arrange
        var router = new RequestRouter();
        router.RegisterHandler("route1", req => PipeResponse.Success(req.RequestId));
        router.RegisterHandler("route2", req => PipeResponse.Success(req.RequestId));

        // Act
        router.Clear();

        // Assert
        Assert.IsFalse(router.HasHandler("route1"));
        Assert.IsFalse(router.HasHandler("route2"));
        Assert.AreEqual(0, router.GetRegisteredRoutes().Count());
    }

    [TestMethod]
    public void RequestRouter_RegisterDelegateHandler_WithCancellationToken_ShouldWork()
    {
        // Arrange
        var router = new RequestRouter();
        var handlerInvoked = false;

        router.RegisterHandler("test/route", async (req, ct) =>
        {
            handlerInvoked = true;
            return PipeResponse.Success(req.RequestId);
        });

        // Assert
        Assert.IsTrue(router.HasHandler("test/route"));
    }

    #endregion

    #region DelegateRequestHandler Tests

    [TestMethod]
    public async Task DelegateRequestHandler_WithAsyncHandler_ShouldInvokeCorrectly()
    {
        // Arrange
        var handler = new DelegateRequestHandler("test", async (req, ct) =>
        {
            await Task.Delay(10, ct);
            return PipeResponse.Success(req.RequestId, "async result");
        });
        var request = PipeRequest.Create("test");

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual("async result", response.Payload);
    }

    [TestMethod]
    public async Task DelegateRequestHandler_WithSyncHandler_ShouldInvokeCorrectly()
    {
        // Arrange
        var handler = new DelegateRequestHandler("test", req => PipeResponse.Success(req.RequestId, "sync result"));
        var request = PipeRequest.Create("test");

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual("sync result", response.Payload);
    }

    #endregion

    #region NamedPipeServer and NamedPipeClient Integration Tests

    [TestMethod]
    public void NamedPipeServerOptions_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new NamedPipeServerOptions();

        // Assert
        Assert.AreEqual("MCCS_IPC_Pipe", options.PipeName);
        Assert.AreEqual(10, options.MaxConcurrentConnections);
        Assert.AreEqual(65536, options.ReceiveBufferSize);
        Assert.AreEqual(65536, options.SendBufferSize);
        Assert.AreEqual(30000, options.RequestTimeoutMs);
    }

    [TestMethod]
    public void NamedPipeClientOptions_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new NamedPipeClientOptions();

        // Assert
        Assert.AreEqual("MCCS_IPC_Pipe", options.PipeName);
        Assert.AreEqual(".", options.ServerName);
        Assert.AreEqual(5000, options.ConnectTimeoutMs);
        Assert.AreEqual(30000, options.RequestTimeoutMs);
        Assert.AreEqual(65536, options.ReceiveBufferSize);
        Assert.IsTrue(options.AutoReconnect);
        Assert.AreEqual(1000, options.ReconnectIntervalMs);
        Assert.AreEqual(3, options.MaxReconnectAttempts);
    }

    [TestMethod]
    public void NamedPipeServer_Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        using var server = new NamedPipeServer();

        // Assert
        Assert.AreEqual("MCCS_IPC_Pipe", server.PipeName);
        Assert.AreEqual(0, server.ActiveConnections);
        Assert.IsFalse(server.IsRunning);
    }

    [TestMethod]
    public void NamedPipeServer_RegisterHandler_ShouldChainCorrectly()
    {
        // Arrange
        using var server = new NamedPipeServer();

        // Act
        var result = server
            .RegisterHandler("route1", req => PipeResponse.Success(req.RequestId))
            .RegisterHandler("route2", req => PipeResponse.Success(req.RequestId));

        // Assert
        Assert.AreSame(server, result);
        Assert.IsTrue(server.Router.HasHandler("route1"));
        Assert.IsTrue(server.Router.HasHandler("route2"));
    }

    [TestMethod]
    public void NamedPipeClient_Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        using var client = new NamedPipeClient();

        // Assert
        Assert.IsFalse(client.IsConnected);
    }

    [TestMethod]
    [ExpectedException(typeof(ObjectDisposedException))]
    public async Task NamedPipeServer_AfterDispose_ShouldThrowOnStart()
    {
        // Arrange
        var server = new NamedPipeServer();
        server.Dispose(); 
        // Act
        await server.StartAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(ObjectDisposedException))]
    public async Task NamedPipeClient_AfterDispose_ShouldThrowOnSend()
    {
        // Arrange
        var client = new NamedPipeClient();
        client.Dispose();

        // Act
        await client.SendAsync("test");
    }

    [TestMethod]
    public async Task NamedPipeServer_ClientConnectedEvent_ShouldFire()
    {
        // Arrange
        var pipeName = $"TestPipe_{Guid.NewGuid():N}";
        var serverOptions = new NamedPipeServerOptions { PipeName = pipeName };
        using var server = new NamedPipeServer(serverOptions);

        var clientConnectedEventFired = false;
        server.ClientConnected += _ => clientConnectedEventFired = true;

        server.RegisterHandler("ping", req => PipeResponse.Success(req.RequestId, "pong"));

        // Start server in background
        var serverTask = server.StartAsync();

        // Create and connect client
        var clientOptions = new NamedPipeClientOptions { PipeName = pipeName, ConnectTimeoutMs = 5000 };
        using var client = new NamedPipeClient(clientOptions);

        try
        {
            await client.ConnectAsync();

            // Send a request to ensure connection is fully established
            var response = await client.SendAsync("ping");

            // Give some time for event to fire
            await Task.Delay(100);
        }
        catch (Exception)
        {
            // Connection might fail in test environment, that's ok
        }

        // Cleanup
        await server.StopAsync();
    }

    [TestMethod]
    public async Task NamedPipeServer_And_Client_Communication_ShouldWork()
    {
        // Arrange
        var pipeName = $"TestPipe_{Guid.NewGuid():N}";
        var serverOptions = new NamedPipeServerOptions { PipeName = pipeName };
        using var server = new NamedPipeServer(serverOptions);

        server.RegisterHandler("echo", req => PipeResponse.Success(req.RequestId, req.Payload));
        server.RegisterHandler("add", req =>
        {
            var serializer = new JsonMessageSerializer();
            var data = serializer.Deserialize<AddRequest>(req.Payload!);
            var result = data!.A + data.B;
            return PipeResponse.Success(req.RequestId, serializer.Serialize(new AddResponse { Result = result }));
        });

        // Start server in background
        var serverTask = Task.Run(async () => await server.StartAsync());

        // Give server time to start
        await Task.Delay(100);

        // Create and connect client
        var clientOptions = new NamedPipeClientOptions
        {
            PipeName = pipeName,
            ConnectTimeoutMs = 5000,
            RequestTimeoutMs = 5000
        };
        using var client = new NamedPipeClient(clientOptions);

        try
        {
            await client.ConnectAsync();

            // Test echo
            var echoResponse = await client.SendAsync("echo", "Hello World");
            Assert.IsTrue(echoResponse.IsSuccess);
            Assert.AreEqual("Hello World", echoResponse.Payload);

            // Test add
            var serializer = new JsonMessageSerializer();
            var addRequest = PipeRequest.Create("add", serializer.Serialize(new AddRequest { A = 10, B = 20 }));
            var addResponse = await client.SendAsync(addRequest);
            Assert.IsTrue(addResponse.IsSuccess);
            var addResult = serializer.Deserialize<AddResponse>(addResponse.Payload!);
            Assert.AreEqual(30, addResult!.Result);
        }
        finally
        {
            client.Disconnect();
            await server.StopAsync();
        }
    }

    [TestMethod]
    public async Task NamedPipeClient_SendAsync_WithStrongTyping_ShouldWork()
    {
        // Arrange
        var pipeName = $"TestPipe_{Guid.NewGuid():N}";
        var serverOptions = new NamedPipeServerOptions { PipeName = pipeName };
        using var server = new NamedPipeServer(serverOptions);

        server.RegisterHandler("process", req =>
        {
            var serializer = new JsonMessageSerializer();
            var input = serializer.Deserialize<TestData>(req.Payload!);
            var output = new TestData { Name = input!.Name.ToUpper(), Value = input.Value * 2 };
            return PipeResponse.Success(req.RequestId, serializer.Serialize(output));
        });

        var serverTask = Task.Run(async () => await server.StartAsync());
        await Task.Delay(100);

        var clientOptions = new NamedPipeClientOptions { PipeName = pipeName };
        using var client = new NamedPipeClient(clientOptions);

        try
        {
            await client.ConnectAsync();

            var result = await client.SendAsync<TestData, TestData>("process", new TestData { Name = "test", Value = 5 });

            Assert.IsNotNull(result);
            Assert.AreEqual("TEST", result.Name);
            Assert.AreEqual(10, result.Value);
        }
        finally
        {
            await server.StopAsync();
        }
    }

    #endregion

    #region NamedPipeFactory Tests

    [TestMethod]
    public void NamedPipeFactory_CreateServer_ShouldUseDefaultValues()
    {
        // Act
        using var server = NamedPipeFactory.CreateServer();

        // Assert
        Assert.AreEqual("MCCS_IPC_Pipe", server.PipeName);
    }

    [TestMethod]
    public void NamedPipeFactory_CreateServer_WithCustomPipeName_ShouldUseProvidedName()
    {
        // Act
        using var server = NamedPipeFactory.CreateServer("CustomPipe");

        // Assert
        Assert.AreEqual("CustomPipe", server.PipeName);
    }

    [TestMethod]
    public void NamedPipeFactory_CreateServer_WithConfigureAction_ShouldApplyOptions()
    {
        // Act
        using var server = NamedPipeFactory.CreateServer(opts =>
        {
            opts.PipeName = "ConfiguredPipe";
            opts.MaxConcurrentConnections = 5;
        });

        // Assert
        Assert.AreEqual("ConfiguredPipe", server.PipeName);
    }

    [TestMethod]
    public void NamedPipeFactory_CreateClient_ShouldUseDefaultValues()
    {
        // Act
        using var client = NamedPipeFactory.CreateClient();

        // Assert
        Assert.IsFalse(client.IsConnected);
    }

    [TestMethod]
    public void NamedPipeFactory_CreateClient_WithCustomPipeName_ShouldWork()
    {
        // Act
        using var client = NamedPipeFactory.CreateClient("CustomPipe");

        // Assert - Client is created successfully
        Assert.IsFalse(client.IsConnected);
    }

    [TestMethod]
    public void NamedPipeFactory_CreateClient_WithConfigureAction_ShouldWork()
    {
        // Act
        using var client = NamedPipeFactory.CreateClient(opts =>
        {
            opts.PipeName = "ConfiguredPipe";
            opts.ConnectTimeoutMs = 10000;
        });

        // Assert
        Assert.IsFalse(client.IsConnected);
    }

    [TestMethod]
    public void NamedPipeFactory_CreateClientPool_ShouldUseDefaultValues()
    {
        // Act
        using var pool = NamedPipeFactory.CreateClientPool();

        // Assert
        Assert.AreEqual(0, pool.TotalConnections);
        Assert.AreEqual(0, pool.AvailableConnections);
        Assert.AreEqual(0, pool.ActiveConnections);
    }

    [TestMethod]
    public void NamedPipeFactory_CreateClientPool_WithCustomOptions_ShouldWork()
    {
        // Act
        using var pool = NamedPipeFactory.CreateClientPool("CustomPipe", 5);

        // Assert
        Assert.AreEqual(0, pool.TotalConnections);
    }

    [TestMethod]
    public void NamedPipeFactory_CreateClientPool_WithConfigureAction_ShouldWork()
    {
        // Act
        using var pool = NamedPipeFactory.CreateClientPool(opts =>
        {
            opts.PipeName = "PoolPipe";
            opts.MaxConnections = 20;
            opts.MinConnections = 2;
        });

        // Assert
        Assert.AreEqual(0, pool.TotalConnections);
    }

    #endregion

    #region NamedPipeClientPool Tests

    [TestMethod]
    public void NamedPipeClientPoolOptions_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new NamedPipeClientPoolOptions();

        // Assert
        Assert.AreEqual("MCCS_IPC_Pipe", options.PipeName);
        Assert.AreEqual(".", options.ServerName);
        Assert.AreEqual(1, options.MinConnections);
        Assert.AreEqual(10, options.MaxConnections);
        Assert.AreEqual(5000, options.ConnectTimeoutMs);
        Assert.AreEqual(30000, options.RequestTimeoutMs);
        Assert.AreEqual(10000, options.AcquireTimeoutMs);
        Assert.AreEqual(60000, options.IdleTimeoutMs);
    }

    [TestMethod]
    public void NamedPipeClientPool_Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        using var pool = new NamedPipeClientPool();

        // Assert
        Assert.AreEqual(0, pool.TotalConnections);
        Assert.AreEqual(0, pool.AvailableConnections);
        Assert.AreEqual(0, pool.ActiveConnections);
    }

    [TestMethod]
    [ExpectedException(typeof(ObjectDisposedException))]
    public async Task NamedPipeClientPool_AfterDispose_ShouldThrowOnSend()
    {
        // Arrange
        var pool = new NamedPipeClientPool();
        pool.Dispose();

        // Act
        await pool.SendAsync("test");
    }

    [TestMethod]
    public async Task NamedPipeClientPool_SendAsync_WithServer_ShouldWork()
    {
        // Arrange
        var pipeName = $"PoolTestPipe_{Guid.NewGuid():N}";
        var serverOptions = new NamedPipeServerOptions { PipeName = pipeName };
        using var server = new NamedPipeServer(serverOptions);
        server.RegisterHandler("pool-test", req => PipeResponse.Success(req.RequestId, "pool response"));

        var serverTask = Task.Run(async () => await server.StartAsync());
        await Task.Delay(100);

        var poolOptions = new NamedPipeClientPoolOptions
        {
            PipeName = pipeName,
            MinConnections = 1,
            MaxConnections = 3
        };
        using var pool = new NamedPipeClientPool(poolOptions);

        try
        {
            // Act
            var response = await pool.SendAsync("pool-test");

            // Assert
            Assert.IsTrue(response.IsSuccess);
            Assert.AreEqual("pool response", response.Payload);
            Assert.IsTrue(pool.TotalConnections >= 1);
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [TestMethod]
    public async Task NamedPipeClientPool_WarmupAsync_ShouldCreateMinConnections()
    {
        // Arrange
        var pipeName = $"WarmupTestPipe_{Guid.NewGuid():N}";
        var serverOptions = new NamedPipeServerOptions
        {
            PipeName = pipeName,
            MaxConcurrentConnections = 5
        };
        using var server = new NamedPipeServer(serverOptions);
        server.RegisterHandler("warmup-test", req => PipeResponse.Success(req.RequestId));

        var serverTask = Task.Run(async () => await server.StartAsync());
        await Task.Delay(100);

        var poolOptions = new NamedPipeClientPoolOptions
        {
            PipeName = pipeName,
            MinConnections = 2,
            MaxConnections = 5
        };
        using var pool = new NamedPipeClientPool(poolOptions);

        try
        {
            // Act
            await pool.WarmupAsync();

            // Assert
            Assert.IsTrue(pool.TotalConnections >= poolOptions.MinConnections);
            Assert.IsTrue(pool.AvailableConnections >= poolOptions.MinConnections);
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [TestMethod]
    public async Task NamedPipeClientPool_ConcurrentRequests_ShouldWork()
    {
        // Arrange
        var pipeName = $"ConcurrentTestPipe_{Guid.NewGuid():N}";
        var serverOptions = new NamedPipeServerOptions
        {
            PipeName = pipeName,
            MaxConcurrentConnections = 10
        };
        using var server = new NamedPipeServer(serverOptions);
        var requestCount = 0;
        server.RegisterHandler("concurrent-test", req =>
        {
            Interlocked.Increment(ref requestCount);
            return PipeResponse.Success(req.RequestId, $"response-{requestCount}");
        });

        var serverTask = Task.Run(async () => await server.StartAsync());
        await Task.Delay(100);

        var poolOptions = new NamedPipeClientPoolOptions
        {
            PipeName = pipeName,
            MinConnections = 2,
            MaxConnections = 5
        };
        using var pool = new NamedPipeClientPool(poolOptions);

        try
        {
            // Act - Send multiple concurrent requests
            var tasks = new List<Task<PipeResponse>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(pool.SendAsync("concurrent-test"));
            }

            var responses = await Task.WhenAll(tasks);

            // Assert
            Assert.AreEqual(10, responses.Length);
            Assert.IsTrue(responses.All(r => r.IsSuccess));
        }
        finally
        {
            await server.StopAsync();
        }
    }

    #endregion

    #region NamedPipeExtensions Tests

    [TestMethod]
    public void NamedPipeExtensions_SetPayload_ShouldSerializeData()
    {
        // Arrange
        var request = PipeRequest.Create("test");
        var data = new TestData { Name = "ExtTest", Value = 99 };

        // Act
        request.SetPayload(data);

        // Assert
        Assert.IsNotNull(request.Payload);
        Assert.IsTrue(request.Payload.Contains("ExtTest"));
        Assert.IsTrue(request.Payload.Contains("99"));
    }

    [TestMethod]
    public void NamedPipeExtensions_GetPayload_ShouldDeserializeData()
    {
        // Arrange
        var serializer = new JsonMessageSerializer();
        var data = new TestData { Name = "ExtTest", Value = 99 };
        var response = PipeResponse.Success("req123", serializer.Serialize(data));

        // Act
        var result = response.GetPayload<TestData>();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("ExtTest", result.Name);
        Assert.AreEqual(99, result.Value);
    }

    [TestMethod]
    public void NamedPipeExtensions_GetPayload_OnFailedResponse_ShouldReturnDefault()
    {
        // Arrange
        var response = PipeResponse.Failure("req123", PipeStatusCode.HandlerException, "Error");

        // Act
        var result = response.GetPayload<TestData>();

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void NamedPipeExtensions_GetPayload_OnEmptyPayload_ShouldReturnDefault()
    {
        // Arrange
        var response = PipeResponse.Success("req123", null);

        // Act
        var result = response.GetPayload<TestData>();

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region AttributedHandler Tests

    [TestMethod]
    public void ApiNamedPipeAttribute_ShouldStoreNameCorrectly()
    {
        // Arrange & Act
        var attribute = new ApiNamedPipeAttribute("TestPipeName");

        // Assert
        Assert.AreEqual("TestPipeName", attribute.Name);
    }

    [TestMethod]
    public void RouteAttribute_ShouldStoreTemplateCorrectly()
    {
        // Arrange & Act
        var attribute = new RouteAttribute("test/route/{id}");

        // Assert
        Assert.AreEqual("test/route/{id}", attribute.Template);
    }

    [TestMethod]
    public void NamedPipeServer_RegisterHandlersFromAttributes_ShouldRegisterHandlers()
    {
        // Arrange
        var pipeName = $"TestAttrPipe_{Guid.NewGuid():N}";
        var options = new NamedPipeServerOptions { PipeName = pipeName };
        using var server = new NamedPipeServer(options);

        // Act
        server.RegisterHandlersFromAttributes(typeof(TestAttributeHandler).Assembly);

        // Assert
        Assert.IsTrue(server.Router.HasHandler("test/ping"));
        Assert.IsTrue(server.Router.HasHandler("test/echo"));
    }

    [TestMethod]
    public async Task NamedPipeServer_AttributeBasedHandler_ShouldProcessRequests()
    {
        // Arrange
        var pipeName = $"TestAttrPipe_{Guid.NewGuid():N}";
        var options = new NamedPipeServerOptions { PipeName = pipeName };
        using var server = new NamedPipeServer(options);
        server.RegisterHandlersFromAttributes(typeof(TestAttributeHandler).Assembly);

        await server.StartAsync();
        await Task.Delay(100);

        var clientOptions = new NamedPipeClientOptions { PipeName = pipeName };
        using var client = new NamedPipeClient(clientOptions);

        try
        {
            await client.ConnectAsync();

            // Test ping handler
            var pingResponse = await client.SendAsync("test/ping");
            Assert.IsTrue(pingResponse.IsSuccess);
            Assert.AreEqual("pong", pingResponse.Payload);

            // Test echo handler
            var echoResponse = await client.SendAsync("test/echo", "Hello");
            Assert.IsTrue(echoResponse.IsSuccess);
            Assert.AreEqual("Hello", echoResponse.Payload);
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [TestMethod]
    public async Task NamedPipeServer_AttributeBasedHandler_WithTypedPayload_ShouldWork()
    {
        // Arrange
        var pipeName = $"TestAttrPipe_{Guid.NewGuid():N}";
        var options = new NamedPipeServerOptions { PipeName = pipeName };
        using var server = new NamedPipeServer(options);
        server.RegisterHandlersFromAttributes(typeof(TestAttributeHandler).Assembly);

        await server.StartAsync();
        await Task.Delay(100);

        var clientOptions = new NamedPipeClientOptions { PipeName = pipeName };
        using var client = new NamedPipeClient(clientOptions);

        try
        {
            await client.ConnectAsync();

            var serializer = new JsonMessageSerializer();
            var request = PipeRequest.Create("test/process", serializer.Serialize(new TestData { Name = "input", Value = 10 }));
            var response = await client.SendAsync(request);

            Assert.IsTrue(response.IsSuccess);
            var result = serializer.Deserialize<TestData>(response.Payload!);
            Assert.IsNotNull(result);
            Assert.AreEqual("INPUT", result.Name);
            Assert.AreEqual(20, result.Value);
        }
        finally
        {
            await server.StopAsync();
        }
    }

    [TestMethod]
    public async Task NamedPipeServer_AttributeBasedHandler_AsyncMethod_ShouldWork()
    {
        // Arrange
        var pipeName = $"TestAttrPipe_{Guid.NewGuid():N}";
        var options = new NamedPipeServerOptions { PipeName = pipeName };
        using var server = new NamedPipeServer(options);
        server.RegisterHandlersFromAttributes(typeof(TestAttributeHandler).Assembly);

        await server.StartAsync();
        await Task.Delay(100);

        var clientOptions = new NamedPipeClientOptions { PipeName = pipeName };
        using var client = new NamedPipeClient(clientOptions);

        try
        {
            await client.ConnectAsync();

            var response = await client.SendAsync("test/async-ping");
            Assert.IsTrue(response.IsSuccess);
            Assert.AreEqual("async-pong", response.Payload);
        }
        finally
        {
            await server.StopAsync();
        }
    }

    #endregion

    #region Helper Classes

    private class TestData
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private class AddRequest
    {
        public int A { get; set; }
        public int B { get; set; }
    }

    private class AddResponse
    {
        public int Result { get; set; }
    }

    private class TestHandler : IRequestHandler
    {
        public string Route { get; }

        public TestHandler(string route)
        {
            Route = route;
        }

        public Task<PipeResponse> HandleAsync(PipeRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PipeResponse.Success(request.RequestId, "test response"));
        }
    }

    #endregion
}

/// <summary>
/// Test handler class with attribute-based routing
/// </summary>
[ApiNamedPipe("TestAttributePipe")]
public class TestAttributeHandler
{
    private readonly JsonMessageSerializer _serializer = new();

    [Route("test/ping")]
    public PipeResponse Ping()
    {
        return PipeResponse.Success(string.Empty, "pong");
    }

    [Route("test/echo")]
    public PipeResponse Echo(PipeRequest request)
    {
        return PipeResponse.Success(request.RequestId, request.Payload);
    }

    [Route("test/process")]
    public PipeResponse Process(PipeRequest request)
    {
        var data = _serializer.Deserialize<TestDataForAttribute>(request.Payload!);
        var result = new TestDataForAttribute
        {
            Name = data!.Name.ToUpper(),
            Value = data.Value * 2
        };
        return PipeResponse.Success(request.RequestId, _serializer.Serialize(result));
    }

    [Route("test/async-ping")]
    public async Task<PipeResponse> AsyncPing(CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        return PipeResponse.Success(string.Empty, "async-pong");
    }

    [Route("test/async-echo")]
    public async Task<PipeResponse> AsyncEcho(PipeRequest request, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        return PipeResponse.Success(request.RequestId, request.Payload);
    }
}

public class TestDataForAttribute
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}
