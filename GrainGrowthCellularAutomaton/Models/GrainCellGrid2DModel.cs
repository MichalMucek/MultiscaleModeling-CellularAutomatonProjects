using ElementaryCellularAutomaton.Models;
using GameOfLife.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace GrainGrowthCellularAutomaton.Models
{
    public class GrainCellGrid2DModel
    {
        private List<List<GrainCellModel>> currentState;
        private List<List<GrainCellModel>> previousState;
        private static GrainModel initialGrain = new GrainModel();
        private List<GrainModel> grains = new List<GrainModel>();
        public int ColumnCount { get; private set; }
        public int RowCount { get; private set; }
        public CellsNeighborhoodTypeModel NeighborhoodType { get; private set; }
        public BoundaryConditionModel BoundaryCondition { get; private set; }
        public int CellCount => ColumnCount * RowCount;
        public int FilledCellsCount { get; private set; } = 0;
        public bool GridIsFull => FilledCellsCount == CellCount;

        private static Random random = new Random();

        private MemoryStream imageMemoryStream;
        private BitmapImage bitmapImage;

        public GrainCellGrid2DModel(
            int columnCount,
            int rowCount,
            CellsNeighborhoodTypeModel neighborhoodType,
            BoundaryConditionModel boundaryCondition)
        {
            ColumnCount = columnCount;
            RowCount = rowCount;
            NeighborhoodType = neighborhoodType;
            BoundaryCondition = boundaryCondition;

            grains.Add(initialGrain);
            currentState = new List<List<GrainCellModel>>();
            previousState = new List<List<GrainCellModel>>();

            CreateNewGrainCellsForCurrentState();
            CreateRowsInPreviousState();
            AddNeighboringCellsToCellsState(currentState);
        }

        private void CreateNewGrainCellsForCurrentState()
        {
            for (int row = 0, cellId = 0; row < RowCount; row++)
            {
                currentState.Add(new List<GrainCellModel>());

                for (int column = 0; column < ColumnCount; column++, cellId++)
                    currentState[row].Add(new GrainCellModel(cellId, column, row, initialGrain));
            }
        }

        private void CreateRowsInPreviousState()
        {
            for (int row = 0; row < RowCount; row++)
                previousState.Add(new List<GrainCellModel>());
        }

        private void AddNeighboringCellsToCellsState(List<List<GrainCellModel>> cellsState)
        {
            GrainCellModel outsideCell = new GrainCellModel(initialGrain);

            for (int row = 0, cellId = 0; row < RowCount; row++)
            {
                for (int column = 0; column < ColumnCount; column++, cellId++)
                {
                    switch (BoundaryCondition)
                    {
                        case BoundaryConditionModel.Absorbing:
                            cellsState[row][column].NeighboringGrainCells = new GrainCellNeighborhood
                            {
                                Top = row == 0 ? outsideCell : cellsState[row - 1][column],
                                TopRight = row == 0 || column == ColumnCount - 1 ? outsideCell : cellsState[row - 1][column + 1],
                                Right = column == ColumnCount - 1 ? outsideCell : cellsState[row][column + 1],
                                BottomRight = row == RowCount - 1 || column == ColumnCount - 1 ? outsideCell : cellsState[row + 1][column + 1],
                                Bottom = row == RowCount - 1 ? outsideCell : cellsState[row + 1][column],
                                BottomLeft = row == RowCount - 1 || column == 0 ? outsideCell : cellsState[row + 1][column - 1],
                                Left = column == 0 ? outsideCell : cellsState[row][column - 1],
                                TopLeft = row == 0 || column == 0 ? outsideCell : cellsState[row - 1][column - 1],
                                Type = NeighborhoodType
                            };

                            break;

                        case BoundaryConditionModel.Periodic:
                            cellsState[row][column].NeighboringGrainCells = new GrainCellNeighborhood
                            {
                                Top = cellsState[row == 0 ? RowCount - 1 : row - 1][column],
                                TopRight = cellsState[row == 0 ? RowCount - 1 : row - 1][column == ColumnCount - 1 ? 0 : column + 1],
                                Right = cellsState[row][column == ColumnCount - 1 ? 0 : column + 1],
                                BottomRight = cellsState[row == RowCount - 1 ? 0 : row + 1][column == ColumnCount - 1 ? 0 : column + 1],
                                Bottom = cellsState[row == RowCount - 1 ? 0 : row + 1][column],
                                BottomLeft = cellsState[row == RowCount - 1 ? 0 : row + 1][column == 0 ? ColumnCount - 1 : column - 1],
                                Left = cellsState[row][column == 0 ? ColumnCount - 1 : column - 1],
                                TopLeft = cellsState[row == 0 ? RowCount - 1 : row - 1][column == 0 ? ColumnCount - 1 : column - 1],
                                Type = NeighborhoodType
                            };

                            break;
                    }
                }
            }
        }

        public void Grow()
        {
            if (!GridIsFull)
            {
                CopyCurrentStateToPrevious();

                foreach (var row in currentState)
                    foreach (var evolvingCell in row)
                        if (evolvingCell.Grain == initialGrain)
                        {
                            evolvingCell.Grain = GetGrainToExpand(previousState[evolvingCell.RowNumber][evolvingCell.ColumnNumber]);

                            if (evolvingCell.Grain != initialGrain)
                                FilledCellsCount++;
                        }
            }
        }

        private void CopyCurrentStateToPrevious()
        {
            previousState = currentState.ConvertAll(
                row => new List<GrainCellModel>(row.ConvertAll(grainCell => new GrainCellModel(grainCell)))
            );

            AddNeighboringCellsToCellsState(previousState);
        }

        private GrainModel GetGrainToExpand(GrainCellModel grainCell)
        {
            var grainsWithMaxCount = GetNonInitialGrainsWithMaxCount(grainCell);

            if (grainsWithMaxCount.Any())
            {
                if (grainsWithMaxCount.Count() == 1)
                    return grainsWithMaxCount.First();
                else
                    return grainsWithMaxCount.ElementAt(random.Next(grainsWithMaxCount.Count()));
            }
            else
                return initialGrain;
        }

        private List<GrainModel> GetNonInitialGrainsWithMaxCount(GrainCellModel grainCell)
        {
            var grainsCounts = grainCell.NeighboringGrainCells.GrainsCounts;
            var grainsWithMaxCount = new List<GrainModel>();

            uint maxCount = 0;

            foreach (var grainCount in grainsCounts)
                if (grainCount.Value > maxCount && grainCount.Key != initialGrain)
                    maxCount = grainCount.Value;

            foreach (var grainCount in grainsCounts)
                if (grainCount.Value == maxCount && grainCount.Key != initialGrain)
                    grainsWithMaxCount.Add(grainCount.Key);

            return grainsWithMaxCount;
        }

        public BitmapImage GetBitmapImage(int cellWidth, int cellHeight, int lineWidth)
        {
            imageMemoryStream?.Dispose();
            imageMemoryStream = new MemoryStream();
            bitmapImage = new BitmapImage();

            int imageWidth = ColumnCount * cellWidth + (ColumnCount + 1) * lineWidth;
            int imageHeight = RowCount * cellHeight + (RowCount + 1) * lineWidth;

            using (var image = new Bitmap(imageWidth, imageHeight))
            {
                using (var imageGraphics = Graphics.FromImage(image))
                {
                    imageGraphics.Clear(Color.White);

                    if (lineWidth > 0)
                    {
                        using (var blackPen = new Pen(Color.Black, lineWidth))
                        {
                            Point lineFirstPoint = new Point();
                            Point lineSecondPoint = new Point();

                            lineFirstPoint.X = 0;
                            lineSecondPoint.X = imageWidth;

                            for (int row = 0; row < RowCount + 1; row++)
                            {
                                lineFirstPoint.Y = row * (cellHeight + lineWidth) + lineWidth / 2;
                                lineSecondPoint.Y = lineFirstPoint.Y;

                                imageGraphics.DrawLine(blackPen, lineFirstPoint, lineSecondPoint);
                            }

                            lineFirstPoint.Y = 0;
                            lineSecondPoint.Y = imageHeight;

                            for (int column = 0; column < ColumnCount + 1; column++)
                            {
                                lineFirstPoint.X = column * (cellWidth + lineWidth) + lineWidth / 2;
                                lineSecondPoint.X = lineFirstPoint.X;

                                imageGraphics.DrawLine(blackPen, lineFirstPoint, lineSecondPoint);
                            }
                        }
                    }

                    using (var grainColorSolidBrush = new SolidBrush(Color.White))
                    {
                        Point cellPosition = new Point();
                        Size cellSize = new Size(cellWidth, cellHeight);

                        foreach (var row in currentState)
                        {
                            cellPosition.Y = row.First().RowNumber * (cellHeight + lineWidth) + lineWidth;

                            foreach (var cell in row)
                            {
                                cellPosition.X = cell.ColumnNumber * (cellWidth + lineWidth) + lineWidth;

                                if (cell.Grain != initialGrain)
                                {
                                    grainColorSolidBrush.Color = Color.FromArgb(cell.Grain.Color.R, cell.Grain.Color.G, cell.Grain.Color.B);

                                    imageGraphics.FillRectangle(grainColorSolidBrush, new Rectangle(cellPosition, cellSize));
                                }

                                cell.StartPositionOnImage = cellPosition;
                                cell.EndPositionOnImage = cellPosition + cellSize;
                            }
                        }
                    }
                }

                bitmapImage.BeginInit();
                image.Save(imageMemoryStream, ImageFormat.Png);
                imageMemoryStream.Seek(0, SeekOrigin.Begin);
                bitmapImage.StreamSource = imageMemoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }

        public bool PutGrainNucleus(Point mousePositionOverImage)
        {
            bool cellIsFound = false;

            foreach (var row in currentState)
            {
                foreach (var cell in row)
                {
                    if (cell.Grain == initialGrain)
                    {
                        if (cell.StartPositionOnImage.X <= mousePositionOverImage.X &&
                                        cell.StartPositionOnImage.Y <= mousePositionOverImage.Y &&
                                        cell.EndPositionOnImage.X >= mousePositionOverImage.X &&
                                        cell.EndPositionOnImage.Y >= mousePositionOverImage.Y)
                        {
                            AddNewGrain();
                            cell.Grain = grains.Last();
                            FilledCellsCount++;

                            cellIsFound = true;
                            break;
                        }
                    }
                }

                if (cellIsFound) break;
            }

            return cellIsFound;
        }

        public void PutGrainNucleusesUniformly(uint countInColumn, uint countInRow)
        {
            ClearGrid();

            double stepInColumn = RowCount / (double)(countInColumn + 1);
            double stepInRow = ColumnCount / (double)(countInRow + 1);

            if (countInColumn >= 1 && countInRow >= 1)
            {
                for (double row = stepInColumn, putedInCoulmnCount = 0;
                    putedInCoulmnCount < countInColumn;
                    row += stepInColumn, putedInCoulmnCount++)
                {
                    for (double column = stepInRow, putedInRowCount = 0;
                        putedInRowCount < countInRow;
                        column += stepInRow, putedInRowCount++)
                    {
                        AddNewGrain();
                        currentState[(int)row][(int)column].Grain = grains.Last();
                        FilledCellsCount++;
                    }
                }
            }
            else
                throw new ArgumentException("countInColumn and countInRow must be grater that or equal to 1");
        }

        public void PutGrainNucleusesRandomly(uint grainCount)
        {
            ClearGrid();

            while (grainCount > 0)
            {
                int randomRow = random.Next(RowCount);
                int randomColumn = random.Next(ColumnCount);

                if (currentState[randomRow][randomColumn].Grain == initialGrain)
                {
                    AddNewGrain();
                    currentState[randomRow][randomColumn].Grain = grains.Last();
                    FilledCellsCount++;
                    grainCount--;
                }
            }
        }

        public uint PutGrainNucleusesRandomlyWithRadius(uint grainCount, uint radius)
        {
            if (radius <= RowCount / 2 && radius <= ColumnCount / 2)
            {
                ClearGrid();

                int failsCount = 0;
                bool probablyPossibleToPutANucleus = true;

                while (grainCount > 0)
                {
                    if (failsCount > 10 && !probablyPossibleToPutANucleus)
                    {
                        if (IsPossibleToPutGrainNucleuseWithRadiusAnywhere(radius))
                            probablyPossibleToPutANucleus = true;
                        else
                            break;
                    }

                    int randomRow = random.Next(RowCount);
                    int randomColumn = random.Next(ColumnCount);

                    if (currentState[randomRow][randomColumn].Grain == initialGrain)
                    {
                        if (IsNoGrainWithinARadius(currentState[randomRow][randomColumn], radius))
                        {
                            AddNewGrain();
                            currentState[randomRow][randomColumn].Grain = grains.Last();
                            FilledCellsCount++;
                            grainCount--;

                            probablyPossibleToPutANucleus = false;
                            failsCount = 0;
                        }
                        else
                            failsCount++;
                    }
                }

                return (uint)FilledCellsCount;
            }
            else
            {
                throw new ArgumentException("Radius can't be grater than grid size");
            }
        }

        private bool IsPossibleToPutGrainNucleuseWithRadiusAnywhere(uint radius)
        {
            foreach (var row in currentState)
                foreach (var grainCell in row)
                    if (IsNoGrainWithinARadius(grainCell, radius))
                        return true;

            return false;
        }

        private bool IsNoGrainWithinARadius(GrainCellModel centerCell, uint radius)
        {
            if (BoundaryCondition == BoundaryConditionModel.Periodic)
                return IsNoGrainWithinARadiusForPeriodicBc(centerCell, radius);
            else
                return IsNoGrainWithinARadiusForAbsorbingBc(centerCell, radius);
        }

        private bool IsNoGrainWithinARadiusForPeriodicBc(GrainCellModel centerCell, uint radius)
        {
            if (radius <= RowCount / 2 && radius <= ColumnCount / 2)
            {
                GrainCellModel upperLeftGrainCell;

                SetUpperLeftCell(centerCell, out upperLeftGrainCell, radius);

                for (int movesToNextRowCount = 0, y = (int)radius;
                    movesToNextRowCount < 2 * radius + 1;
                    movesToNextRowCount++, y--)
                {
                    var cellInSquare = upperLeftGrainCell;

                    for (int i = 0; i < movesToNextRowCount; i++)
                        cellInSquare = cellInSquare.NeighboringGrainCells.Bottom;

                    for (int movesToNextColumnCount = 0, x = -1 * (int)radius;
                        movesToNextColumnCount < 2 * radius + 1;
                        movesToNextColumnCount++, x++)
                    {
                        double distanceFromCenter = Math.Pow(x, 2) + Math.Pow(y, 2);

                        if (distanceFromCenter <= Math.Pow(radius + 0.5, 2))
                            if (cellInSquare.Grain != initialGrain)
                                return false;

                        cellInSquare = cellInSquare.NeighboringGrainCells.Right;
                    }
                }

                return true;
            }
            else
            {
                throw new ArgumentException("Radius can't be grater than grid size");
            }
        }

        private void SetUpperLeftCell(GrainCellModel centerCell, out GrainCellModel upperLeftGrainCell, uint radius)
        {
            upperLeftGrainCell = centerCell;

            for (int movesToMakeCount = 0; movesToMakeCount < radius; movesToMakeCount++)
                upperLeftGrainCell = upperLeftGrainCell.NeighboringGrainCells.TopLeft;
        }

        private bool IsNoGrainWithinARadiusForAbsorbingBc(GrainCellModel centerCell, uint radius)
        {
            int upperLeftCellRow = centerCell.RowNumber - (int)radius;
            int upperLeftCellColumn = centerCell.ColumnNumber - (int)radius;

            for (int row = upperLeftCellRow, y = (int)radius;
                    row <= upperLeftCellRow + 2 * radius;
                    row++, y--)
            {
                if (row >= 0 && row < RowCount)
                {
                    for (int column = upperLeftCellColumn, x = -1 * (int)radius;
                        column <= upperLeftCellColumn + 2 * radius;
                        column++, x++)
                    {
                        if (column >= 0 && column < ColumnCount)
                        {
                            double distanceFromCenter = Math.Pow(x, 2) + Math.Pow(y, 2);

                            if (distanceFromCenter <= Math.Pow(radius + 0.5, 2))
                                if (currentState[row][column].Grain != initialGrain)
                                    return false;
                        }
                    }
                }
            }

            return true;
        }

        private void ClearGrid()
        {
            SetGridToInitialState();
            RemoveAllGrainsExceptInitial();
        }

        private void SetGridToInitialState()
        {
            foreach (var row in currentState)
                foreach (var grainCell in row)
                {
                    if (grainCell.Grain != initialGrain)
                    {
                        grainCell.Grain = initialGrain;
                        FilledCellsCount--;
                    }
                }
        }

        private void RemoveAllGrainsExceptInitial()
        {
            while (grains.Count != 1)
                grains.RemoveAt(grains.Count - 1);
        }

        private void AddNewGrain()
        {
            grains.Add(new GrainModel(grains.Count));

            SetUniqueColorForNewGrain();
        }

        private void SetUniqueColorForNewGrain()
        {
            bool colorRepeats = true;

            while (colorRepeats)
            {
                foreach (var grain in grains)
                {
                    if (grain != grains.Last())
                    {
                        if (grain.Color == grains.Last().Color)
                        {
                            grains.Last().NewColor();
                            break;
                        }
                    }
                    else
                        colorRepeats = false;
                }
            }
        }
    }
}