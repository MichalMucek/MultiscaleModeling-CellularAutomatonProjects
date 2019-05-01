using ElementaryCellularAutomaton.Models;
using GameOfLife.Models;
using System.Linq;
using System.Collections.Generic;

namespace GameOfLife
{
    class CellsNeighborhood
    {
        public CellsNeighborhoodTypeModel Type { get; private set; }
        public CellModel Top { get; private set; }
        public CellModel TopRight { get; private set; }
        public CellModel Right { get; private set; }
        public CellModel BottomRight { get; private set; }
        public CellModel Bottom { get; private set; }
        public CellModel BottomLeft { get; private set; }
        public CellModel Left { get; private set; }
        public CellModel TopLeft { get; private set; }
        private List<CellModel> cells = new List<CellModel>(8);

        private CellsNeighborhood(bool first = true)
        {
            cells.Add(Top);
            cells.Add(TopRight);
            cells.Add(Right);
            cells.Add(BottomRight);
            cells.Add(Bottom);
            cells.Add(BottomLeft);
            cells.Add(Left);
            cells.Add(TopLeft);
        }

        public CellsNeighborhood(CellModel top, CellModel right, CellModel bottom, CellModel left) : this(true)
        {
            Top = top;
            Right = right;
            Bottom = bottom;
            Left = left;
            Type = CellsNeighborhoodTypeModel.VonNeumann;
        }

        public CellsNeighborhood(CellModel top, CellModel topRight, CellModel right, CellModel bottomRight, 
            CellModel bottom, CellModel bottomLeft, CellModel left, CellModel topLeft) : this(top, right, bottom, left)
        {
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
            TopLeft = topLeft;
            Type = CellsNeighborhoodTypeModel.Moore;
        }

        public int GetAliveNeighboursCount() => cells.Where(x => x.IsAlive).Count();

        public int GetDeadNeighboursCount() => cells.Where(x => !x.IsAlive).Count();
    }
}
