using ElementaryCellularAutomaton;
using ElementaryCellularAutomaton.Models;
using Caliburn.Micro;

namespace CellularAutomatonGUI.ViewModels
{
    public class ShellViewModel : Screen
    {
        public RuleModel Rule { get; set; }
        public BoundaryConditionModel BoundaryCondition { get; set; }
        public int Iterations { get; set; }
    }
}
