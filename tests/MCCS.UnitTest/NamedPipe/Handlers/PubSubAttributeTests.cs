using MCCS.Infrastructure.Communication.NamedPipe;

namespace MCCS.UnitTest.NamedPipe.Handlers;

[TestClass]
public class PubSubAttributeTests
{
    #region PubSubPipeAttribute Tests

    [TestMethod]
    public void PubSubPipeAttribute_ShouldSetName()
    {
        var attr = new PubSubPipeAttribute("TestPubSubPipe");

        Assert.AreEqual("TestPubSubPipe", attr.Name);
    }

    [TestMethod]
    public void PubSubPipeAttribute_ShouldBeApplicableToClass()
    {
        var attrUsage = typeof(PubSubPipeAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .FirstOrDefault() as AttributeUsageAttribute;

        Assert.IsNotNull(attrUsage);
        Assert.AreEqual(AttributeTargets.Class, attrUsage.ValidOn);
        Assert.IsFalse(attrUsage.AllowMultiple);
        Assert.IsFalse(attrUsage.Inherited);
    }

    [TestMethod]
    public void PubSubPipeAttribute_OnClass_ShouldBeRetrievable()
    {
        var type = typeof(TestPubSubHandler);
        var attr = type.GetCustomAttributes(typeof(PubSubPipeAttribute), false)
            .FirstOrDefault() as PubSubPipeAttribute;

        Assert.IsNotNull(attr);
        Assert.AreEqual("Test_PubSub_Pipe", attr.Name);
    }

    #endregion

    #region TopicAttribute Tests

    [TestMethod]
    public void TopicAttribute_ShouldSetTopic()
    {
        var attr = new TopicAttribute("sensor/temperature");

        Assert.AreEqual("sensor/temperature", attr.Topic);
    }

    [TestMethod]
    public void TopicAttribute_ShouldBeApplicableToMethod()
    {
        var attrUsage = typeof(TopicAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .FirstOrDefault() as AttributeUsageAttribute;

        Assert.IsNotNull(attrUsage);
        Assert.AreEqual(AttributeTargets.Method, attrUsage.ValidOn);
        Assert.IsTrue(attrUsage.AllowMultiple); // Multiple topics can be applied
        Assert.IsFalse(attrUsage.Inherited);
    }

    [TestMethod]
    public void TopicAttribute_OnMethod_ShouldBeRetrievable()
    {
        var method = typeof(TestPubSubHandler).GetMethod(nameof(TestPubSubHandler.HandleTemperature));
        Assert.IsNotNull(method);

        var attrs = method.GetCustomAttributes(typeof(TopicAttribute), false)
            .Cast<TopicAttribute>()
            .ToArray();

        Assert.AreEqual(1, attrs.Length);
        Assert.AreEqual("sensor/temperature", attrs[0].Topic);
    }

    [TestMethod]
    public void TopicAttribute_MultipleOnMethod_ShouldAllBeRetrievable()
    {
        var method = typeof(TestPubSubHandler).GetMethod(nameof(TestPubSubHandler.HandleMultipleTopics));
        Assert.IsNotNull(method);

        var attrs = method.GetCustomAttributes(typeof(TopicAttribute), false)
            .Cast<TopicAttribute>()
            .ToArray();

        Assert.AreEqual(3, attrs.Length);
        Assert.IsTrue(attrs.Any(a => a.Topic == "topic1"));
        Assert.IsTrue(attrs.Any(a => a.Topic == "topic2"));
        Assert.IsTrue(attrs.Any(a => a.Topic == "topic3"));
    }

    #endregion

    #region Combined Attribute Usage Tests

    [TestMethod]
    public void PubSubPipeAndTopicAttributes_ShouldWorkTogether()
    {
        var type = typeof(TestPubSubHandler);

        // Get class attribute
        var classAttr = type.GetCustomAttributes(typeof(PubSubPipeAttribute), false)
            .FirstOrDefault() as PubSubPipeAttribute;
        Assert.IsNotNull(classAttr);
        Assert.AreEqual("Test_PubSub_Pipe", classAttr.Name);

        // Get all methods with Topic attribute
        var topicMethods = type.GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(TopicAttribute), false).Any())
            .ToArray();

        Assert.IsTrue(topicMethods.Length >= 2);
    }

    [TestMethod]
    public void TopicAttribute_WithHierarchicalTopic_ShouldWorkCorrectly()
    {
        var method = typeof(TestPubSubHandler).GetMethod(nameof(TestPubSubHandler.HandleHierarchicalTopic));
        Assert.IsNotNull(method);

        var attr = method.GetCustomAttributes(typeof(TopicAttribute), false)
            .FirstOrDefault() as TopicAttribute;

        Assert.IsNotNull(attr);
        Assert.AreEqual("sensors/building1/floor2/room301/temperature", attr.Topic);
    }

    #endregion

    #region Test Handler Class

    [PubSubPipe("Test_PubSub_Pipe")]
    private class TestPubSubHandler
    {
        [Topic("sensor/temperature")]
        public void HandleTemperature()
        {
        }

        [Topic("topic1")]
        [Topic("topic2")]
        [Topic("topic3")]
        public void HandleMultipleTopics()
        {
        }

        [Topic("sensors/building1/floor2/room301/temperature")]
        public void HandleHierarchicalTopic()
        {
        }
    }

    #endregion
}
