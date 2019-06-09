using GrainGrowthCellularAutomaton.Models;
using CellularAutomaton2D;
using CellularAutomaton2D.Models;
using System.Windows;
using System;
using System.Linq;
using System.Collections.Generic;

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
        public double DislocationDensity { get; set; } = 0;
        public bool IsRecrystallized { get; set; } = false;
        private GrainCellModel thisGrainCellWithVonNeumannNeighborhood;

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
            Width = obj.Width;
            Height = obj.Height;
            DislocationDensity = obj.DislocationDensity;
            IsRecrystallized = obj.IsRecrystallized;
            VonNeumannCellNeighborhood = obj.VonNeumannCellNeighborhood;
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

        public bool IsOnGrainBoundary => thisGrainCellWithVonNeumannNeighborhood.Energy != 0;

        public EightSidedGrainCellNeighborhood VonNeumannCellNeighborhood
        {
            get
            {
                try
                {
                    return (EightSidedGrainCellNeighborhood)thisGrainCellWithVonNeumannNeighborhood.NeighboringCells;
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine(e.StackTrace);
                    throw new NullReferenceException("thisGrainCellWithVonNeumannNeighborhood has to be created first");
                }
            }
            set
            {
                if (thisGrainCellWithVonNeumannNeighborhood == null)
                    thisGrainCellWithVonNeumannNeighborhood = new GrainCellModel();

                thisGrainCellWithVonNeumannNeighborhood.State = State;
                thisGrainCellWithVonNeumannNeighborhood.NeighboringCells = value;
            }
        }

        public List<GrainCellModel> RecrystallizedNeighboringGrainCells
        {
            get
            {
                var recrystallizedNeighboringCells = new List<GrainCellModel>();

                switch (NeighboringCells.Type)
                {
                    case CellNeighborhoodTypeModel.Radial:
                    case CellNeighborhoodTypeModel.RadialWithCenterOfMass:
                        foreach (var grainCell in ((RadialGrainCellNeighborhood)NeighboringCells).GrainCells)
                            if (grainCell.IsRecrystallized)
                                recrystallizedNeighboringCells.Add(grainCell);

                        break;

                    default:
                        foreach (var grainCell in ((EightSidedGrainCellNeighborhood)NeighboringCells).GrainCells)
                            if (grainCell.IsRecrystallized)
                                recrystallizedNeighboringCells.Add(grainCell);
                        break;
                }

                return recrystallizedNeighboringCells;
            }
        }

        public Dictionary<ICellState, int> RecrystallizedNeighboringGrainsCounts
        {
            get
            {
                var recrystallizedNeighboringGrainsCounts = new Dictionary<ICellState, int>();

                foreach (var grainCell in RecrystallizedNeighboringGrainCells)
                    if (recrystallizedNeighboringGrainsCounts.ContainsKey(grainCell.State))
                        recrystallizedNeighboringGrainsCounts[grainCell.State]++;
                    else
                        recrystallizedNeighboringGrainsCounts.Add(grainCell.State, 1);

                if (RecrystallizedNeighboringGrainCells.Any())
                    if (recrystallizedNeighboringGrainsCounts.Count == 0)
                        throw new Exception("Coś poczło nie tak, głębiej");

                return recrystallizedNeighboringGrainsCounts;
            }
        }

        public List<GrainCellModel> NeighboringGrainCellsList
        {
            get
            {
                switch (NeighboringCells.Type)
                {
                    case CellNeighborhoodTypeModel.Radial:
                    case CellNeighborhoodTypeModel.RadialWithCenterOfMass:
                        return ((RadialGrainCellNeighborhood)NeighboringCells).GrainCells;

                    default:
                        return ((EightSidedGrainCellNeighborhood)NeighboringCells).GrainCells;
                }
            }
        }
    }
}