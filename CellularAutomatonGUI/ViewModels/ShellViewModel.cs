using Caliburn.Micro;

namespace CellularAutomatonGUI.ViewModels
{
    public class ShellViewModel : Screen
    {
        public ElementaryCellularAutomatonViewModel ElementaryCellularAutomatonViewModel { get; private set; }
        public GameOfLifeViewModel GameOfLifeViewModel { get; private set; }
        public GrainGrowthCellularAutomatonViewModel GrainGrowthCellularAutomatonViewModel { get; private set; }

        public ShellViewModel()
        {
            ElementaryCellularAutomatonViewModel = new ElementaryCellularAutomatonViewModel();
            GameOfLifeViewModel = new GameOfLifeViewModel();
            GrainGrowthCellularAutomatonViewModel = new GrainGrowthCellularAutomatonViewModel();
        }
    }
}