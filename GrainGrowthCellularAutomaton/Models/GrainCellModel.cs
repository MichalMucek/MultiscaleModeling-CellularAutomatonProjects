using GrainGrowthCellularAutomaton.Models;
using CellularAutomaton2D;
using System.Windows;
using System;

namespace GrainGrowthCellularAutomaton
{
    internal class GrainCellModel : ICell
    {
        public int Id { get; private set; }
        public int ColumnNumber { get; private set; }
        public int RowNumber { get; private set; }
        public ICellState State { get; set; }
        public ICellNeighborhood NeighboringCells { get; set; }
        public Point CenterOfMass { get; set; }
        public Point StartPositionOnImage { get; set; }
        public Point EndPositionOnImage { get; set; }

        [ThreadStatic]
        private static Random random;

        public GrainCellModel()
        {
            if (random == null)
                random = new Random();

            Id = -1;
            ColumnNumber = -1;
            RowNumber = -1;
            State = null;
            RandomizeCenterOfMass();
        }

        public GrainCellModel(GrainModel grain)
            : this()
            => State = grain;

        public GrainCellModel(int id, int columnNumber, int rowNumber, GrainModel grain)
        {
            if (random == null)
                random = new Random();

            Id = id;
            ColumnNumber = columnNumber;
            RowNumber = rowNumber;
            State = grain;
            RandomizeCenterOfMass();
        }

        public GrainCellModel(GrainCellModel obj)
        {
            Id = obj.Id;
            ColumnNumber = obj.ColumnNumber;
            RowNumber = obj.RowNumber;
            State = obj.State;
            CenterOfMass = new Point(obj.CenterOfMass.X, obj.CenterOfMass.Y);
        }

        private void RandomizeCenterOfMass()
        {
            double x = GetRandomNumber(-0.5, 0.5);
            double y = GetRandomNumber(-0.5, 0.5);

            CenterOfMass = new Point(x, y);
        }

        private double GetRandomNumber(double minimum, double maximum)
            => random.NextDouble() * (maximum - minimum) + minimum;
    }
}