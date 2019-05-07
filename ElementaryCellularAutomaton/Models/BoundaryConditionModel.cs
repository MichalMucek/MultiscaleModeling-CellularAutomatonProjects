using System.ComponentModel;

namespace ElementaryCellularAutomaton.Models
{
    public enum BoundaryConditionModel
    {
        [Description("Outside is dead")]
        OutsideIsDead,

        [Description("Outside is alive")]
        OutsideIsAlive,

        [Description("Periodic")]
        Periodic
    }
}