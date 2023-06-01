using System;

public static class FileSizeUtil
{
    private static readonly string[] SizeSuffixes = { "байт", "КБ", "МБ", "ГБ", "ТБ", "ПБ", "ЕБ", "ЗБ", "ЙБ" };
    private static readonly string[] minutesStrings = { "минута", "минуты", "минут" };
    private static readonly string[] hoursStrings = { "час", "часов", "часа" };

    public static string BytesToString(long bytesCount)
    {
        return SizeSuffix(bytesCount, 1);
    }
    public static string PrettyModifyDate(DateTime date)
    {
        DateTime now = DateTime.Now;
        TimeSpan delta = now - date;

        if (delta.TotalDays > 7)
        {
            return date.ToShortDateString();
        }
        if (delta.TotalDays >= 1)
        {
            var days = Math.Floor(delta.TotalDays);
            return days + " " + GetDays(days) + " назад";
        }
        if (delta.TotalHours >= 1)
        {
            var hours = Math.Floor(delta.TotalHours);
            return hours + " " + GetHours(hours) + " назад";
        }
        if (delta.TotalMinutes >= 1)
        {
            var minutes = Math.Floor(delta.TotalMinutes);
            return minutes + " " + GetMinutes(minutes) + " назад";
        }
        else
        {
            var seconds = Math.Floor(delta.TotalSeconds);
            return seconds + " " + GetSeconds(seconds) + " назад";
        }
    }

    private static string GetSeconds(double seconds)
    {
        if (seconds == 1) return "секунду";
        if (seconds >= 2 && seconds <= 4) return "секунды";
        if (seconds >= 5 && seconds <= 20) return "секунд";

        return seconds % 10 == 1 ? "секунду" : "секунды";
    }
    private static string GetMinutes(double minutes)
    {
        if (minutes == 1) return "минуту";
        if (minutes >= 2 && minutes <= 4) return "минуты";
        if (minutes >= 5 && minutes <= 20) return "минут";

        double last = minutes % 10;
        if (last == 0 || last >= 5) return "минут";
        if (last == 1) return "минуту";
        return "минуты";
    }
    private static string GetHours(double hours)
    {
        if (hours == 1) return "час";
        if (hours >= 2 && hours <= 4) return "часа";
        if (hours >= 5 && hours <= 20) return "часов";

        return hours % 10 == 1 ? "час" : "часов";
    }
    private static string GetDays(double days)
    {
        if (days == 1) return "день";
        if (days >= 2 && days <= 4) return "дня";
        if (days >= 5 && days <= 20) return "дней";

        return days % 10 == 1 ? "день" : "дня";
    }

    private static string SizeSuffix(long value, int decimalPlaces = 1)
    {
        if (decimalPlaces < 0) throw new ArgumentOutOfRangeException(nameof(decimalPlaces));
        if (value < 0) return "-" + SizeSuffix(-value, decimalPlaces);
        if (value == 0) return "0 " + SizeSuffixes[0];

        // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
        int mag = (int)Math.Log(value, 1024);

        // 1L << (mag * 10) == 2 ^ (10 * mag) 
        // [i.e. the number of bytes in the unit corresponding to mag]
        decimal adjustedSize = (decimal)value / (1L << (mag * 10));

        // make adjustment when the value is large enough that
        // it would round up to 1000 or more
        if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
        {
            mag += 1;
            adjustedSize /= 1024;
        }

        return (Math.Round(adjustedSize * 10) / 10) + " " + SizeSuffixes[mag];
    }
}