using HelixToolkit.SharpDX.Core.Assimp;
using SharpDX;

namespace MCCS.Services.Model3DService.EventParameters
{
    public class ImportResult
    {
        public bool Success { get; set; }
        public HelixToolkitScene Scene { get; set; }
        public string FilePath { get; set; }
        public int Index { get; set; }
        public BoundingBox Bound { get; set; }
        public Vector3 Centroid { get; set; }
        public Exception Error { get; set; }
    }
}
