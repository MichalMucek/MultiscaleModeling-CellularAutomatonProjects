using GrainGrowthCellularAutomaton.Models;
using CellularAutomaton2D;
using System.Drawing;

namespace GrainGrowthCellularAutomaton
{
    internal class GrainCellModel : ICell
    {
        public int Id { get; private set; }
        public int ColumnNumber { get; private set; }
        public int RowNumber { get; private set; }
        public ICellState State { get; set; }
        public ICellNeighborhood NeighboringCells { get; set; }
        public Point StartPositionOnImage { get; set; }
        public Point EndPositionOnImage { get; set; }

        public GrainCellModel()
        {
            Id = -1;
            ColumnNumber = -1;
            RowNumber = -1;
            State = null;
        }

        public GrainCellModel(GrainModel grain)
            : this()
            => State = grain;

        public GrainCellModel(int id, int columnNumber, int rowNumber, GrainModel grain)
        {
            Id = id;
            ColumnNumber = columnNumber;
            RowNumber = rowNumber;
            State = grain;
        }

        public GrainCellModel(GrainCellModel obj)
        {
            Id = obj.Id;
            ColumnNumber = obj.ColumnNumber;
            RowNumber = obj.RowNumber;
            State = obj.State;
        }
    }
}