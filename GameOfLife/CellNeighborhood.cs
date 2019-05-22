using GameOfLife.Models;
using CellularAutomaton2D;
using CellularAutomaton2D.Models;
using System.Collections.Generic;

namespace GameOfLife
{
    public class CellNeighborhood : IEightSidedCellNeighborhood
    {
        private const int SIDES_COUNT = 8;

        public CellNeighborhoodTypeModel Type { get; set; }
        public ICell Top { get => cells[0]; set => cells[0] = (CellModel)value; }
        public ICell TopRight { get => cells[1]; set => cells[1] = (CellModel)value; }
        public ICell Right { get => cells[2]; set => cells[2] = (CellModel)value; }
        public ICell BottomRight { get => cells[3]; set => cells[3] = (CellModel)value; }
        public ICell Bottom { get => cells[4]; set => cells[4] = (CellModel)value; }
        public ICell BottomLeft { get => cells[5]; set => cells[5] = (CellModel)value; }
        public ICell Left { get => cells[6]; set => cells[6] = (CellModel)value; }
        public ICell TopLeft { get => cells[7]; set => cells[7] = (CellModel)value; }
        private CellModel[] cells = new CellModel[SIDES_COUNT];

        public CellNeighborhood()
        {
            for (int sideIndex = 0; sideIndex < SIDES_COUNT; sideIndex++)
                cells[sideIndex] = new CellModel();
        }

        public CellNeighborhood(CellModel top, CellModel right, CellModel bottom, CellModel left) : this()
        {
            Top = top;
            Right = right;
            Bottom = bottom;
            Left = left;
            Type = CellNeighborhoodTypeModel.VonNeumann;
        }

        public CellNeighborhood(CellModel top, CellModel topRight, CellModel right, CellModel bottomRight,
            CellModel bottom, CellModel bottomLeft, CellModel left, CellModel topLeft) : this(top, right, bottom, left)
        {
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
            TopLeft = topLeft;
            Type = CellNeighborhoodTypeModel.Moore;
        }

        public CellNeighborhood(CellNeighborhood obj) : this()
        {
            switch (obj.Type)
            {
                case CellNeighborhoodTypeModel.VonNeumann:
                    Top = new CellModel((CellModel)obj.Top);
                    Right = new CellModel((CellModel)obj.Right);
                    Bottom = new CellModel((CellModel)obj.Bottom);
                    Left = new CellModel((CellModel)obj.Left);
                    Type = obj.Type;
                    break;

                case CellNeighborhoodTypeModel.Moore:
                    Top = new CellModel((CellModel)obj.Top);
                    TopRight = new CellModel((CellModel)obj.TopRight);
                    Right = new CellModel((CellModel)obj.Right);
                    BottomRight = new CellModel((CellModel)obj.BottomRight);
                    Bottom = new CellModel((CellModel)obj.Bottom);
                    BottomLeft = new CellModel((CellModel)obj.BottomLeft);
                    Left = new CellModel((CellModel)obj.Left);
                    TopLeft = new CellModel((CellModel)obj.TopLeft);
                    Type = obj.Type;
                    break;
            }
        }

        public Dictionary<ICellState, int> StatesCounts
        {
            get
            {
                var statesCounts = new Dictionary<ICellState, int>();

                switch (Type)
                {
                    case CellNeighborhoodTypeModel.VonNeumann:
                        for (int sideIndex = 0; sideIndex < SIDES_COUNT; sideIndex += 2)
                        {
                            if (statesCounts.ContainsKey(cells[sideIndex].State))
                                statesCounts[cells[sideIndex].State]++;
                            else
                                statesCounts.Add(cells[sideIndex].State, 1);
                        }
                        break;

                    case CellNeighborhoodTypeModel.Moore:
                        foreach (var cell in cells)
                        {
                            if (statesCounts.ContainsKey(cell.State))
                                statesCounts[cell.State]++;
                            else
                                statesCounts.Add(cell.State, 1);
                        }
                        break;
                }

                return statesCounts;
            }
        }
    }
}