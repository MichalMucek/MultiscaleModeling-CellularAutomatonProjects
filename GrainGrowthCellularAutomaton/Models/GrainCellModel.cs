using GrainGrowthCellularAutomaton.Models;

namespace GrainGrowthCellularAutomaton
{
    internal class GrainCellModel : GameOfLife.Models.CellModel
    {
        public GrainCellNeighborhood NeighboringGrainCells { get; set; }
        public GrainModel Grain { get; set; }

        public GrainCellModel()
            : base()
            => Grain = null;

        public GrainCellModel(GrainModel grain)
            : base()
            => Grain = grain;

        public GrainCellModel(int id, int columnNumber, int rowNumber, GrainModel grain)
            : base(id, columnNumber, rowNumber)
            => Grain = grain;

        public GrainCellModel(GrainCellModel obj)
            : base(obj)
            => Grain = obj.Grain;
    }
}