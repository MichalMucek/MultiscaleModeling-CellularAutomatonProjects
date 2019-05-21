using System.Collections.Generic;

namespace CellularAutomaton2D
{
    internal interface ICellNeighborhood
    {
        CellNeighborhoodType Type { get; }
        Dictionary<ICellState, uint> StatesCounts { get; }
    }
}