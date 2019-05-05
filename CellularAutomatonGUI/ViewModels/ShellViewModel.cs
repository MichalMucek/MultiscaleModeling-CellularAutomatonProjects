using Caliburn.Micro;
using System.ComponentModel.Composition;
using System.Windows.Media.Imaging;

namespace CellularAutomatonGUI.ViewModels
{
    [Export(typeof(ShellViewModel))]
    public class ShellViewModel : Screen
    {
        public ElementaryCellularAutomatonViewModel ElementaryCellularAutomatonViewModel { get; private set; }
        public GameOfLifeViewModel GameOfLifeViewModel { get; private set; }
        public CellGridImageViewModel CellGridImageViewModel { get; private set; }

        public ShellViewModel()
        {
            CellGridImageViewModel = new CellGridImageViewModel();
            ElementaryCellularAutomatonViewModel = new ElementaryCellularAutomatonViewModel(this);
            GameOfLifeViewModel = new GameOfLifeViewModel(this);
        }

        public string CellGridImageFilename
        {
            get => CellGridImageViewModel.CellGridImageFilename;
            set
            {
                CellGridImageViewModel.CellGridImageFilename = value;
                NotifyOfPropertyChange(() => CellGridImageFilename);
            }
        }

        public BitmapImage CellGridBitmapImage
        {
            get => CellGridImageViewModel.BitmapImage;
            set
            {
                CellGridImageViewModel.BitmapImage = value;
                NotifyOfPropertyChange(() => CellGridBitmapImage);
            }
        }
    }
}