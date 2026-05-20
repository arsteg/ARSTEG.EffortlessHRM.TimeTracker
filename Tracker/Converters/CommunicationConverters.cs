using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TimeTracker.Converters
{
    /// <summary>
    /// Converts a string to its first letter (uppercase)
    /// </summary>
    public class FirstLetterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                return str[0].ToString().ToUpper();
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a positive number to Visibility.Visible, otherwise Collapsed
    /// Use ConverterParameter=Inverse to invert the logic
    /// </summary>
    public class PositiveToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isPositive = false;

            if (value is int intValue)
                isPositive = intValue > 0;
            else if (value is double doubleValue)
                isPositive = doubleValue > 0;
            else if (value is long longValue)
                isPositive = longValue > 0;

            // Check for Inverse parameter
            bool inverse = parameter is string paramStr &&
                          paramStr.Equals("Inverse", StringComparison.OrdinalIgnoreCase);

            if (inverse)
                return isPositive ? Visibility.Collapsed : Visibility.Visible;

            return isPositive ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts null to Visibility.Collapsed, non-null to Visible
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a boolean to the inverse Visibility
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts IsOwn boolean to message background color
    /// </summary>
    public class OwnMessageBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOwn && isOwn)
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8E8E8"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts IsOwn boolean to message foreground color
    /// </summary>
    public class OwnMessageForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOwn && isOwn)
            {
                return Brushes.White;
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts IsOwn boolean to message horizontal alignment
    /// </summary>
    public class OwnMessageAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOwn && isOwn)
            {
                return HorizontalAlignment.Right;
            }
            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts presence status to color
    /// </summary>
    public class PresenceStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as string;
            var color = status switch
            {
                "online" => "#4CAF50",
                "away" => "#FF9800",
                "busy" or "dnd" => "#F44336",
                _ => "#9E9E9E"
            };
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a boolean to Visibility
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts relative time from DateTime (handles both UTC and local times)
    /// </summary>
    public class RelativeTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dateTime;

            if (value is DateTime dt)
            {
                // Convert to local time if it's UTC
                dateTime = dt.Kind == DateTimeKind.Utc ? dt.ToLocalTime() : dt;
            }
            else if (value is DateTimeOffset dto)
            {
                dateTime = dto.LocalDateTime;
            }
            else if (value is string dateStr && DateTime.TryParse(dateStr, out var parsed))
            {
                dateTime = parsed.Kind == DateTimeKind.Utc ? parsed.ToLocalTime() : parsed;
            }
            else
            {
                return "";
            }

            var now = DateTime.Now;
            var diff = now - dateTime;

            if (diff.TotalSeconds < 0)
                return "Just now"; // Future time, treat as now

            if (diff.TotalSeconds < 60)
                return "Just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays}d ago";

            return dateTime.ToString("MMM d");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts null or empty string to Visibility.Collapsed
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
