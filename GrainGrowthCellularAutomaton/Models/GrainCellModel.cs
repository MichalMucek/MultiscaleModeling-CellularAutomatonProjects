using GrainGrowthCellularAutomaton.Models;
using CellularAutomaton2D;
using System.Windows;
using System;
using System.Linq;

namespace GrainGrowthCellularAutomaton
{
    internal class GrainCellModel : ICell
    {
        public int Id { get; private set; }
        public int ColumnNumber { get; private set; }
        public int RowNumber { get; private set; }
        public ICellState State { get; set; }
        public ICellNeighborhood NeighboringCells { get; set; }
        public Point CenterOfMass { get; private set; }
        public Point GlobalCenterOfMass { get; private set; }
        public Point StartPositionOnImage { get; set; }
        public Point EndPositionOnImage { get; set; }
        public double Width { get; private set; } = 1;
        public double Height { get; private set; } = 1;

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
            RandomizeNewCenterOfMass();
            SetGlobalCenterOfMass();
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
            RandomizeNewCenterOfMass();
            SetGlobalCenterOfMass();
        }

        public GrainCellModel(GrainCellModel obj)
        {
            Id = obj.Id;
            ColumnNumber = obj.ColumnNumber;
            RowNumber = obj.RowNumber;
            State = obj.State;
            CenterOfMass = new Point(obj.CenterOfMass.X, obj.CenterOfMass.Y);
            GlobalCenterOfMass = new Point(obj.GlobalCenterOfMass.X, obj.GlobalCenterOfMass.Y);
        }

        private void RandomizeNewCenterOfMass()
        {
            double x = GetRandomDouble(-Width / 2.0, Height / 2.0);
            double y = GetRandomDouble(-Width / 2.0, Height / 2.0);

            CenterOfMass = new Point(x, y);
        }

        private double GetRandomDouble(double minimum, double maximum)
            => random.NextDouble() * (maximum - minimum) + minimum;

        private void SetGlobalCenterOfMass()
        {
            GlobalCenterOfMass = new Point
            {
                X = CenterOfMass.X + ColumnNumber + Width / 2.0,
                Y = CenterOfMass.Y - RowNumber - Height / 2.0
            };
        }

        public double Energy
        {
            get
            {
                const double grainBoundaryEnergy = 1.0;
                int otherGrainsInNeighborhoodCount = 0;

                foreach (var grainCount in NeighboringCells.StatesCounts)
                    if (grainCount.Key != State)
                        otherGrainsInNeighborhoodCount += grainCount.Value;

                return grainBoundaryEnergy * otherGrainsInNeighborhoodCount;
            }
        }

        public bool IsOnGrainBoundary
        {
            get
            {
                if (NeighboringCells.StatesCounts.Count == 1)
                    if (NeighboringCells.StatesCounts.ElementAt(0).Key == State)
                        return true;

                return false;
            }
        }
    }
}