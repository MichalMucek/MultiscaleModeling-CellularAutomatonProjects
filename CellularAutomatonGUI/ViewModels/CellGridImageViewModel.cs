using Caliburn.Micro;
using System.Windows.Media.Imaging;

namespace CellularAutomatonGUI.ViewModels
{
    public class CellGridImageViewModel : Screen
    {
        private BitmapImage bitmapImage;

        public BitmapImage BitmapImage
        {
            get => bitmapImage;
            set
            {
                bitmapImage = value;
                NotifyOfPropertyChange(() => BitmapImage);
            }
        }
    }
}