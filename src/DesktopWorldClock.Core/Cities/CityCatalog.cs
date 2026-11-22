namespace DesktopWorldClock.Core.Cities;

/// <summary>Built-in list of selectable cities. Bundled with the app so no network access is needed.</summary>
public static class CityCatalog
{
    public const string DefaultCityId = "shanghai";

    public static IReadOnlyList<City> All { get; } =
    [
        new("shanghai", "上海", "Shanghai", "Asia/Shanghai"),
        new("beijing", "北京", "Beijing", "Asia/Shanghai"),
        new("hong-kong", "香港", "Hong Kong", "Asia/Hong_Kong"),
        new("taipei", "台北", "Taipei", "Asia/Taipei"),
        new("tokyo", "东京", "Tokyo", "Asia/Tokyo"),
        new("seoul", "首尔", "Seoul", "Asia/Seoul"),
        new("singapore", "新加坡", "Singapore", "Asia/Singapore"),
        new("kuala-lumpur", "吉隆坡", "Kuala Lumpur", "Asia/Kuala_Lumpur"),
        new("manila", "马尼拉", "Manila", "Asia/Manila"),
        new("jakarta", "雅加达", "Jakarta", "Asia/Jakarta"),
        new("bangkok", "曼谷", "Bangkok", "Asia/Bangkok"),
        new("kathmandu", "加德满都", "Kathmandu", "Asia/Kathmandu"),
        new("new-delhi", "新德里", "New Delhi", "Asia/Kolkata"),
        new("dubai", "迪拜", "Dubai", "Asia/Dubai"),
        new("moscow", "莫斯科", "Moscow", "Europe/Moscow"),
        new("istanbul", "伊斯坦布尔", "Istanbul", "Europe/Istanbul"),
        new("cairo", "开罗", "Cairo", "Africa/Cairo"),
        new("johannesburg", "约翰内斯堡", "Johannesburg", "Africa/Johannesburg"),
        new("berlin", "柏林", "Berlin", "Europe/Berlin"),
        new("paris", "巴黎", "Paris", "Europe/Paris"),
        new("amsterdam", "阿姆斯特丹", "Amsterdam", "Europe/Amsterdam"),
        new("rome", "罗马", "Rome", "Europe/Rome"),
        new("madrid", "马德里", "Madrid", "Europe/Madrid"),
        new("london", "伦敦", "London", "Europe/London"),
        new("reykjavik", "雷克雅未克", "Reykjavik", "Atlantic/Reykjavik"),
        new("sao-paulo", "圣保罗", "São Paulo", "America/Sao_Paulo"),
        new("buenos-aires", "布宜诺斯艾利斯", "Buenos Aires", "America/Argentina/Buenos_Aires"),
        new("new-york", "纽约", "New York", "America/New_York"),
        new("toronto", "多伦多", "Toronto", "America/Toronto"),
        new("chicago", "芝加哥", "Chicago", "America/Chicago"),
        new("mexico-city", "墨西哥城", "Mexico City", "America/Mexico_City"),
        new("denver", "丹佛", "Denver", "America/Denver"),
        new("los-angeles", "洛杉矶", "Los Angeles", "America/Los_Angeles"),
        new("vancouver", "温哥华", "Vancouver", "America/Vancouver"),
        new("honolulu", "檀香山", "Honolulu", "Pacific/Honolulu"),
        new("sydney", "悉尼", "Sydney", "Australia/Sydney"),
        new("melbourne", "墨尔本", "Melbourne", "Australia/Melbourne"),
        new("auckland", "奥克兰", "Auckland", "Pacific/Auckland"),
    ];

    public static City Default { get; } = All.First(city => city.Id == DefaultCityId);

    /// <returns>The matching city, or <c>null</c> when no city has this id.</returns>
    public static City? FindById(string id) =>
        All.FirstOrDefault(city => string.Equals(city.Id, id, StringComparison.Ordinal));

    /// <summary>Matches Chinese name, English name or id; an empty query returns every city.</summary>
    public static IReadOnlyList<City> Search(string query)
    {
        var trimmed = query.Trim();
        if (trimmed.Length == 0)
        {
            return All;
        }

        return [.. All.Where(city => Matches(city, trimmed))];
    }

    private static bool Matches(City city, string query) =>
        city.NameZh.Contains(query, StringComparison.OrdinalIgnoreCase)
        || city.NameEn.Contains(query, StringComparison.OrdinalIgnoreCase)
        || city.Id.Contains(query, StringComparison.OrdinalIgnoreCase);
}
