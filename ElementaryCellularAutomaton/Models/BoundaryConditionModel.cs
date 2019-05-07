using System.ComponentModel;

namespace ElementaryCellularAutomaton.Models
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