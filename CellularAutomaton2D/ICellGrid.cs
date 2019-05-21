using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace CellularAutomaton2D
{
    public interface ICellGrid
    {
        List<List<ICell>> CurrentState { get; }
        List<List<ICell>> PreviousState { get; }
        ICellState ZeroState { get; }
        int ColumnCount { get; }
        int RowCount { get; }
        int CellCount { get; }
        int PopulatedCellsCount { get; }
        bool IsFullyPopulated { get; }

        void Evolve();

        BitmapImage GetBitmapImage(int cellWidth, int cellHeightm, int lineWidth);
    }
}