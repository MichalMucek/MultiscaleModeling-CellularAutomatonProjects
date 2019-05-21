﻿using System.Drawing;

namespace CellularAutomaton2D
{
    internal interface ICell
    {
        int Id { get; }
        int ColumnNumber { get; }
        int RowNumber { get; }
        ICellState State { get; }
        ICellNeighborhood NeighboringCells { get; }
        Point StartPositionOnImage { get; }
        Point EndPositionOnImage { get; }
    }
}