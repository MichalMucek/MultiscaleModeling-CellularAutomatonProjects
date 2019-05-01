using System.ComponentModel;

namespace GameOfLife.Models
{
    public enum CellsNeighborhoodTypeModel
    {
        [Description("von Neumann")]
        VonNeumann = 5,

        [Description("Moore")]
        Moore = 9
    }
}