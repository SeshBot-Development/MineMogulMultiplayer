using System;
using System.Globalization;
using System.Text.RegularExpressions;

// Token: 0x020000E8 RID: 232
public static class TimeUtil
{
	// Token: 0x06000634 RID: 1588 RVA: 0x000205C4 File Offset: 0x0001E7C4
	public static string GetDisplaySaveTime(string rawTimestamp)
	{
		if (string.IsNullOrWhiteSpace(rawTimestamp))
		{
			return rawTimestamp ?? string.Empty;
		}
		DateTimeOffset dateTimeOffset;
		if (!DateTimeOffset.TryParseExact(rawTimestamp, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTimeOffset))
		{
			return rawTimestamp;
		}
		CultureInfo currentCulture = CultureInfo.CurrentCulture;
		string text = currentCulture.DateTimeFormat.LongDatePattern;
		text = Regex.Replace(text, "[\\p{Ps}(\\[]?\\s*dddd\\s*[\\p{Pe})\\]]?[,，]?\\s*", "");
		text = Regex.Replace(text, "[\\p{Ps}(\\[]?\\s*ddd\\s*[\\p{Pe})\\]]?[,，]?\\s*", "");
		text = Regex.Replace(text, "\\s{2,}", " ").Trim();
		text = Regex.Replace(text, "\\s*,\\s*", ", ");
		text = text.Trim(new char[] { ',', ' ' });
		if (string.IsNullOrEmpty(text))
		{
			text = currentCulture.DateTimeFormat.ShortDatePattern;
		}
		string text2 = text + " " + currentCulture.DateTimeFormat.ShortTimePattern;
		return dateTimeOffset.ToLocalTime().ToString(text2, currentCulture);
	}
}
