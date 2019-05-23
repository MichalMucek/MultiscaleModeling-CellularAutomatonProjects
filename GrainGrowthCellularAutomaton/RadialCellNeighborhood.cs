using System.Collections.Generic;
using CellularAutomaton2D;
using CellularAutomaton2D.Models;

namespace GrainGrowthCellularAutomaton
{
    internal class RadialGrainCellNeighborhood : IRadialCellNeighborhood
    {
        public CellNeighborhoodTypeModel Type { get; set; }
        public List<ICell> Cells { get; private set; }
        private Dictionary<ICellState, int> grainsCounts;

        public Dictionary<ICellState, int> StatesCount
        {
            get
            {
                grainsCounts = new Dictionary<ICellState, int>();

                switch (Type)
                {
                    case CellNeighborhoodTypeModel.CellsWithinRadius:
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