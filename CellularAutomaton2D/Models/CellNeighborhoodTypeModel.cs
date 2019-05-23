using System.ComponentModel;

namespace CellularAutomaton2D.Models
{
    public enum CellNeighborhoodTypeModel
    {
        [Description("von Neumann")]
        VonNeumann = 5,

        [Description("Moore")]
        Moore = 9,

        [Description("Random pentagonal")]
        RandomPentagonal,

        [Description("Left hexagonal")]
        LeftHexagonal,

        [Description("Right hexagonal")]
        RightHexagonal,

        [Description("Random hexagonal")]
        RandomHexagonal,

        [Description("Cells within radius")]
        CellsWithinRadius,

        [Description("Cells COM within radius")]
        CellCenterOfMassWithinRadius,
    }
}