using System.Collections.Generic;
using CellularAutomaton2D;
using CellularAutomaton2D.Models;

namespace GrainGrowthCellularAutomaton
{
    internal class RadialGrainCellNeighborhood : ICellNeighborhood
    {
        public CellNeighborhoodTypeModel Type { get; private set; }
        public List<ICell> Cells { get; private set; } = new List<ICell>();
        private Dictionary<ICellState, int> grainsCounts;

        public RadialGrainCellNeighborhood(List<ICell> cells, CellNeighborhoodTypeModel type)
        {
            Type = type;
            Cells = cells;
        }

        public Dictionary<ICellState, int> StatesCounts
        {
            get
            {
                grainsCounts = new Dictionary<ICellState, int>();

                switch (Type)
                {
                    case CellNeighborhoodTypeModel.Radial:
                    case CellNeighborhoodTypeModel.RadialWithCenterOfMass:
                        foreach (GrainCellModel grainCell in Cells)
                            if (grainsCounts.ContainsKey(grainCell.State))
                                grainsCounts[grainCell.State]++;
                            else
                                grainsCounts.Add(grainCell.State, 1);
                        break;
                }

                return grainsCounts;
            }
        }
    }
}