namespace MCCS.UserControl.DynamicGrid
{
    public record LayoutSettingModel
    {
        public required List<CellViewModel> Cells { get; init; } 

        public required List<GridSizeDefinitionModel> Rows { get; init; }

        public required List<GridSizeDefinitionModel> Columns { get; init; }
    }
}
