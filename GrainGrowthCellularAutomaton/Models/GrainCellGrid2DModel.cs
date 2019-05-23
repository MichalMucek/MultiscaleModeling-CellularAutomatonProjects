using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using CellularAutomaton2D;
using CellularAutomaton2D.Models;

namespace GrainGrowthCellularAutomaton.Models
{
    public class GrainCellGrid2DModel : ICellGrid
    {
        public List<List<ICell>> CurrentState { get; private set; }
        public List<List<ICell>> PreviousState { get; private set; }
        public ICellState ZeroState { get; private set; } = new GrainModel();
        private List<ICellState> grains = new List<ICellState>();
        public int ColumnCount { get; private set; }
        public int RowCount { get; private set; }
        public CellNeighborhoodTypeModel NeighborhoodType { get; private set; }
        public BoundaryConditionModel BoundaryCondition { get; private set; }
        public int CellCount => ColumnCount * RowCount;
        public int PopulatedCellsCount { get; private set; } = 0;
        public bool IsFullyPopulated => PopulatedCellsCount == CellCount;

        private static Random random = new Random();

        private MemoryStream imageMemoryStream;
        private BitmapImage bitmapImage;

        public GrainCellGrid2DModel(
            int columnCount,
            int rowCount,
            CellNeighborhoodTypeModel neighborhoodType,
            BoundaryConditionModel boundaryCondition)
        {
            ColumnCount = columnCount;
            RowCount = rowCount;
            NeighborhoodType = neighborhoodType;
            BoundaryCondition = boundaryCondition;

            grains.Add(ZeroState);
            CurrentState = new List<List<ICell>>();
            PreviousState = new List<List<ICell>>();

            CreateNewGrainCellsForCurrentState();
            CreateRowsInPreviousState();
            AddNeighboringCellsToCellsState(CurrentState);
        }

        private void CreateNewGrainCellsForCurrentState()
        {
            for (int row = 0, cellId = 0; row < RowCount; row++)
            {
                CurrentState.Add(new List<ICell>());

                for (int column = 0; column < ColumnCount; column++, cellId++)
                    CurrentState[row].Add(new GrainCellModel(cellId, column, row, (GrainModel)ZeroState));
            }
        }

        private void CreateRowsInPreviousState()
        {
            for (int row = 0; row < RowCount; row++)
                PreviousState.Add(new List<ICell>());
        }

