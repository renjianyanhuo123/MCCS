namespace MCCS.Models.Stations
{
    public record StationMenuItemModel
    {
        public long Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
