using System.Globalization;

namespace MedicalSystemApp.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime AsUtcDate(this DateTime d)
            => DateTime.SpecifyKind(new DateTime(d.Year, d.Month, d.Day, 0, 0, 0), DateTimeKind.Utc);

        public static string ToHrDate(this DateTime d)
            => d.ToLocalTime().ToString("dd.MM.yyyy.", new CultureInfo("hr-HR"));
    }
}