        private void AddNeighboringCellsToCellsState(List<List<ICell>> cellsState)
        {
            GrainCellModel outsideCell = new GrainCellModel((GrainModel)ZeroState);

            for (int row = 0, cellId = 0; row < RowCount; row++)
            {
                for (int column = 0; column < ColumnCount; column++, cellId++)
                {
                    switch (BoundaryCondition)
                    {
                        case BoundaryConditionModel.Absorbing:
                            cellsState[row][column].NeighboringCells = new EightSidedGrainCellNeighborhood
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
                            cellsState[row][column].NeighboringCells = new EightSidedGrainCellNeighborhood
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

        public void Evolve()
        {
            if (!IsFullyPopulated)
            {
                CopyCurrentStateToPrevious();

                foreach (var row in CurrentState)
                    foreach (var evolvingCell in row)
                        if (evolvingCell.State == ZeroState)
                        {
                            evolvingCell.State = GetGrainToExpand((GrainCellModel)PreviousState[evolvingCell.RowNumber][evolvingCell.ColumnNumber]);

                            if (evolvingCell.State != ZeroState)
                                PopulatedCellsCount++;
                        }
            }
        }

        private void CopyCurrentStateToPrevious()
        {
            PreviousState = CurrentState.ConvertAll(
                row => new List<ICell>(row.ConvertAll(grainCell => new GrainCellModel((GrainCellModel)grainCell)))
            );

            AddNeighboringCellsToCellsState(PreviousState);
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
                return (GrainModel)ZeroState;
        }

        private List<GrainModel> GetNonInitialGrainsWithMaxCount(GrainCellModel grainCell)
        {
            var grainsCounts = grainCell.NeighboringCells.StatesCounts;
            var grainsWithMaxCount = new List<GrainModel>();

            int maxCount = 0;

            foreach (var grainCount in grainsCounts)
                if (grainCount.Value > maxCount && grainCount.Key != ZeroState)
                    maxCount = grainCount.Value;

            foreach (var grainCount in grainsCounts)
                if (grainCount.Value == maxCount && grainCount.Key != ZeroState)
                    grainsWithMaxCount.Add((GrainModel)grainCount.Key);

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

                        foreach (var row in CurrentState)
                        {
                            cellPosition.Y = row.First().RowNumber * (cellHeight + lineWidth) + lineWidth;

                            foreach (var grainCell in row)
                            {
                                cellPosition.X = grainCell.ColumnNumber * (cellWidth + lineWidth) + lineWidth;

                                if (grainCell.State != ZeroState)
                                {
                                    grainColorSolidBrush.Color = Color.FromArgb(grainCell.State.Color.R, grainCell.State.Color.G, grainCell.State.Color.B);

                                    imageGraphics.FillRectangle(grainColorSolidBrush, new Rectangle(cellPosition, cellSize));
                                }

                                grainCell.StartPositionOnImage = cellPosition;
                                grainCell.EndPositionOnImage = cellPosition + cellSize;
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

            foreach (var row in CurrentState)
            {
                foreach (var grainCell in row)
                {
                    if (grainCell.State == ZeroState)
                    {
                        if (grainCell.StartPositionOnImage.X <= mousePositionOverImage.X &&
                            grainCell.StartPositionOnImage.Y <= mousePositionOverImage.Y &&
                            grainCell.EndPositionOnImage.X >= mousePositionOverImage.X &&
                            grainCell.EndPositionOnImage.Y >= mousePositionOverImage.Y)
                        {
                            AddNewGrain();
                            grainCell.State = grains.Last();
                            PopulatedCellsCount++;

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
                        CurrentState[(int)row][(int)column].State = grains.Last();
                        PopulatedCellsCount++;
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

                if (CurrentState[randomRow][randomColumn].State == ZeroState)
                {
                    AddNewGrain();
                    CurrentState[randomRow][randomColumn].State = grains.Last();
                    PopulatedCellsCount++;
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

                    if (CurrentState[randomRow][randomColumn].State == ZeroState)
                    {
                        if (IsNoGrainWithinARadius((GrainCellModel)CurrentState[randomRow][randomColumn], radius))
                        {
                            AddNewGrain();
                            CurrentState[randomRow][randomColumn].State = grains.Last();
                            PopulatedCellsCount++;
                            grainCount--;

                            probablyPossibleToPutANucleus = false;
                            failsCount = 0;
                        }
                        else
                            failsCount++;
                    }
                }

                return (uint)PopulatedCellsCount;
            }
            else
            {
                throw new ArgumentException("Radius can't be grater than grid size");
            }
        }

        private bool IsPossibleToPutGrainNucleuseWithRadiusAnywhere(uint radius)
        {
            foreach (var row in CurrentState)
                foreach (var grainCell in row)
                    if (IsNoGrainWithinARadius((GrainCellModel)grainCell, radius))
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
                        cellInSquare = (GrainCellModel)cellInSquare.NeighboringCells.Bottom;

                    for (int movesToNextColumnCount = 0, x = -1 * (int)radius;
                        movesToNextColumnCount < 2 * radius + 1;
                        movesToNextColumnCount++, x++)
                    {
                        double distanceFromCenter = Math.Pow(x, 2) + Math.Pow(y, 2);

                        if (distanceFromCenter <= Math.Pow(radius + 0.5, 2))
                            if (cellInSquare.State != ZeroState)
                                return false;

                        cellInSquare = (GrainCellModel)cellInSquare.NeighboringCells.Right;
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
                upperLeftGrainCell = (GrainCellModel)upperLeftGrainCell.NeighboringCells.TopLeft;
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
                                if (CurrentState[row][column].State != ZeroState)
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
            foreach (var row in CurrentState)
                foreach (var grainCell in row)
                {
                    if (grainCell.State != ZeroState)
                    {
                        grainCell.State = ZeroState;
                        PopulatedCellsCount--;
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
                            grains.Last().SetRandomColor();
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