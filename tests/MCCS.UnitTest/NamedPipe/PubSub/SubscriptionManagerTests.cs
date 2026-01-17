using System.IO.Pipes;
using MCCS.Infrastructure.Communication.NamedPipe.PubSub;

namespace MCCS.UnitTest.NamedPipe.PubSub;

[TestClass]
public class SubscriptionManagerTests
{
    private SubscriptionManager _manager = null!;

    [TestInitialize]
    public void Setup()
    {
        _manager = new SubscriptionManager();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _manager.Clear();
    }

    #region Subscribe Tests

    [TestMethod]
    public void Subscribe_ShouldAddSubscription()
    {
        using var pipeServer = CreateMockPipeServer();

        var subscription = _manager.Subscribe("subscriber-1", "test/topic", pipeServer);

        Assert.IsNotNull(subscription);
        Assert.AreEqual("subscriber-1", subscription.SubscriberId);
        Assert.AreEqual("test/topic", subscription.Topic);
        Assert.IsTrue(_manager.IsSubscribed("subscriber-1", "test/topic"));
    }

    [TestMethod]
    public void Subscribe_SameSubscriberMultipleTopics_ShouldAddAllSubscriptions()
    {
        using var pipeServer = CreateMockPipeServer();

        _manager.Subscribe("subscriber-1", "topic1", pipeServer);
        _manager.Subscribe("subscriber-1", "topic2", pipeServer);
        _manager.Subscribe("subscriber-1", "topic3", pipeServer);

        var subscriptions = _manager.GetSubscriptions("subscriber-1");
        Assert.AreEqual(3, subscriptions.Count);
        Assert.IsTrue(subscriptions.Contains("topic1"));
        Assert.IsTrue(subscriptions.Contains("topic2"));
        Assert.IsTrue(subscriptions.Contains("topic3"));
    }

    [TestMethod]
    public void Subscribe_MultipleSubscribersSameTopic_ShouldAddAllSubscribers()
    {
        using var pipeServer1 = CreateMockPipeServer();
        using var pipeServer2 = CreateMockPipeServer();
        using var pipeServer3 = CreateMockPipeServer();

        _manager.Subscribe("subscriber-1", "shared/topic", pipeServer1);
        _manager.Subscribe("subscriber-2", "shared/topic", pipeServer2);
        _manager.Subscribe("subscriber-3", "shared/topic", pipeServer3);

        var subscribers = _manager.GetSubscribers("shared/topic");
        Assert.AreEqual(3, subscribers.Count);
    }

