using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EduPath.Avalonia.Converters
{
    public class SidebarWidthConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                // Nếu chiều rộng của thanh bên > 130px thì hiển thị chữ (true)
                // Nếu kéo nhỏ hơn 130px thì ẩn chữ đi (false)
                return width > 130;
            }
            return true;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}