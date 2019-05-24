using CellularAutomaton2D.Models;
using System.Collections.Generic;

namespace CellularAutomaton2D
{
    public interface ICellNeighborhood
    {
        CellNeighborhoodTypeModel Type { get; }
        Dictionary<ICellState, int> StatesCounts { get; }
    }
}