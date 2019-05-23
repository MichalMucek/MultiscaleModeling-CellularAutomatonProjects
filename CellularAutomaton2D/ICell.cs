using System.Drawing;

namespace CellularAutomaton2D
{
    public interface ICell
    {
        int Id { get; }
        int ColumnNumber { get; }
        int RowNumber { get; }
        ICellState State { get; set; }
        ICellNeighborhood NeighboringCells { get; set; }
        Point StartPositionOnImage { get; set; }
        Point EndPositionOnImage { get; set; }
    }
}