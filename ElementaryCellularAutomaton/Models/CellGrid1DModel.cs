using System.Collections.Generic;

namespace ElementaryCellularAutomaton.Models
{
    public class CellGrid1DModel
    {
        private List<CellModel> CurrentState { get; set; } = new List<CellModel>();
        private LinkedList<CellModel[]> PreviousStates { get; set; } = new LinkedList<CellModel[]>();
        private CellModel[] PreviousState { get; set; }
        public RuleModel Rule { get; private set; }
        public BoundaryConditionModel BoundaryCondition { get; set; }
        public readonly int Size;
        public readonly int FirstCellId = 1;
        public readonly int LastCellId;
        public int GenerationCount { get; private set; } = 0;

        public CellGrid1DModel(int size, RuleModel rule, BoundaryConditionModel boundaryCondition)
        {
            Size = size;
            LastCellId = size;
            Rule = rule;
            BoundaryCondition = boundaryCondition;
            AddNewCells();
        }

        private void AddNewCells()
        {
            int centerAliveCellId = Size / 2;

            for (int cellId = 1; cellId <= Size; cellId++)
                CurrentState.Add(new CellModel(cellId, centerAliveCellId == cellId));
        }

        public void Evolve()
        {
            CellsNeighborhood cellsNeighborhood = new CellsNeighborhood();

            CopyCurrentStateToPreviousStates();

            for (int evolvingCellId = 1; evolvingCellId <= Size; evolvingCellId++)
            {
                CellModel evolvingCell = CurrentState[evolvingCellId];

                if (IsCellBoundary(evolvingCell))
                    cellsNeighborhood = GetCellsNeighborhoodForNotBoundaryCell(evolvingCellId);
                else
                {
                    switch (BoundaryCondition)
                    {
                        case BoundaryConditionModel.FalseOutside:
                            cellsNeighborhood = GetCellsNeighborhoodForFalseOutsideBc(evolvingCellId);
                            break;
                        case BoundaryConditionModel.TrueOutside:
                            cellsNeighborhood = GetCellsNeighborhoodForTrueOutsideBc(evolvingCellId);
                            break;
                        case BoundaryConditionModel.Periodical:
                            cellsNeighborhood = GetCellsNeighborhoodForPeriodicalBc(evolvingCellId);
                            break;
                    }
                }

                evolvingCell.IsAlive = Rule.Table[cellsNeighborhood];
            }

            GenerationCount++;
        }

        private void CopyCurrentStateToPreviousStates()
        {
            PreviousStates.AddLast(new CellModel[Size]);
            CurrentState.CopyTo(PreviousStates.Last.Value);
            PreviousState = PreviousStates.Last.Value;
        }

        private bool IsCellBoundary(CellModel cell) => cell.Id != 1 && cell.Id != Size;

        private CellsNeighborhood GetCellsNeighborhoodForNotBoundaryCell(int cellId)
            => new CellsNeighborhood {
                Left = PreviousState[cellId - 1].IsAlive,
                Center = PreviousState[cellId].IsAlive,
                Right = PreviousState[cellId - 1].IsAlive
            };

        private CellsNeighborhood GetCellsNeighborhoodForFalseOutsideBc(int cellId)
            => GetCellsNeighborhoodForTrueOrFalseOutsideBc(cellId, false);

        private CellsNeighborhood GetCellsNeighborhoodForTrueOutsideBc(int cellId)
            => GetCellsNeighborhoodForTrueOrFalseOutsideBc(cellId, true);

        private CellsNeighborhood GetCellsNeighborhoodForTrueOrFalseOutsideBc(int cellId, bool isOutsideAlive)
        {
            CellsNeighborhood cellsNeighborhood = new CellsNeighborhood();

            if (cellId == FirstCellId)
            {
                cellsNeighborhood.Left = isOutsideAlive;
                cellsNeighborhood.Center = PreviousState[cellId].IsAlive;
                cellsNeighborhood.Right = PreviousState[cellId + 1].IsAlive;
            }

            if (cellId == LastCellId)
            {
                cellsNeighborhood.Left = PreviousState[cellId - 1].IsAlive;
                cellsNeighborhood.Center = PreviousState[cellId].IsAlive;
                cellsNeighborhood.Right = isOutsideAlive;
            }

            return cellsNeighborhood;
        }

        private CellsNeighborhood GetCellsNeighborhoodForPeriodicalBc(int cellId)
        {
            CellsNeighborhood cellsNeighborhood = new CellsNeighborhood();

            if (cellId == FirstCellId)
            {
                cellsNeighborhood.Left = PreviousState[LastCellId].IsAlive;
                cellsNeighborhood.Center = PreviousState[cellId].IsAlive;
                cellsNeighborhood.Right = PreviousState[cellId + 1].IsAlive;
            }

            if (cellId == LastCellId)
            {
                cellsNeighborhood.Left = PreviousState[cellId - 1].IsAlive; 
                cellsNeighborhood.Center = PreviousState[cellId].IsAlive;
                cellsNeighborhood.Right = PreviousState[FirstCellId].IsAlive;
            }

            return cellsNeighborhood;
        }
    }
}
