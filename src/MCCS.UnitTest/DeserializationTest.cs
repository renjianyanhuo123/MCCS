using MCCS.Common.DataManagers.Devices;
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
                                        "signalInfos": [
                                            {
                                                "id" : 1,
                                                "name": "信号1"
                                            },
                                            {
                                                "id": 2,
                                                "name": "信号2"
                                            }
                                        ]
                                    }
                                    """;

        [TestMethod]
        [DataRow(_json)]
        public void Deserialization_StationSiteControllerInfo(string json)
        {
            var obj = JsonConvert.DeserializeObject<ControllerDevice>(json);
            IsNotNull(obj);
            AreEqual(obj.Name, "测试");
            AreEqual(obj.SignalInfos.Count, 2);
        }
    }
}
