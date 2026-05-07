using System.Globalization;

namespace SalesforceManager.Services.Salesforce.Utilities
{
    internal static class SalesforceDateTimeFormatter
    {
        public static string FormatForDisplay(string? rawDateTime)
        {
            if (string.IsNullOrWhiteSpace(rawDateTime))
            {
                return string.Empty;
            }

            if (!DateTimeOffset.TryParse(
                rawDateTime,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
            {
                return rawDateTime;
            }

            return parsed.ToString("d MMM yyyy, h:mm tt", CultureInfo.InvariantCulture).Replace("AM", "am").Replace("PM", "pm");
        }
    }
}