    [TestMethod]
    public void Subscribe_DuplicateSubscription_ShouldUpdateExisting()
    {
        using var pipeServer1 = CreateMockPipeServer();
        using var pipeServer2 = CreateMockPipeServer();

        _manager.Subscribe("subscriber-1", "test/topic", pipeServer1);
        _manager.Subscribe("subscriber-1", "test/topic", pipeServer2);

        var subscribers = _manager.GetSubscribers("test/topic");
        Assert.AreEqual(1, subscribers.Count);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Subscribe_WithNullSubscriberId_ShouldThrowException()
    {
        using var pipeServer = CreateMockPipeServer();
        _manager.Subscribe(null!, "test/topic", pipeServer);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Subscribe_WithEmptyTopic_ShouldThrowException()
    {
        using var pipeServer = CreateMockPipeServer();
        _manager.Subscribe("subscriber-1", "", pipeServer);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Subscribe_WithNullPipeStream_ShouldThrowException()
    {
        _manager.Subscribe("subscriber-1", "test/topic", null!);
    }

    #endregion

    #region Unsubscribe Tests

    [TestMethod]
    public void Unsubscribe_SpecificTopic_ShouldRemoveSubscription()
    {
        using var pipeServer = CreateMockPipeServer();
        _manager.Subscribe("subscriber-1", "topic1", pipeServer);
        _manager.Subscribe("subscriber-1", "topic2", pipeServer);

        var result = _manager.Unsubscribe("subscriber-1", "topic1");

        Assert.IsTrue(result);
        Assert.IsFalse(_manager.IsSubscribed("subscriber-1", "topic1"));
        Assert.IsTrue(_manager.IsSubscribed("subscriber-1", "topic2"));
    }

    [TestMethod]
    public void Unsubscribe_AllTopics_ShouldRemoveAllSubscriptions()
    {
        using var pipeServer = CreateMockPipeServer();
        _manager.Subscribe("subscriber-1", "topic1", pipeServer);
        _manager.Subscribe("subscriber-1", "topic2", pipeServer);
        _manager.Subscribe("subscriber-1", "topic3", pipeServer);

        var result = _manager.Unsubscribe("subscriber-1", null);

        Assert.IsTrue(result);
        Assert.AreEqual(0, _manager.GetSubscriptions("subscriber-1").Count);
    }

    [TestMethod]
    public void Unsubscribe_NonExistentSubscription_ShouldReturnTrue()
    {
        var result = _manager.Unsubscribe("non-existent", "topic");

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Unsubscribe_LastSubscriberFromTopic_ShouldRemoveTopic()
    {
        using var pipeServer = CreateMockPipeServer();
        _manager.Subscribe("subscriber-1", "test/topic", pipeServer);

        _manager.Unsubscribe("subscriber-1", "test/topic");

        Assert.AreEqual(0, _manager.GetSubscriberCount("test/topic"));
        Assert.IsFalse(_manager.GetAllTopics().Contains("test/topic"));
    }

    #endregion

    #region RemoveSubscriber Tests

    [TestMethod]
    public void RemoveSubscriber_ShouldRemoveAllSubscriptions()
    {
        using var pipeServer = CreateMockPipeServer();
        _manager.Subscribe("subscriber-1", "topic1", pipeServer);
        _manager.Subscribe("subscriber-1", "topic2", pipeServer);
        _manager.Subscribe("subscriber-1", "topic3", pipeServer);

        _manager.RemoveSubscriber("subscriber-1");

        Assert.AreEqual(0, _manager.GetSubscriptions("subscriber-1").Count);
        Assert.AreEqual(0, _manager.GetSubscriberCount("topic1"));
        Assert.AreEqual(0, _manager.GetSubscriberCount("topic2"));
        Assert.AreEqual(0, _manager.GetSubscriberCount("topic3"));
    }

    [TestMethod]
    public void RemoveSubscriber_ShouldNotAffectOtherSubscribers()
    {
        using var pipeServer1 = CreateMockPipeServer();
        using var pipeServer2 = CreateMockPipeServer();
        _manager.Subscribe("subscriber-1", "shared/topic", pipeServer1);
        _manager.Subscribe("subscriber-2", "shared/topic", pipeServer2);

        _manager.RemoveSubscriber("subscriber-1");

        Assert.AreEqual(1, _manager.GetSubscriberCount("shared/topic"));
        Assert.IsTrue(_manager.IsSubscribed("subscriber-2", "shared/topic"));
    }

    [TestMethod]
    public void RemoveSubscriber_NonExistent_ShouldNotThrow()
    {
        // Should not throw
        _manager.RemoveSubscriber("non-existent");
    }

    #endregion

    #region GetSubscribers Tests

    [TestMethod]
    public void GetSubscribers_ShouldReturnAllSubscribersForTopic()
    {
        using var pipeServer1 = CreateMockPipeServer();
        using var pipeServer2 = CreateMockPipeServer();
        _manager.Subscribe("subscriber-1", "test/topic", pipeServer1);
        _manager.Subscribe("subscriber-2", "test/topic", pipeServer2);

        var subscribers = _manager.GetSubscribers("test/topic");

        Assert.AreEqual(2, subscribers.Count);
        Assert.IsTrue(subscribers.Any(s => s.SubscriberId == "subscriber-1"));
        Assert.IsTrue(subscribers.Any(s => s.SubscriberId == "subscriber-2"));
    }

    [TestMethod]
    public void GetSubscribers_NonExistentTopic_ShouldReturnEmptyList()
    {
        var subscribers = _manager.GetSubscribers("non-existent");

        Assert.IsNotNull(subscribers);
        Assert.AreEqual(0, subscribers.Count);
    }

    #endregion

    #region GetSubscriptions Tests

    [TestMethod]
    public void GetSubscriptions_ShouldReturnAllTopicsForSubscriber()
    {
        using var pipeServer = CreateMockPipeServer();
        _manager.Subscribe("subscriber-1", "topic1", pipeServer);
        _manager.Subscribe("subscriber-1", "topic2", pipeServer);

        var topics = _manager.GetSubscriptions("subscriber-1");

        Assert.AreEqual(2, topics.Count);
        Assert.IsTrue(topics.Contains("topic1"));
        Assert.IsTrue(topics.Contains("topic2"));
    }

    [TestMethod]
    public void GetSubscriptions_NonExistentSubscriber_ShouldReturnEmptyList()
    {
        var topics = _manager.GetSubscriptions("non-existent");

        Assert.IsNotNull(topics);
        Assert.AreEqual(0, topics.Count);
    }

    #endregion

    #region IsSubscribed Tests

    [TestMethod]
    public void IsSubscribed_ExistingSubscription_ShouldReturnTrue()
    {
        using var pipeServer = CreateMockPipeServer();
        _manager.Subscribe("subscriber-1", "test/topic", pipeServer);

        Assert.IsTrue(_manager.IsSubscribed("subscriber-1", "test/topic"));
    }

    [TestMethod]
    public void IsSubscribed_NonExistentSubscription_ShouldReturnFalse()
    {
        Assert.IsFalse(_manager.IsSubscribed("subscriber-1", "test/topic"));
    }

    [TestMethod]
    public void IsSubscribed_CaseInsensitive_ShouldMatch()
    {
        using var pipeServer = CreateMockPipeServer();
        _manager.Subscribe("SUBSCRIBER-1", "TEST/TOPIC", pipeServer);

        // Topic matching should be case-insensitive
        Assert.IsTrue(_manager.IsSubscribed("subscriber-1", "test/topic"));
    }

    #endregion

    #region GetAllTopics Tests

    [TestMethod]
    public void GetAllTopics_ShouldReturnAllUniqueTopics()
    {
        using var pipeServer1 = CreateMockPipeServer();
        using var pipeServer2 = CreateMockPipeServer();
        _manager.Subscribe("subscriber-1", "topic1", pipeServer1);
        _manager.Subscribe("subscriber-1", "topic2", pipeServer1);
        _manager.Subscribe("subscriber-2", "topic2", pipeServer2);
        _manager.Subscribe("subscriber-2", "topic3", pipeServer2);

        var topics = _manager.GetAllTopics();

        Assert.AreEqual(3, topics.Count);
        Assert.IsTrue(topics.Contains("topic1"));
        Assert.IsTrue(topics.Contains("topic2"));
        Assert.IsTrue(topics.Contains("topic3"));
    }

    [TestMethod]
    public void GetAllTopics_Empty_ShouldReturnEmptyList()
    {
        var topics = _manager.GetAllTopics();

        Assert.IsNotNull(topics);
        Assert.AreEqual(0, topics.Count);
    }

    #endregion

    #region GetSubscriberCount Tests

    [TestMethod]
    public void GetSubscriberCount_ShouldReturnCorrectCount()
    {
        using var pipeServer1 = CreateMockPipeServer();
        using var pipeServer2 = CreateMockPipeServer();
        using var pipeServer3 = CreateMockPipeServer();
        _manager.Subscribe("subscriber-1", "test/topic", pipeServer1);
        _manager.Subscribe("subscriber-2", "test/topic", pipeServer2);
        _manager.Subscribe("subscriber-3", "test/topic", pipeServer3);

        Assert.AreEqual(3, _manager.GetSubscriberCount("test/topic"));
    }

    [TestMethod]
    public void GetSubscriberCount_NonExistentTopic_ShouldReturnZero()
    {
        Assert.AreEqual(0, _manager.GetSubscriberCount("non-existent"));
    }

    #endregion

    #region Clear Tests

    [TestMethod]
    public void Clear_ShouldRemoveAllSubscriptions()
    {
        using var pipeServer1 = CreateMockPipeServer();
        using var pipeServer2 = CreateMockPipeServer();
        _manager.Subscribe("subscriber-1", "topic1", pipeServer1);
        _manager.Subscribe("subscriber-2", "topic2", pipeServer2);

        _manager.Clear();

        Assert.AreEqual(0, _manager.GetAllTopics().Count);
        Assert.AreEqual(0, _manager.GetSubscriptions("subscriber-1").Count);
        Assert.AreEqual(0, _manager.GetSubscriptions("subscriber-2").Count);
    }

    #endregion

    #region Event Tests

    [TestMethod]
    public void Subscribe_ShouldTriggerSubscriptionAddedEvent()
    {
        using var pipeServer = CreateMockPipeServer();
        string? eventSubscriberId = null;
        string? eventTopic = null;

        _manager.SubscriptionAdded += (subscriberId, topic) =>
        {
            eventSubscriberId = subscriberId;
            eventTopic = topic;
        };

        _manager.Subscribe("subscriber-1", "test/topic", pipeServer);

        Assert.AreEqual("subscriber-1", eventSubscriberId);
        Assert.AreEqual("test/topic", eventTopic);
    }

    [TestMethod]
    public void Unsubscribe_ShouldTriggerSubscriptionRemovedEvent()
    {
        using var pipeServer = CreateMockPipeServer();
        _manager.Subscribe("subscriber-1", "test/topic", pipeServer);

        string? eventSubscriberId = null;
        string? eventTopic = null;

        _manager.SubscriptionRemoved += (subscriberId, topic) =>
        {
            eventSubscriberId = subscriberId;
            eventTopic = topic;
        };

        _manager.Unsubscribe("subscriber-1", "test/topic");

        Assert.AreEqual("subscriber-1", eventSubscriberId);
        Assert.AreEqual("test/topic", eventTopic);
    }

    #endregion

    #region Concurrency Tests

    [TestMethod]
    public async Task Subscribe_ConcurrentOperations_ShouldBeThreadSafe()
    {
        var tasks = new List<Task>();
        var pipeServers = new List<NamedPipeServerStream>();

        for (var i = 0; i < 100; i++)
        {
            var index = i;
            var pipeServer = CreateMockPipeServer();
            pipeServers.Add(pipeServer);

            tasks.Add(Task.Run(() =>
            {
                _manager.Subscribe($"subscriber-{index}", "shared/topic", pipeServer);
            }));
        }

        await Task.WhenAll(tasks);

        Assert.AreEqual(100, _manager.GetSubscriberCount("shared/topic"));

        foreach (var pipeServer in pipeServers)
        {
            pipeServer.Dispose();
        }
    }

    [TestMethod]
    public async Task SubscribeAndUnsubscribe_ConcurrentOperations_ShouldBeThreadSafe()
    {
        using var pipeServer = CreateMockPipeServer();

        // First, add some subscriptions
        for (var i = 0; i < 50; i++)
        {
            _manager.Subscribe($"subscriber-{i}", "test/topic", pipeServer);
        }

        var subscribeTasks = new List<Task>();
        var unsubscribeTasks = new List<Task>();

        // Concurrently subscribe and unsubscribe
        for (var i = 0; i < 25; i++)
        {
            var index = i;
            subscribeTasks.Add(Task.Run(() =>
            {
                _manager.Subscribe($"new-subscriber-{index}", "test/topic", pipeServer);
            }));

            unsubscribeTasks.Add(Task.Run(() =>
            {
                _manager.Unsubscribe($"subscriber-{index}", "test/topic");
            }));
        }

        await Task.WhenAll(subscribeTasks.Concat(unsubscribeTasks));

        // Should have: 50 - 25 (unsubscribed) + 25 (new) = 50 subscribers
        Assert.AreEqual(50, _manager.GetSubscriberCount("test/topic"));
    }

    #endregion

    #region Helper Methods

    private static NamedPipeServerStream CreateMockPipeServer()
    {
        var pipeName = $"TestPipe_{Guid.NewGuid():N}";
        return new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
    }

    #endregion
}
