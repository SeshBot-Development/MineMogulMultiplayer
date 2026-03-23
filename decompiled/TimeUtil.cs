using System;
using System.Globalization;
using System.Text.RegularExpressions;

public static class TimeUtil
{
	public static string GetDisplaySaveTime(string rawTimestamp)
	{
		if (string.IsNullOrWhiteSpace(rawTimestamp))
		{
			return rawTimestamp ?? string.Empty;
		}
		if (!DateTimeOffset.TryParseExact(rawTimestamp, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
		{
			return rawTimestamp;
		}
		CultureInfo currentCulture = CultureInfo.CurrentCulture;
		string longDatePattern = currentCulture.DateTimeFormat.LongDatePattern;
		longDatePattern = Regex.Replace(longDatePattern, "[\\p{Ps}(\\[]?\\s*dddd\\s*[\\p{Pe})\\]]?[,，]?\\s*", "");
		longDatePattern = Regex.Replace(longDatePattern, "[\\p{Ps}(\\[]?\\s*ddd\\s*[\\p{Pe})\\]]?[,，]?\\s*", "");
		longDatePattern = Regex.Replace(longDatePattern, "\\s{2,}", " ").Trim();
		longDatePattern = Regex.Replace(longDatePattern, "\\s*,\\s*", ", ");
		longDatePattern = longDatePattern.Trim(',', ' ');
		if (string.IsNullOrEmpty(longDatePattern))
		{
			longDatePattern = currentCulture.DateTimeFormat.ShortDatePattern;
		}
		string text = longDatePattern + " " + currentCulture.DateTimeFormat.ShortTimePattern;
		return result.ToLocalTime().ToString(text, currentCulture);
	}
}
