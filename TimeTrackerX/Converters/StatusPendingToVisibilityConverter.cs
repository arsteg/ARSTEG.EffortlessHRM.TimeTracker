using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TimeTrackerX.Converters
{
    // Converts "pending" status to IsVisible=true, else IsVisible=false
    public class StatusPendingToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() == "pending";
            }
            return false; // Default to hidden if value is not a string
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            throw new NotImplementedException("ConvertBack is not supported.");
        }
    }

    // Converts "approved" status to IsVisible=true, else IsVisible=false
}
