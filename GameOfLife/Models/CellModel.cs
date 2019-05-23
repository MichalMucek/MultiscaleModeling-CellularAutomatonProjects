using CellularAutomaton2D;
using System.Drawing;

namespace GameOfLife.Models
{
    public class CellModel : ICell
    {
        public int Id { get; private set; }
        public int ColumnNumber { get; private set; }
        public int RowNumber { get; private set; }
        public ICellState State { get; set; }
        public CellStateModel AliveState { get; private set; }
        public CellStateModel DeadState { get; private set; }
        public ICellNeighborhood NeighboringCells { get; set; }
        public Point StartPositionOnImage { get; set; }
        public Point EndPositionOnImage { get; set; }

        public CellModel()
        {
            Id = -1;
            ColumnNumber = -1;
            RowNumber = -1;
            State = null;
            AliveState = null;
            DeadState = null;
        }

        public CellModel(bool isAlive, CellStateModel aliveState, CellStateModel deadState)
            : this()
        {
            AliveState = aliveState;
            DeadState = deadState;

            State = isAlive ? AliveState : DeadState;
        }

        public CellModel(CellStateModel cellState, CellStateModel aliveState, CellStateModel deadState)
            : this()
        {
            State = cellState;
            AliveState = aliveState;
            DeadState = deadState;
        }

        public CellModel(int id, int columnNumber, int rowNumber,
            CellStateModel cellState, CellStateModel aliveState, CellStateModel deadState)
            : this(cellState, aliveState, deadState)
        {
            Id = id;
            ColumnNumber = columnNumber;
            RowNumber = rowNumber;
        }

        public CellModel(CellModel obj)
        {
            Id = obj.Id;
            ColumnNumber = obj.ColumnNumber;
            RowNumber = obj.RowNumber;
            State = obj.State;
            AliveState = obj.AliveState;
            DeadState = obj.DeadState;
        }

        public void Kill()
            => State = DeadState;

        public void Revive()
            => State = AliveState;

        public bool IsAlive
        {
            get => State == AliveState;
            set
            {
                if (value == true)
                    State = AliveState;
                else
                    State = DeadState;
            }
        }

        public bool IsDead
        {
            get => State == DeadState;
            set
            {
                if (value == true)
                    State = DeadState;
                else
                    State = AliveState;
            }
        }
    }
}