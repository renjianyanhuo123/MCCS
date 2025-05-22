using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MCCS.Models
{
    public class ModelData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
        public Color OriginalColor { get; set; }
        public Point3D Position { get; set; }
        public Model3D Model { get; set; }
        public string FileName => System.IO.Path.GetFileName(FilePath);

        // 用于显示的附加属性
        public string Value { get; set; }
        public string DisplayText => $"{Name}\n文件: {FileName}";
    }
}
