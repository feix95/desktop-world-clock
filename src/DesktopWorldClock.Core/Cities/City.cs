namespace DesktopWorldClock.Core.Cities;

/// <summary>A selectable city and the IANA time zone it observes.</summary>
/// <param name="Id">Stable identifier persisted in user settings.</param>
/// <param name="TimeZoneId">IANA time zone name, for example <c>Asia/Shanghai</c>.</param>
public sealed record City(string Id, string NameZh, string NameEn, string TimeZoneId);
