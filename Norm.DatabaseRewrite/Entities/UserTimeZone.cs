namespace Norm.DatabaseRewrite.Entities;

public record UserTimeZone(ulong UserId, string TimeZoneId)
{
    public int Id { get; set; }
}