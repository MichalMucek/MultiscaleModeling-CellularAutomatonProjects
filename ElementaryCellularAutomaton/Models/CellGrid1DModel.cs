﻿using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ElementaryCellularAutomaton.Models
{
    public class CellGrid1DModel
    {
        private List<CellModel> CurrentState { get; set; } = new List<CellModel>();
        private List<List<CellModel>> PreviousStates { get; set; } = new List<List<CellModel>>();
        private List<CellModel> PreviousState { get; set; }
        public RuleModel Rule { get; private set; }
        public BoundaryConditionModel BoundaryCondition { get; set; }
        public readonly int Size;
        public readonly int FirstCellId = 0;
        public readonly int LastCellId;
        public int GenerationCount { get; private set; } = 1;

        private MemoryStream imageMemoryStream;
        private BitmapImage bitmapImage;

        public CellGrid1DModel(int size, RuleModel rule, BoundaryConditionModel boundaryCondition)
        {
            Size = size;
            LastCellId = size - 1;
            Rule = rule;
            BoundaryCondition = boundaryCondition;
            AddNewCells();
        }

        private void AddNewCells()
        {
            int centerAliveCellId = Size / 2;

            for (int cellId = 0; cellId < Size; cellId++)
                CurrentState.Add(new CellModel(cellId, centerAliveCellId == cellId));
        }

        public void Evolve()
        {
            CellsNeighborhood cellsNeighborhood = new CellsNeighborhood();

            CopyCurrentStateToPreviousStates();

            foreach (var evolvingCell in PreviousState)
            {
                if (IsCellBoundary(evolvingCell))
                    cellsNeighborhood = GetCellsNeighborhoodForNotBoundaryCell(evolvingCell);
                else
                {
                    switch (BoundaryCondition)
                    {
                        case BoundaryConditionModel.Absorbing:
                            cellsNeighborhood = GetCellsNeighborhoodForFalseOutsideBc(evolvingCell);
                            break;

                        case BoundaryConditionModel.CounterAbsorbing:
                            cellsNeighborhood = GetCellsNeighborhoodForTrueOutsideBc(evolvingCell);
                            break;

                        case BoundaryConditionModel.Periodic:
                            cellsNeighborhood = GetCellsNeighborhoodForPeriodicalBc(evolvingCell);
                            break;
                    }
                }

                CurrentState[evolvingCell.Id].IsAlive = Rule.Table[cellsNeighborhood];
            }

            GenerationCount++;
        }

        private void CopyCurrentStateToPreviousStates()
        {
            PreviousStates.Add(CurrentState.ConvertAll(x => new CellModel(x)));
            PreviousState = PreviousStates.Last();
        }

        private bool IsCellBoundary(CellModel cell) => cell.Id != FirstCellId && cell.Id != LastCellId;

        private CellsNeighborhood GetCellsNeighborhoodForNotBoundaryCell(CellModel cell)
            => new CellsNeighborhood
            {
                Left = PreviousState[cell.Id - 1].IsAlive,
                Center = PreviousState[cell.Id].IsAlive,
                Right = PreviousState[cell.Id + 1].IsAlive
            };

        private CellsNeighborhood GetCellsNeighborhoodForFalseOutsideBc(CellModel cell)
            => GetCellsNeighborhoodForTrueOrFalseOutsideBc(cell, false);

        private CellsNeighborhood GetCellsNeighborhoodForTrueOutsideBc(CellModel cell)
            => GetCellsNeighborhoodForTrueOrFalseOutsideBc(cell, true);

        private CellsNeighborhood GetCellsNeighborhoodForTrueOrFalseOutsideBc(CellModel cell, bool isOutsideAlive)
        {
            CellsNeighborhood cellsNeighborhood = new CellsNeighborhood();

            if (cell.Id == FirstCellId)
            {
                cellsNeighborhood.Left = isOutsideAlive;
                cellsNeighborhood.Center = PreviousState[cell.Id].IsAlive;
                cellsNeighborhood.Right = PreviousState[cell.Id + 1].IsAlive;
            }

            if (cell.Id == LastCellId)
            {
                cellsNeighborhood.Left = PreviousState[cell.Id - 1].IsAlive;
                cellsNeighborhood.Center = PreviousState[cell.Id].IsAlive;
                cellsNeighborhood.Right = isOutsideAlive;
            }

            return cellsNeighborhood;
        }

        private CellsNeighborhood GetCellsNeighborhoodForPeriodicalBc(CellModel cell)
        {
            CellsNeighborhood cellsNeighborhood = new CellsNeighborhood();

            if (cell.Id == FirstCellId)
            {
                cellsNeighborhood.Left = PreviousState[LastCellId].IsAlive;
                cellsNeighborhood.Center = PreviousState[cell.Id].IsAlive;
                cellsNeighborhood.Right = PreviousState[cell.Id + 1].IsAlive;
            }

            if (cell.Id == LastCellId)
            {
                cellsNeighborhood.Left = PreviousState[cell.Id - 1].IsAlive;
                cellsNeighborhood.Center = PreviousState[cell.Id].IsAlive;
                cellsNeighborhood.Right = PreviousState[FirstCellId].IsAlive;
            }

            return cellsNeighborhood;
        }

        public BitmapImage GetBitmapImage()
        {
            imageMemoryStream?.Dispose();
            imageMemoryStream = new MemoryStream();
            bitmapImage = new BitmapImage();

            const int CELL_WIDTH = 5;
            const int CELL_HEIGHT = 5;

            int imageWidth = Size * CELL_WIDTH;
            int imageHeight = GenerationCount * CELL_HEIGHT;

            Point cellLocation = new Point();
            Size cellSize = new Size(CELL_WIDTH, CELL_HEIGHT);

            Bitmap image = new Bitmap(imageWidth, imageHeight);
            SolidBrush blackSolidBrush = new SolidBrush(Color.Black);

            using (var imageGraphics = Graphics.FromImage(image))
            {
                imageGraphics.Clear(Color.White);

                if (PreviousStates.Any())
                {
                    for (int prevStateIndex = 0; prevStateIndex < PreviousStates.Count; prevStateIndex++)
                    {
                        var previousState = PreviousStates[prevStateIndex];

                        for (int cellIndex = 0; cellIndex < previousState.Count; cellIndex++)
                        {
                            cellLocation.X = cellIndex * CELL_WIDTH;
                            cellLocation.Y = prevStateIndex * CELL_HEIGHT;

                            if (previousState[cellIndex].IsAlive)
                                imageGraphics.FillRectangle(blackSolidBrush, new Rectangle(cellLocation, cellSize));
                        }
                    }
                }

                for (int cellIndex = 0; cellIndex < CurrentState.Count; cellIndex++)
                {
                    cellLocation.X = cellIndex * CELL_WIDTH;
                    cellLocation.Y = (GenerationCount - 1) * CELL_HEIGHT;

                    if (CurrentState[cellIndex].IsAlive)
                        imageGraphics.FillRectangle(blackSolidBrush, new Rectangle(cellLocation, cellSize));
                }

                imageGraphics.Dispose();
                blackSolidBrush.Dispose();
            }

            bitmapImage.BeginInit();
            image.Save(imageMemoryStream, ImageFormat.Png);
            imageMemoryStream.Seek(0, SeekOrigin.Begin);
            bitmapImage.StreamSource = imageMemoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}