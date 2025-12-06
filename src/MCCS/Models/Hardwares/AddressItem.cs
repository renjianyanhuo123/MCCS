namespace MCCS.Models.Hardwares
{
    public record AddressItem
    {
        public long Value { get; init; }
        public required string Display { get; init; }
    }
}
