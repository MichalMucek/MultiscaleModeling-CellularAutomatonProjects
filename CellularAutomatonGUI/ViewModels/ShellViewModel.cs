using Caliburn.Micro;

namespace CellularAutomatonGUI.ViewModels
{
    public class ShellViewModel : Screen
    {
        public ElementaryCellularAutomatonViewModel ElementaryCellularAutomatonViewModel { get; private set; }
        public LifeLikeCellularAutomatonViewModel GameOfLifeViewModel { get; private set; }
        public GrainGrowthCellularAutomatonViewModel GrainGrowthCellularAutomatonViewModel { get; private set; }

        public ShellViewModel()
        {
            ElementaryCellularAutomatonViewModel = new ElementaryCellularAutomatonViewModel();
            GameOfLifeViewModel = new LifeLikeCellularAutomatonViewModel();
            GrainGrowthCellularAutomatonViewModel = new GrainGrowthCellularAutomatonViewModel();
        }
    }
}