using System;
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

        [ThreadStatic]
        private static Random random;

        public GrainCellNeighborhood()
        {
            if (random == null)
                random = new Random();

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
                var grainsCounts = new Dictionary<ICellState, int>();

                switch (Type)
                {
                    case CellNeighborhoodTypeModel.VonNeumann:
                        for (int sideIndex = 0; sideIndex < SIDES_COUNT; sideIndex += 2)
                            CountGrain(grainsCounts, sideIndex);

                        break;

                    case CellNeighborhoodTypeModel.Moore:
                        foreach (var grainCell in grainCells)
                        {
                            if (grainsCounts.ContainsKey(grainCell.State))
                                grainsCounts[grainCell.State]++;
                            else
                                grainsCounts.Add(grainCell.State, 1);
                        }
                        break;

                    case CellNeighborhoodTypeModel.RandomPentagonal:
                        const int TopPentagonal = 0;
                        const int RightPentagonal = 1;
                        const int BottomPentagonal = 2;
                        const int LeftPentagonal = 3;
                        int randomSide = random.Next(4);

                        switch (randomSide)
                        {
                            case TopPentagonal:
                                CountGrainsForPentagonalNeighborhood(6, grainsCounts);
                                break;

                            case RightPentagonal:
                                CountGrainsForPentagonalNeighborhood(0, grainsCounts);
                                break;

                            case BottomPentagonal:
                                CountGrainsForPentagonalNeighborhood(2, grainsCounts);
                                break;

                            case LeftPentagonal:
                                CountGrainsForPentagonalNeighborhood(4, grainsCounts);
                                break;
                        }
                        break;
                }

                return grainsCounts;
            }
        }

        private void CountGrainsForPentagonalNeighborhood(int startingIndex, Dictionary<ICellState, int> grainsCounts)
        {
            for (int sideIndex = startingIndex, checkedCells = 0; checkedCells < 5; sideIndex++, checkedCells++)
            {
                if (sideIndex == 8)
                    sideIndex = 0;

                CountGrain(grainsCounts, sideIndex);
            }
        }

        private void CountGrain(Dictionary<ICellState, int> grainsCounts, int sideIndex)
        {
            if (grainsCounts.ContainsKey(grainCells[sideIndex].State))
                grainsCounts[grainCells[sideIndex].State]++;
            else
                grainsCounts.Add(grainCells[sideIndex].State, 1);
        }
    }
}