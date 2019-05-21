using System.Collections.Generic;
using CellularAutomaton2D;
using CellularAutomaton2D.Models;

namespace GrainGrowthCellularAutomaton.Models
{
    internal class GrainCellNeighborhood : IEightSidedCellNeighborhood
    {
        private const int SIDES_COUNT = 8;

        public CellNeighborhoodTypeModel Type { get; set; }
        public ICell Top { get => grainCells[0]; set => grainCells[0] = (GrainCellModel)value; }
        public ICell TopRight { get => grainCells[1]; set => grainCells[1] = (GrainCellModel)value; }
        public ICell Right { get => grainCells[2]; set => grainCells[2] = (GrainCellModel)value; }
        public ICell BottomRight { get => grainCells[3]; set => grainCells[3] = (GrainCellModel)value; }
        public ICell Bottom { get => grainCells[4]; set => grainCells[4] = (GrainCellModel)value; }
        public ICell BottomLeft { get => grainCells[5]; set => grainCells[5] = (GrainCellModel)value; }
        public ICell Left { get => grainCells[6]; set => grainCells[6] = (GrainCellModel)value; }
        public ICell TopLeft { get => grainCells[7]; set => grainCells[7] = (GrainCellModel)value; }
        private GrainCellModel[] grainCells = new GrainCellModel[SIDES_COUNT];

        public GrainCellNeighborhood()
        {
            for (int sideIndex = 0; sideIndex < SIDES_COUNT; sideIndex++)
                grainCells[sideIndex] = new GrainCellModel();
        }

        public GrainCellNeighborhood(GrainCellModel top, GrainCellModel right, GrainCellModel bottom, GrainCellModel left) : this()
        {
            Top = top;
            Right = right;
            Bottom = bottom;
            Left = left;
            Type = CellNeighborhoodTypeModel.VonNeumann;
        }

        public GrainCellNeighborhood(GrainCellModel top, GrainCellModel topRight, GrainCellModel right, GrainCellModel bottomRight,
            GrainCellModel bottom, GrainCellModel bottomLeft, GrainCellModel left, GrainCellModel topLeft) : this(top, right, bottom, left)
        {
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
            TopLeft = topLeft;
            Type = CellNeighborhoodTypeModel.Moore;
        }

        public GrainCellNeighborhood(GrainCellNeighborhood obj) : this()
        {
            switch (obj.Type)
            {
                case CellNeighborhoodTypeModel.VonNeumann:
                    Top = new GrainCellModel((GrainCellModel)obj.Top);
                    Right = new GrainCellModel((GrainCellModel)obj.Right);
                    Bottom = new GrainCellModel((GrainCellModel)obj.Bottom);
                    Left = new GrainCellModel((GrainCellModel)obj.Left);
                    Type = obj.Type;
                    break;

                case CellNeighborhoodTypeModel.Moore:
                    Top = new GrainCellModel((GrainCellModel)obj.Top);
                    TopRight = new GrainCellModel((GrainCellModel)obj.TopRight);
                    Right = new GrainCellModel((GrainCellModel)obj.Right);
                    BottomRight = new GrainCellModel((GrainCellModel)obj.BottomRight);
                    Bottom = new GrainCellModel((GrainCellModel)obj.Bottom);
                    BottomLeft = new GrainCellModel((GrainCellModel)obj.BottomLeft);
                    Left = new GrainCellModel((GrainCellModel)obj.Left);
                    TopLeft = new GrainCellModel((GrainCellModel)obj.TopLeft);
                    Type = obj.Type;
                    break;
            }
        }

        public Dictionary<ICellState, int> StatesCounts
        {
            get
            {
                var grainsCount = new Dictionary<ICellState, int>();

                switch (Type)
                {
                    case CellNeighborhoodTypeModel.VonNeumann:
                        for (int sideIndex = 0; sideIndex < SIDES_COUNT; sideIndex += 2)
                        {
                            if (grainsCount.ContainsKey(grainCells[sideIndex].State))
                                grainsCount[grainCells[sideIndex].State]++;
                            else
                                grainsCount.Add(grainCells[sideIndex].State, 1);
                        }
                        break;

                    case CellNeighborhoodTypeModel.Moore:
                        foreach (var grainCell in grainCells)
                        {
                            if (grainsCount.ContainsKey(grainCell.State))
                                grainsCount[grainCell.State]++;
                            else
                                grainsCount.Add(grainCell.State, 1);
                        }
                        break;
                }

                return grainsCount;
            }
        }
    }
}