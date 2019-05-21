using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace CellularAutomaton2D
{
    internal interface ICellGrid
    {
        List<List<ICell>> CurrentState { get; }
        List<List<ICell>> PreviousState { get; }
        ICellState ZeroState { get; }
        int ColumnCount { get; }
        int RowCount { get; }
        int CellCount { get; }
        int PopulatedCellsCount { get; }
        bool IsFullyPopulated { get; }

        void CreateNewGrainCellsForCurrentState();

        void CreateRowsInPreviousState();

        void AddNeighboringCellsToCellsState(List<List<ICell>> cellsState);

        void Evolve();

        void CopyCurrentStateToPrevious();

        BitmapImage GetBitmapImage(int cellWidth, int cellHeightm, int lineWidth);
    }
}