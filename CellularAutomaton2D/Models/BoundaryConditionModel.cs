using System.ComponentModel;

namespace CellularAutomaton2D.Models
{
    public enum BoundaryConditionModel
    {
        [Description("Absorbing")]
        Absorbing,

        [Description("Counter-Absorbing")]
        CounterAbsorbing,

        [Description("Periodic")]
        Periodic
    }
}