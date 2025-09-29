namespace MCCS.Models.MethodManager
{
    public record MethodMenuItemModel
    {
        public long Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Icon { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
    }
}
