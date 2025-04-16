using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;

namespace TimeTrackerX.Converters
{
    // Boolean inverter (e.g., for toggling visibility or enabled states)
    public class ValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value;
        }
    }

    // Converts text input and focus state to visibility boolean (e.g., for placeholder visibility)
    public class TextInputToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (values.Length >= 2 && values[0] is bool hasText && values[1] is bool hasFocus)
            {
                // Return false (hide) if focused or has text, true (show) otherwise
                return !(hasFocus || hasText);
            }
            return true; // Default to visible if inputs are invalid
        }

        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture
        )
        {
            throw new NotSupportedException("ConvertBack is not implemented.");
        }

        object? IMultiValueConverter.Convert(
            IList<object?> values,
            Type targetType,
            object? parameter,
            CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }

    // Monitors password input length for a TextBox (used for password fields)
    public class PasswordBoxMonitor : AvaloniaObject
    {
        public static readonly AttachedProperty<bool> IsMonitoringProperty =
            AvaloniaProperty.RegisterAttached<PasswordBoxMonitor, TextBox, bool>(
                "IsMonitoring",
                defaultValue: false,
                inherits: false,
                defaultBindingMode: BindingMode.TwoWay
            );

        public static readonly AttachedProperty<int> PasswordLengthProperty =
            AvaloniaProperty.RegisterAttached<PasswordBoxMonitor, TextBox, int>(
                "PasswordLength",
                defaultValue: 0,
                inherits: false,
                defaultBindingMode: BindingMode.TwoWay
            );

        public static bool GetIsMonitoring(TextBox textBox)
        {
            return textBox.GetValue(IsMonitoringProperty);
        }

        public static void SetIsMonitoring(TextBox textBox, bool value)
        {
            textBox.SetValue(IsMonitoringProperty, value);
        }

        public static int GetPasswordLength(TextBox textBox)
        {
            return textBox.GetValue(PasswordLengthProperty);
        }

        public static void SetPasswordLength(TextBox textBox, int value)
        {
            textBox.SetValue(PasswordLengthProperty, value);
        }

        private static void OnIsMonitoringChanged(
            AvaloniaObject d,
            AvaloniaPropertyChangedEventArgs e
        )
        {
            if (d is TextBox textBox)
            {
                if (e.NewValue is bool isMonitoring && isMonitoring)
                {
                    textBox.TextChanged += TextBox_TextChanged;
                }
                else
                {
                    textBox.TextChanged -= TextBox_TextChanged;
                }
            }
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                SetPasswordLength(textBox, textBox.Text?.Length ?? 0);
            }
        }
    }
}
