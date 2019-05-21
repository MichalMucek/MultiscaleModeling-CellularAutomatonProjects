using System.Collections.Generic;
using CellularAutomaton2D.Models;

namespace CellularAutomaton2D
{
    public interface IEightSidedCellNeighborhood
    {
        CellNeighborhoodTypeModel Type { get; }
        ICell Top { get; }
        ICell TopRight { get; }
        ICell Right { get; }
        ICell BottomRight { get; }
        ICell Bottom { get; }
        ICell BottomLeft { get; }
        ICell Left { get; }
        ICell TopLeft { get; }
        Dictionary<ICellState, int> StatesCounts { get; }
    }
}