using GameOfLife.Models;
using System.Collections.Generic;

namespace GrainGrowthCellularAutomaton.Models
{
    internal class GrainCellNeighborhood
    {
        private const int SIDES_COUNT = 8;

        public CellsNeighborhoodTypeModel Type { get; set; }
        public GrainCellModel Top { get => grainCells[0]; set => grainCells[0] = value; }
        public GrainCellModel TopRight { get => grainCells[1]; set => grainCells[1] = value; }
        public GrainCellModel Right { get => grainCells[2]; set => grainCells[2] = value; }
        public GrainCellModel BottomRight { get => grainCells[3]; set => grainCells[3] = value; }
        public GrainCellModel Bottom { get => grainCells[4]; set => grainCells[4] = value; }
        public GrainCellModel BottomLeft { get => grainCells[5]; set => grainCells[5] = value; }
        public GrainCellModel Left { get => grainCells[6]; set => grainCells[6] = value; }
        public GrainCellModel TopLeft { get => grainCells[7]; set => grainCells[7] = value; }
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
            Type = CellsNeighborhoodTypeModel.VonNeumann;
        }

        public GrainCellNeighborhood(GrainCellModel top, GrainCellModel topRight, GrainCellModel right, GrainCellModel bottomRight,
            GrainCellModel bottom, GrainCellModel bottomLeft, GrainCellModel left, GrainCellModel topLeft) : this(top, right, bottom, left)
        {
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
            TopLeft = topLeft;
            Type = CellsNeighborhoodTypeModel.Moore;
        }

        public GrainCellNeighborhood(GrainCellNeighborhood obj) : this()
        {
            switch (obj.Type)
            {
                case CellsNeighborhoodTypeModel.VonNeumann:
                    Top = new GrainCellModel(obj.Top);
                    Right = new GrainCellModel(obj.Right);
                    Bottom = new GrainCellModel(obj.Bottom);
                    Left = new GrainCellModel(obj.Left);
                    Type = obj.Type;
                    break;

                case CellsNeighborhoodTypeModel.Moore:
                    Top = new GrainCellModel(obj.Top);
                    TopRight = new GrainCellModel(obj.TopRight);
                    Right = new GrainCellModel(obj.Right);
                    BottomRight = new GrainCellModel(obj.BottomRight);
                    Bottom = new GrainCellModel(obj.Bottom);
                    BottomLeft = new GrainCellModel(obj.BottomLeft);
                    Left = new GrainCellModel(obj.Left);
                    TopLeft = new GrainCellModel(obj.TopLeft);
                    Type = obj.Type;
                    break;
            }
        }

        public Dictionary<GrainModel, uint> GrainsCounts
        {
            get
            {
                var grainsCount = new Dictionary<GrainModel, uint>();

                switch (Type)
                {
                    case CellsNeighborhoodTypeModel.VonNeumann:
                        for (int sideIndex = 0; sideIndex < SIDES_COUNT; sideIndex += 2)
                        {
                            if (grainsCount.ContainsKey(grainCells[sideIndex].Grain))
                                grainsCount[grainCells[sideIndex].Grain]++;
                            else
                                grainsCount.Add(grainCells[sideIndex].Grain, 1);
                        }
                        break;

                    case CellsNeighborhoodTypeModel.Moore:
                        foreach (var grainCell in grainCells)
                        {
                            if (grainsCount.ContainsKey(grainCell.Grain))
                                grainsCount[grainCell.Grain]++;
                            else
                                grainsCount.Add(grainCell.Grain, 1);
                        }
                        break;
                }

                return grainsCount;
            }
        }
    }
}