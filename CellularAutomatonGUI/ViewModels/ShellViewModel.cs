using Caliburn.Micro;
using System.Windows.Media.Imaging;

namespace CellularAutomatonGUI.ViewModels
{
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