namespace MCCS.UserControl.DynamicGrid
{
    public class SplitRequestEventArgs : EventArgs
    {
        public CutDirectionEnum Direction { get; set; }

        public required string CellId { get; set; }
    }
}
