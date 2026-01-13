using MCCS.Station.Abstractions.Models;

using Newtonsoft.Json;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace MCCS.UnitTest
{
    [TestClass]
    public sealed class DeserializationTest
    {
        public const string _json = """
                                    {
                                        "id": 1,
                                        "name": "测试",
                                        "signalInfos": [ 1,2]
                                    }
                                    """;

        [TestMethod]
        [DataRow(_json)]
        public void Deserialization_StationSiteControllerInfo(string json)
        {
            var obj = JsonConvert.DeserializeObject<ControllerDevice>(json);
            IsNotNull(obj);
            AreEqual(obj.Name, "测试");
            AreEqual(obj.SignalIds.Count, 2);
        }
    }
}
