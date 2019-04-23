using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace System.Windows.Media
{
    [ValueConversion(typeof(Image), typeof(ImageSource))]
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var image = value as Image;
            var bitmapImage = new BitmapImage();
            var memoryStream = new MemoryStream();

            bitmapImage.BeginInit();

            image.Save(memoryStream, ImageFormat.Bmp);
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            bitmapImage.StreamSource = memoryStream;

            bitmapImage.EndInit();

            return bitmapImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
