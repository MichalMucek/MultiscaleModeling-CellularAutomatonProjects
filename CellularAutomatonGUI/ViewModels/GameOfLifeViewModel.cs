using Caliburn.Micro;
using ElementaryCellularAutomaton.Models;

namespace CellularAutomatonGUI.ViewModels
{
    public class GameOfLifeViewModel : Screen
    {
        private BindableCollection<BoundaryConditionModel> _boundaryConditions = new BindableCollection<BoundaryConditionModel>();
        private BoundaryConditionModel _selectedBoundaryCondition = BoundaryConditionModel.OutsideIsDead;
        private ShellViewModel _shellViewModel;

        public GameOfLifeViewModel(ShellViewModel shellViewModel)
        {
            _shellViewModel = shellViewModel;

            _boundaryConditions.Add(BoundaryConditionModel.OutsideIsDead);
            _boundaryConditions.Add(BoundaryConditionModel.OutsideIsAlive);
            _boundaryConditions.Add(BoundaryConditionModel.Periodical);
        }

        public BindableCollection<BoundaryConditionModel> BoundaryConditions => _boundaryConditions;

        public BoundaryConditionModel SelectedBoundaryCondition
        {
            get => _selectedBoundaryCondition;
            set => _selectedBoundaryCondition = value;
        }
    }
}
