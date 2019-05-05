using GameOfLife.Models;
using System.Linq;

namespace GameOfLife
{
    internal class CellsNeighborhood
    {
        private const int SIDES_COUNT = 8;

        public CellsNeighborhoodTypeModel Type { get; set; }
        public CellModel Top { get => cells[0]; set => cells[0] = value; }
        public CellModel TopRight { get => cells[1]; set => cells[1] = value; }
        public CellModel Right { get => cells[2]; set => cells[2] = value; }
        public CellModel BottomRight { get => cells[3]; set => cells[3] = value; }
        public CellModel Bottom { get => cells[4]; set => cells[4] = value; }
        public CellModel BottomLeft { get => cells[5]; set => cells[5] = value; }
        public CellModel Left { get => cells[6]; set => cells[6] = value; }
        public CellModel TopLeft { get => cells[7]; set => cells[7] = value; }
        private CellModel[] cells = new CellModel[SIDES_COUNT];

        public CellsNeighborhood()
        {
            for (int sideIndex = 0; sideIndex < SIDES_COUNT; sideIndex++)
                cells[sideIndex] = new CellModel();
        }

        public CellsNeighborhood(CellModel top, CellModel right, CellModel bottom, CellModel left) : this()
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

        public CellsNeighborhood(CellsNeighborhood obj) : this()
        {
            switch (obj.Type)
            {
                case CellsNeighborhoodTypeModel.VonNeumann:
                    Top = new CellModel(obj.Top);
                    Right = new CellModel(obj.Right);
                    Bottom = new CellModel(obj.Bottom);
                    Left = new CellModel(obj.Left);
                    Type = obj.Type;
                    break;

                case CellsNeighborhoodTypeModel.Moore:
                    Top = new CellModel(obj.Top);
                    TopRight = new CellModel(obj.TopRight);
                    Right = new CellModel(obj.Right);
                    BottomRight = new CellModel(obj.BottomRight);
                    Bottom = new CellModel(obj.Bottom);
                    BottomLeft = new CellModel(obj.BottomLeft);
                    Left = new CellModel(obj.Left);
                    TopLeft = new CellModel(obj.TopLeft);
                    Type = obj.Type;
                    break;
            }
        }

        public int AliveNeighboursCount
        {
            get
            {
                int count = 0;

                switch (Type)
                {
                    case CellsNeighborhoodTypeModel.VonNeumann:
                        for (int sideIndex = 0; sideIndex < SIDES_COUNT; sideIndex += 2)
                            if (cells[sideIndex].IsAlive) count++;
                        break;

                    case CellsNeighborhoodTypeModel.Moore:
                        count = cells.Where(x => x.IsAlive).Count();
                        break;
                }

                return count;
            }
        }

        public int DeadNeighboursCount => (int)Type - 1 - AliveNeighboursCount;
    }
}