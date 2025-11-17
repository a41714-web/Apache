using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Apache.Converters
{
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            !string.IsNullOrWhiteSpace(value?.ToString());

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class StockColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stock)
                return stock <= 5 ? Colors.Red : Colors.Green;
            return Colors.Black;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class SelectedTabConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string currentTab = value?.ToString() ?? "";
            string tabName = parameter?.ToString() ?? "";
            return currentTab == tabName ? Color.FromArgb("#1976D2") : Color.FromArgb("#757575");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class TabVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string currentTab = value?.ToString() ?? "";
            string tabName = parameter?.ToString() ?? "";
            return currentTab == tabName;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            if (bool.TryParse(value?.ToString(), out var parsed))
                return !parsed;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            if (bool.TryParse(value?.ToString(), out var parsed))
                return !parsed;
            return false;
        }
    }

    public class MultiBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Collections.IEnumerable enumerable)
            {
                bool any = false;
                foreach (var item in enumerable)
                {
                    if (item is bool b)
                    {
                        any = true;
                        if (!b) return false;
                    }
                }
                return any ? true : false;
            }

            if (value is bool single)
                return single;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    // Converts the boolean IsCustomerMode to a display string for the login page
    public class BoolToRoleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCustomer)
            {
                return isCustomer ? "Mode: Customer" : "Mode: Admin";
            }
            return "Mode: Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
