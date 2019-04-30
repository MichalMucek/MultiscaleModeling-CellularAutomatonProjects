using Caliburn.Micro;
using System.ComponentModel.Composition;

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
    }
}
