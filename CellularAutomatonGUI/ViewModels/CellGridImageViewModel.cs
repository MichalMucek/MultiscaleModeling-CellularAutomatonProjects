using Caliburn.Micro;
using System.ComponentModel.Composition;
using System.Windows.Media.Imaging;

namespace CellularAutomatonGUI.ViewModels
{
    [Export(typeof(CellGridImageViewModel))]
    public class CellGridImageViewModel : Screen
    {
        private BitmapImage bitmapImage;
        private static string cellGridImageFilename;

        public string CellGridImageFilename
        {
            get => cellGridImageFilename;
            set
            {
                cellGridImageFilename = value;
                NotifyOfPropertyChange(() => CellGridImageFilename);
            }
        }

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