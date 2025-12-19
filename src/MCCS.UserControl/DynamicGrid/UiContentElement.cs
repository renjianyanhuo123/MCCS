using System.Text.Json.Serialization;
using System.Windows;

namespace MCCS.UserControl.DynamicGrid
{
    public sealed class UiContentElement(string id)
    {
        public string Id { get; private set; } = id;
        [JsonIgnore]
        public required FrameworkElement Content { get; set; }
        public CellTypeEnum CellType { get; set; } = CellTypeEnum.EditableElement;
    }
}
