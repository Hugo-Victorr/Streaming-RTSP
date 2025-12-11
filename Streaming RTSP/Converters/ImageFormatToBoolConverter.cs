using Streaming_RTSP.Core.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Streaming_RTSP.Converters
{
    public class ImageFormatToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ImageFormat imageFormat && parameter is string paramStr)
            {
                if (int.TryParse(paramStr, out int formatValue))
                {
                    return (int)imageFormat == formatValue;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string paramStr)
            {
                if (int.TryParse(paramStr, out int formatValue))
                {
                    return (ImageFormat)formatValue;
                }
            }
            return ImageFormat.PNG;
        }
    }
}
