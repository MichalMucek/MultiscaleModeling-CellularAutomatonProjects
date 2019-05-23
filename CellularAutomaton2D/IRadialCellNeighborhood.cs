using CellularAutomaton2D.Models;
using System.Collections.Generic;

namespace CellularAutomaton2D
{
    public interface IRadialCellNeighborhood
    {
        CellNeighborhoodTypeModel Type { get; }
        List<ICell> Cells { get; }
        Dictionary<ICell, int> StatesCount { get; }
    }
}