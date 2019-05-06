using System.Drawing;

namespace GameOfLife.Models
{
    internal class CellModel : ElementaryCellularAutomaton.Models.CellModel
    {
        public int ColumnNumber { get; private set; }
        public int RowNumber { get; private set; }
        public CellsNeighborhood Neighborhood { get; set; }
        public Point StartPositionOnImage { get; set; }
        public Point EndPositionOnImage { get; set; }

        public CellModel() : base(-1, false)
        {
            ColumnNumber = -1;
            RowNumber = -1;
        }

        public CellModel(bool isAlive) : base(-1, isAlive)
        {
            ColumnNumber = -1;
            RowNumber = -1;
        }

        public CellModel(int id, int columnNumber, int rowNumber, bool isAlive) : base(id, isAlive)
        {
            ColumnNumber = columnNumber;
            RowNumber = rowNumber;
        }

        public CellModel(CellModel obj) : base(obj)
        {
            ColumnNumber = obj.ColumnNumber;
            RowNumber = obj.RowNumber;
        }

        public CellModel(CellModel obj, CellsNeighborhood cellsNeighborhood) : base(obj)
        {
            ColumnNumber = obj.ColumnNumber;
            RowNumber = obj.RowNumber;
            Neighborhood = cellsNeighborhood;
        }

        public void Kill() => IsAlive = false;

        public void Revive() => IsAlive = true;
    }
}