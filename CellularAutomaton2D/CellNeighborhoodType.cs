using System.ComponentModel;

namespace CellularAutomaton2D
{
    internal enum CellNeighborhoodType
    {
        [Description("von Neumann")]
        VonNeumann = 5,

        [Description("Random pentagonal")]
        RandomPentagonal = 6,

        [Description("Left hexagonal")]
        LeftHexagonal = 7,

        [Description("Right hexagonal")]
        Rightexagonal = 7,

        [Description("Random hexagonal")]
        RandomHexagonal = 7,

        [Description("Moore")]
        Moore = 9,

        [Description("Cells within radius")]
        CellsWithinRadius,

        [Description("Cells COM within radius")]
        CellCenterOfMassWithinRadius,
    }
}