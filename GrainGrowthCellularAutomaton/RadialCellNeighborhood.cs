using System;
using System.Collections.Generic;
using CellularAutomaton2D;
using CellularAutomaton2D.Models;

namespace GrainGrowthCellularAutomaton
{
    internal class RadialGrainCellNeighborhood : ICellNeighborhood
    {
        public CellNeighborhoodTypeModel Type { get; private set; }
        public List<GrainCellModel> GrainCells { get; private set; } = new List<GrainCellModel>();
        public int Count => GrainCells.Count;
        private Dictionary<ICellState, int> grainsCounts;

        public RadialGrainCellNeighborhood(List<GrainCellModel> grainCells, CellNeighborhoodTypeModel type)
        {
            Type = type;
            GrainCells = grainCells;
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
                        foreach (GrainCellModel grainCell in GrainCells)
                            if (grainsCounts.ContainsKey(grainCell.State))
                                grainsCounts[grainCell.State]++;
                            else
                                grainsCounts.Add(grainCell.State, 1);
                        break;

                    default:
                        throw new ArgumentException("RadialGrainCellNeighborhood can be of radial type only");
                }

                return grainsCounts;
            }
        }
    }
}