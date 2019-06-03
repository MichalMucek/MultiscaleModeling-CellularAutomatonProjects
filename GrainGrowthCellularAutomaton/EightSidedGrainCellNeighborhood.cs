using System;
using System.Collections.Generic;
using CellularAutomaton2D;
using CellularAutomaton2D.Models;

namespace GrainGrowthCellularAutomaton.Models
{
    internal class EightSidedGrainCellNeighborhood : ICellNeighborhood
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
        private Dictionary<ICellState, int> grainsCounts;

        [ThreadStatic]
        private static Random random;

        public EightSidedGrainCellNeighborhood()
        {
            if (random == null)
                random = new Random();

            for (int sideIndex = 0; sideIndex < SIDES_COUNT; sideIndex++)
                grainCells[sideIndex] = new GrainCellModel();
        }

        public EightSidedGrainCellNeighborhood(GrainCellModel top, GrainCellModel right, GrainCellModel bottom, GrainCellModel left) : this()
        {
            Top = top;
            Right = right;
            Bottom = bottom;
            Left = left;
            Type = CellNeighborhoodTypeModel.VonNeumann;
        }

        public EightSidedGrainCellNeighborhood(GrainCellModel top, GrainCellModel topRight, GrainCellModel right, GrainCellModel bottomRight,
            GrainCellModel bottom, GrainCellModel bottomLeft, GrainCellModel left, GrainCellModel topLeft) : this(top, right, bottom, left)
        {
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
            TopLeft = topLeft;
            Type = CellNeighborhoodTypeModel.Moore;
        }

        public Dictionary<ICellState, int> StatesCounts
        {
            get
            {
                if (random == null)
                    random = new Random();

                grainsCounts = new Dictionary<ICellState, int>();

                switch (Type)
                {
                    case CellNeighborhoodTypeModel.VonNeumann:
                        for (int sideIndex = 0; sideIndex < SIDES_COUNT; sideIndex += 2)
                            CountGrain(sideIndex);

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
                                CountGrainsForPentagonalNeighborhood(6);
                                break;

                            case RightPentagonal:
                                CountGrainsForPentagonalNeighborhood(0);
                                break;

                            case BottomPentagonal:
                                CountGrainsForPentagonalNeighborhood(2);
                                break;

                            case LeftPentagonal:
                                CountGrainsForPentagonalNeighborhood(4);
                                break;
                        }
                        break;

                    case CellNeighborhoodTypeModel.LeftHexagonal:
                        CountGrainsForLeftHexagonalNeighborhood();
                        break;

                    case CellNeighborhoodTypeModel.RightHexagonal:
                        CountGrainsForRightHexagonalNeighborhood();
                        break;

                    case CellNeighborhoodTypeModel.RandomHexagonal:
                        const int LeftHexagonal = 0;
                        const int RightHexagonal = 1;

                        int randomHexagonal = random.Next(2);

                        switch (randomHexagonal)
                        {
                            case LeftHexagonal:
                                CountGrainsForLeftHexagonalNeighborhood();
                                break;

                            case RightHexagonal:
                                CountGrainsForRightHexagonalNeighborhood();
                                break;
                        }
                        break;
                }

                return grainsCounts;
            }
        }

        private void CountGrainsForPentagonalNeighborhood(int startingIndex)
        {
            for (int sideIndex = startingIndex, checkedCells = 0; checkedCells < 5; sideIndex++, checkedCells++)
            {
                if (sideIndex == 8)
                    sideIndex = 0;

                CountGrain(sideIndex);
            }
        }

        private void CountGrainsForLeftHexagonalNeighborhood()
        {
            CountGrainsForHexagonalNeighborhoodCorner(0);
            CountGrainsForHexagonalNeighborhoodCorner(4);
        }

        private void CountGrainsForRightHexagonalNeighborhood()
        {
            CountGrainsForHexagonalNeighborhoodCorner(6);
            CountGrainsForHexagonalNeighborhoodCorner(2);
        }

        private void CountGrainsForHexagonalNeighborhoodCorner(int startingIndex)
        {
            for (int sideIndex = startingIndex, checkedCells = 0; checkedCells < 3; sideIndex++, checkedCells++)
            {
                if (sideIndex == 8)
                    sideIndex = 0;

                CountGrain(sideIndex);
            }
        }

        private void CountGrain(int sideIndex)
        {
            if (grainsCounts.ContainsKey(grainCells[sideIndex].State))
                grainsCounts[grainCells[sideIndex].State]++;
            else
                grainsCounts.Add(grainCells[sideIndex].State, 1);
        }
    }
}