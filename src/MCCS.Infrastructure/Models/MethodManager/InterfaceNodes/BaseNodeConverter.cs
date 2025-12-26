using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MCCS.Infrastructure.Models.MethodManager.InterfaceNodes
{
    public class BaseNodeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BaseNode);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var nodeType = jo["NodeType"]!.Value<int>();

            BaseNode node;

            switch ((NodeTypeEnum)nodeType)
            {
                case NodeTypeEnum.Cell:
                    node = new CellNode();
                    break;

                case NodeTypeEnum.SplitterHorizontal:
                case NodeTypeEnum.SplitterVertical:
                    node = new SplitterNode();
                    break; 
                default:
                    throw new JsonSerializationException($"Unknown NodeType: {nodeType}");
            }

            serializer.Populate(jo.CreateReader(), node);
            return node;
        }

        public override void WriteJson(
            JsonWriter writer,
            object? value,
            JsonSerializer serializer) =>
            serializer.Serialize(writer, value);
    }

}
