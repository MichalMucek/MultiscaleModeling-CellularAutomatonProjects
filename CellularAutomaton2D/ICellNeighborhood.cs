using System.Collections.Generic;

namespace CellularAutomaton2D
{
    public interface ICellNeighborhood
    {
        CellNeighborhoodType Type { get; }
        Dictionary<ICellState, uint> StatesCounts { get; }
    }
}