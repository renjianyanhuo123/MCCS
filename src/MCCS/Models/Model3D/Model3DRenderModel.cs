using MCCS.Station.Core.HardwareDevices; 

namespace MCCS.Models.Model3D
{
    public record Model3DRenderModel
    {
        public long PseudoChannelId { get; init; }

        public long BillboardId { get; init; }

        public required string Model3DId { get; init; }

        public required DataPoint<float> Data { get; init; }
    }
}
