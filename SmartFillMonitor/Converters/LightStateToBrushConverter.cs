using SmartFillMonitor.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace SmartFillMonitor.Converters
{
    public class LightStateToBrushConverter : IValueConverter
    {
        private static readonly Color offColor = Colors.DimGray;
        private static readonly Color GreenColor = Colors.Green;
        private static readonly Color YellowColor = Colors.Yellow;
        private static readonly Color RedColor = Colors.Red;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = value is LightState ls ? ls : LightState.Off;
            var role = (parameter as string)?.ToLowerInvariant() ?? string.Empty;
            if (state == LightState.Off) return offColor;
            return role switch
            {
                "green" => state == LightState.Green ? GreenColor : offColor,
                "yellow" => state == LightState.Yellow ? YellowColor : offColor,
                "red" => state == LightState.Red ? RedColor : offColor,
                _ => offColor
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
