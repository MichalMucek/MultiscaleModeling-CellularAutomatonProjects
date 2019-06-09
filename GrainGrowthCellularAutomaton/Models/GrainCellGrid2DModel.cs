using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
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
        public int CellNeighborhoodRadius { get; private set; } = 0;
        public int CellCount => ColumnCount * RowCount;
        public int PopulatedCellsCount { get; private set; } = 0;
        public bool IsFullyPopulated => PopulatedCellsCount == CellCount;

        [ThreadStatic]
        private static Random staticRandom;

        private MemoryStream imageMemoryStream;
        private BitmapImage bitmapImage;

        public GrainCellGrid2DModel(
            int columnCount,
            int rowCount,
            CellNeighborhoodTypeModel neighborhoodType,
            BoundaryConditionModel boundaryCondition,
            int cellNeighborhoodRadius = 0)
        {
            CreateNewStaticRandom();

            ColumnCount = columnCount;
            RowCount = rowCount;
            NeighborhoodType = neighborhoodType;
            BoundaryCondition = boundaryCondition;
            CellNeighborhoodRadius = cellNeighborhoodRadius;

            grains.Add(ZeroState);
            CurrentState = new List<List<ICell>>();
            PreviousState = new List<List<ICell>>();

            CreateNewGrainCellsForCurrentState();
            CreateRowsInPreviousState();
            AddNeighboringCellsToCellsState(CurrentState);
        }

        private void CreateNewStaticRandom()
        {
            if (staticRandom == null)
                staticRandom = new Random();
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

        private void AddNeighboringCellsToCellsState(List<List<ICell>> cellsState, bool canSkipNonZeroState = true)
        {
            GrainCellModel outsideCell = new GrainCellModel((GrainModel)ZeroState);

            for (int row = 0, cellId = 0; row < RowCount; row++)
            {
                for (int column = 0; column < ColumnCount; column++, cellId++)
                {
                    var grainCell = (GrainCellModel)cellsState[row][column];

                    if (cellsState[row][column].State == ZeroState || !canSkipNonZeroState)
                    {
                        switch (NeighborhoodType)
                        {
                            case CellNeighborhoodTypeModel.VonNeumann:
                            case CellNeighborhoodTypeModel.Moore:
                            case CellNeighborhoodTypeModel.RandomPentagonal:
                            case CellNeighborhoodTypeModel.LeftHexagonal:
                            case CellNeighborhoodTypeModel.RightHexagonal:
                            case CellNeighborhoodTypeModel.RandomHexagonal:
                                switch (BoundaryCondition)
                                {
                                    case BoundaryConditionModel.Absorbing:
                                        grainCell.NeighboringCells = GetEightSidedGrainCellNeighborhoodForAbsorbingBC(outsideCell, cellsState, row, column, NeighborhoodType);
                                        grainCell.VonNeumannCellNeighborhood = GetEightSidedGrainCellNeighborhoodForAbsorbingBC(outsideCell, cellsState, row, column, CellNeighborhoodTypeModel.VonNeumann);
                                        break;

                                    case BoundaryConditionModel.Periodic:
                                        grainCell.NeighboringCells = GetEightSidedGrainCellNeighborhoodForPeriodicBC(cellsState, row, column, NeighborhoodType);
                                        grainCell.VonNeumannCellNeighborhood = GetEightSidedGrainCellNeighborhoodForPeriodicBC(cellsState, row, column, CellNeighborhoodTypeModel.VonNeumann);
                                        break;
                                }
                                break;

                            case CellNeighborhoodTypeModel.Radial:
                                switch (BoundaryCondition)
                                {
                                    case BoundaryConditionModel.Absorbing:
                                        cellsState[row][column].NeighboringCells = new RadialGrainCellNeighborhood(
                                            GetGrainCellsWithinARadiusForAbsorbingBc(
                                                (GrainCellModel)cellsState[row][column],
                                                CellNeighborhoodRadius,
                                                cellsState),
                                            NeighborhoodType);

                                        grainCell.VonNeumannCellNeighborhood = GetEightSidedGrainCellNeighborhoodForAbsorbingBC(outsideCell, cellsState, row, column, CellNeighborhoodTypeModel.VonNeumann);
                                        break;

                                    case BoundaryConditionModel.Periodic:
                                        cellsState[row][column].NeighboringCells = new RadialGrainCellNeighborhood(
                                            GetGrainCellsWithinARadiusForPeriodicBc(
                                                (GrainCellModel)cellsState[row][column],
                                                CellNeighborhoodRadius,
                                                cellsState),
                                            NeighborhoodType);

                                        grainCell.VonNeumannCellNeighborhood = GetEightSidedGrainCellNeighborhoodForPeriodicBC(cellsState, row, column, CellNeighborhoodTypeModel.VonNeumann);
                                        break;
                                }
                                break;

                            case CellNeighborhoodTypeModel.RadialWithCenterOfMass:
                                switch (BoundaryCondition)
                                {
                                    case BoundaryConditionModel.Absorbing:
                                        cellsState[row][column].NeighboringCells = new RadialGrainCellNeighborhood(
                                            GetGrainCellsWithCenterOfMassWithinARadiusForAbsorbingBc(
                                                (GrainCellModel)cellsState[row][column],
                                                CellNeighborhoodRadius,
                                                cellsState),
                                            NeighborhoodType);

                                        grainCell.VonNeumannCellNeighborhood = GetEightSidedGrainCellNeighborhoodForAbsorbingBC(outsideCell, cellsState, row, column, CellNeighborhoodTypeModel.VonNeumann);
                                        break;

                                    case BoundaryConditionModel.Periodic:
                                        cellsState[row][column].NeighboringCells = new RadialGrainCellNeighborhood(
                                            GetGrainCellsWithCenterOfMassWithinARadiusForPeriodicBc(
                                                (GrainCellModel)cellsState[row][column],
                                                CellNeighborhoodRadius,
                                                cellsState),
                                            NeighborhoodType);

                                        grainCell.VonNeumannCellNeighborhood = GetEightSidedGrainCellNeighborhoodForPeriodicBC(cellsState, row, column, CellNeighborhoodTypeModel.VonNeumann);
                                        break;
                                }
                                break;
                        }
                    }
                }
            }
        }

        private EightSidedGrainCellNeighborhood GetEightSidedGrainCellNeighborhoodForAbsorbingBC(GrainCellModel outsideCell, List<List<ICell>> cellsState, int row, int column, CellNeighborhoodTypeModel neighborhoodType)
        {
            return new EightSidedGrainCellNeighborhood
            {
                Top = row == 0 ? outsideCell : cellsState[row - 1][column],
                TopRight = row == 0 || column == ColumnCount - 1 ? outsideCell : cellsState[row - 1][column + 1],
                Right = column == ColumnCount - 1 ? outsideCell : cellsState[row][column + 1],
                BottomRight = row == RowCount - 1 || column == ColumnCount - 1 ? outsideCell : cellsState[row + 1][column + 1],
                Bottom = row == RowCount - 1 ? outsideCell : cellsState[row + 1][column],
                BottomLeft = row == RowCount - 1 || column == 0 ? outsideCell : cellsState[row + 1][column - 1],
                Left = column == 0 ? outsideCell : cellsState[row][column - 1],
                TopLeft = row == 0 || column == 0 ? outsideCell : cellsState[row - 1][column - 1],
                Type = neighborhoodType
            };
        }

        private EightSidedGrainCellNeighborhood GetEightSidedGrainCellNeighborhoodForPeriodicBC(List<List<ICell>> cellsState, int row, int column, CellNeighborhoodTypeModel neighborhoodType)
        {
            return new EightSidedGrainCellNeighborhood
            {
                Top = cellsState[row == 0 ? RowCount - 1 : row - 1][column],
                TopRight = cellsState[row == 0 ? RowCount - 1 : row - 1][column == ColumnCount - 1 ? 0 : column + 1],
                Right = cellsState[row][column == ColumnCount - 1 ? 0 : column + 1],
                BottomRight = cellsState[row == RowCount - 1 ? 0 : row + 1][column == ColumnCount - 1 ? 0 : column + 1],
                Bottom = cellsState[row == RowCount - 1 ? 0 : row + 1][column],
                BottomLeft = cellsState[row == RowCount - 1 ? 0 : row + 1][column == 0 ? ColumnCount - 1 : column - 1],
                Left = cellsState[row][column == 0 ? ColumnCount - 1 : column - 1],
                TopLeft = cellsState[row == 0 ? RowCount - 1 : row - 1][column == 0 ? ColumnCount - 1 : column - 1],
                Type = neighborhoodType
            };
        }

        private List<GrainCellModel> GetGrainCellsWithinARadiusForPeriodicBc(GrainCellModel centerGrainCell, int radius, List<List<ICell>> cellsState)
        {
            if (radius <= 0)
                throw new ArgumentException("Radius must be grater than 0");

            if (radius > RowCount / 2 || radius > ColumnCount / 2)
                throw new ArgumentException("Radius must be smaller than grid size");

            List<GrainCellModel> grainCellsWithinARadius = new List<GrainCellModel>();

            var upperLeftGrainCell = GetUpperLeftGrainCellFromSquarePerimiter(centerGrainCell, radius);

            for (int movesMadeToNextRowCount = 0, row = upperLeftGrainCell.RowNumber, y = radius;
                movesMadeToNextRowCount < 2 * radius + 1;
                movesMadeToNextRowCount++, row++, y--)
            {
                if (row >= RowCount)
                    row = 0;

                for (int movesMadeToNextColumnCount = 0, column = upperLeftGrainCell.ColumnNumber, x = -1 * radius;
                    movesMadeToNextColumnCount < 2 * radius + 1;
                    movesMadeToNextColumnCount++, column++, x++)
                {
                    if (column >= ColumnCount)
                        column = 0;

                    var grainCellInSquare = (GrainCellModel)cellsState[row][column];

                    if (grainCellInSquare != centerGrainCell)
                    {
                        double distanceFromCenter = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

                        if (distanceFromCenter <= radius + 0.5)
                            grainCellsWithinARadius.Add(grainCellInSquare);
                    }
                }
            }

            return grainCellsWithinARadius;
        }

        private ICell GetUpperLeftGrainCellFromSquarePerimiter(GrainCellModel centerCell, int radius)
        {
            if (radius <= 0)
                throw new ArgumentException("Radius must be grater than 0");

            int upperLeftGrainCellRow = centerCell.RowNumber - radius;
            int upperLeftGrainCellColumn = centerCell.ColumnNumber - radius;

            if (upperLeftGrainCellRow < 0)
                upperLeftGrainCellRow += RowCount;

            if (upperLeftGrainCellColumn < 0)
                upperLeftGrainCellColumn += ColumnCount;

            return CurrentState[upperLeftGrainCellRow][upperLeftGrainCellColumn];
        }

        private List<GrainCellModel> GetGrainCellsWithinARadiusForAbsorbingBc(GrainCellModel centerGrainCell, int radius, List<List<ICell>> cellsState)
        {
            if (radius <= 0)
                throw new ArgumentException("Radius must be grater than 0");

            if (radius > RowCount / 2 || radius > ColumnCount / 2)
                throw new ArgumentException("Radius must be smaller than grid size");

            List<GrainCellModel> grainCellsWithinARadius = new List<GrainCellModel>();
            GrainCellModel outsideGrainCell = new GrainCellModel((GrainModel)ZeroState);
            GrainCellModel grainCellInSquare;

            int upperLeftGrainCellRow = centerGrainCell.RowNumber - radius;
            int upperLeftGrainCellColumn = centerGrainCell.ColumnNumber - radius;

            for (int row = upperLeftGrainCellRow, y = radius;
                    row <= upperLeftGrainCellRow + 2 * radius;
                    row++, y--)
            {
                if (row >= -1 && row <= RowCount)
                {
                    for (int column = upperLeftGrainCellColumn, x = -1 * radius;
                        column <= upperLeftGrainCellColumn + 2 * radius;
                        column++, x++)
                    {
                        if (column >= -1 && column <= ColumnCount)
                        {
                            if (row == -1 || row == RowCount || column == -1 || column == ColumnCount)
                                grainCellInSquare = outsideGrainCell;
                            else
                                grainCellInSquare = (GrainCellModel)cellsState[row][column];

                            if (grainCellInSquare != centerGrainCell)
                            {
                                double distanceFromCenter = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

                                if (distanceFromCenter <= radius + 0.5)
                                    grainCellsWithinARadius.Add(grainCellInSquare);
                            }
                        }
                    }
                }
            }

            return grainCellsWithinARadius;
        }

        private List<GrainCellModel> GetGrainCellsWithCenterOfMassWithinARadiusForPeriodicBc(GrainCellModel centerGrainCell, int radius, List<List<ICell>> cellsState)
        {
            if (radius <= 0)
                throw new ArgumentException("Radius must be grater than 0");

            if (radius > RowCount / 2 || radius > ColumnCount / 2)
                throw new ArgumentException("Radius must be smaller than grid size");

            List<GrainCellModel> grainCellsWithinARadius = new List<GrainCellModel>();

            var upperLeftGrainCell = GetUpperLeftGrainCellFromSquarePerimiter(centerGrainCell, radius);

            double centerGrainCellLocalX = centerGrainCell.CenterOfMass.X + radius + centerGrainCell.Width / 2.0;
            double centerGrainCellLocalY = centerGrainCell.CenterOfMass.Y - radius - centerGrainCell.Height / 2.0;

            for (int movesMadeToNextRowCount = 0, globalRow = upperLeftGrainCell.RowNumber, localRow = 0;
                movesMadeToNextRowCount < 2 * radius + 1;
                movesMadeToNextRowCount++, globalRow++, localRow++)
            {
                if (globalRow >= RowCount)
                    globalRow = 0;

                for (int movesMadeToNextColumnCount = 0, globalColumn = upperLeftGrainCell.ColumnNumber, localColumn = 0;
                    movesMadeToNextColumnCount < 2 * radius + 1;
                    movesMadeToNextColumnCount++, globalColumn++, localColumn++)
                {
                    if (globalColumn >= ColumnCount)
                        globalColumn = 0;

                    var grainCellInSquare = (GrainCellModel)cellsState[globalRow][globalColumn];

                    if (grainCellInSquare != centerGrainCell)
                    {
                        double grainCellInSquareLocalX = grainCellInSquare.CenterOfMass.X + localColumn + grainCellInSquare.Width / 2.0;
                        double grainCellInSquareLocalY = grainCellInSquare.CenterOfMass.Y - localRow - grainCellInSquare.Height / 2.0;

                        double distanceBetweenCentersOfMass = Math.Sqrt(
                            Math.Pow(grainCellInSquareLocalX - centerGrainCellLocalX, 2) +
                            Math.Pow(grainCellInSquareLocalY - centerGrainCellLocalY, 2)
                        );

                        if (distanceBetweenCentersOfMass <= radius)
                            grainCellsWithinARadius.Add(grainCellInSquare);
                    }
                }
            }

            return grainCellsWithinARadius;
        }

        private List<GrainCellModel> GetGrainCellsWithCenterOfMassWithinARadiusForAbsorbingBc(GrainCellModel centerGrainCell, int radius, List<List<ICell>> cellsState)
        {
            if (radius <= 0)
                throw new ArgumentException("Radius must be grater than 0");

            if (radius > RowCount / 2 || radius > ColumnCount / 2)
                throw new ArgumentException("Radius must be smaller than grid size");

            List<GrainCellModel> grainCellsWithinARadius = new List<GrainCellModel>();
            GrainCellModel outsideGrainCell = new GrainCellModel((GrainModel)ZeroState);
            GrainCellModel grainCellInSquare;

            int upperLeftGrainCellRow = centerGrainCell.RowNumber - radius;
            int upperLeftGrainCellColumn = centerGrainCell.ColumnNumber - radius;

            for (int row = upperLeftGrainCellRow;
                    row <= upperLeftGrainCellRow + 2 * radius;
                    row++)
            {
                if (row >= -1 && row <= RowCount)
                {
                    for (int column = upperLeftGrainCellColumn;
                        column <= upperLeftGrainCellColumn + 2 * radius;
                        column++)
                    {
                        if (column >= -1 && column <= ColumnCount)
                        {
                            if (row == -1 || row == RowCount || column == -1 || column == ColumnCount)
                                grainCellInSquare = outsideGrainCell;
                            else
                                grainCellInSquare = (GrainCellModel)cellsState[row][column];

                            if (grainCellInSquare != centerGrainCell)
                            {
                                double distanceBetweenCentersOfMass = Math.Sqrt(
                                    Math.Pow(grainCellInSquare.GlobalCenterOfMass.X - centerGrainCell.GlobalCenterOfMass.X, 2) +
                                    Math.Pow(grainCellInSquare.GlobalCenterOfMass.Y - centerGrainCell.GlobalCenterOfMass.Y, 2)
                                );

                                if (distanceBetweenCentersOfMass <= radius)
                                    grainCellsWithinARadius.Add(grainCellInSquare);
                            }
                        }
                    }
                }
            }

            return grainCellsWithinARadius;
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

        private void CopyCurrentStateToPrevious(bool canSkipNonZeroStateWhileAddingNeighboringCells = false)
        {
            PreviousState = CurrentState.ConvertAll(
                row => new List<ICell>(row.ConvertAll(grainCell => new GrainCellModel((GrainCellModel)grainCell)))
            );

            AddNeighboringCellsToCellsState(PreviousState, canSkipNonZeroStateWhileAddingNeighboringCells);
        }

        private GrainModel GetGrainToExpand(GrainCellModel grainCell)
        {
            CreateNewStaticRandom();

            var grainsWithMaxCount = GetNonInitialGrainsWithMaxCount(grainCell);

            if (grainsWithMaxCount.Any())
            {
                if (grainsWithMaxCount.Count() == 1)
                    return grainsWithMaxCount.First();
                else
                    return grainsWithMaxCount.ElementAt(staticRandom.Next(grainsWithMaxCount.Count()));
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
            ResetImageMemoryStream();
            bitmapImage = new BitmapImage();

            int imageWidth, imageHeight;
            SetImageWidthAndHeight(out imageWidth, out imageHeight, cellWidth, cellHeight, lineWidth);

            using (var image = new Bitmap(imageWidth, imageHeight))
            {
                using (var imageGraphics = Graphics.FromImage(image))
                {
                    imageGraphics.Clear(Color.FromArgb(ZeroState.Color.R, ZeroState.Color.G, ZeroState.Color.B));

                    DrawBlackOrWhiteLinesGridOnImage(imageWidth, imageHeight, lineWidth, cellWidth, cellHeight, imageGraphics);

                    using (var grainColorSolidBrush = new SolidBrush(Color.White))
                    {
                        Point cellPosition = new Point();
                        Size cellSize = new Size(cellWidth, cellHeight);

                        foreach (var row in CurrentState)
                        {
                            cellPosition.Y = row.First().RowNumber * (cellHeight + lineWidth) + lineWidth;

                            foreach (GrainCellModel grainCell in row)
                            {
                                cellPosition.X = grainCell.ColumnNumber * (cellWidth + lineWidth) + lineWidth;

                                if (grainCell.State != ZeroState)
                                {
                                    grainColorSolidBrush.Color = Color.FromArgb(grainCell.State.Color.R, grainCell.State.Color.G, grainCell.State.Color.B);

                                    imageGraphics.FillRectangle(grainColorSolidBrush, new Rectangle(cellPosition, cellSize));
                                }

                                if (HasGrainCellPositionOnImageChanged(grainCell, cellPosition, cellSize))
                                {
                                    grainCell.StartPositionOnImage = new System.Windows.Point(cellPosition.X, cellPosition.Y);
                                    grainCell.EndPositionOnImage = grainCell.StartPositionOnImage + new System.Windows.Vector(cellSize.Width, cellSize.Height);
                                }
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

        public BitmapImage GetEnergyBitmapImage(int cellWidth, int cellHeight, int lineWidth)
        {
            ResetImageMemoryStream();
            bitmapImage = new BitmapImage();

            int imageWidth, imageHeight;
            SetImageWidthAndHeight(out imageWidth, out imageHeight, cellWidth, cellHeight, lineWidth);

            using (var image = new Bitmap(imageWidth, imageHeight))
            {
                using (var imageGraphics = Graphics.FromImage(image))
                {
                    imageGraphics.Clear(Color.FromArgb(ZeroState.Color.R, ZeroState.Color.G, ZeroState.Color.B));

                    DrawBlackOrWhiteLinesGridOnImage(imageWidth, imageHeight, lineWidth, cellWidth, cellHeight, imageGraphics, true);

                    using (var grainCellEnergyLevelColorSolidBrush = new SolidBrush(Color.Yellow))
                    {
                        Point cellPosition = new Point();
                        Size cellSize = new Size(cellWidth, cellHeight);

                        foreach (var row in CurrentState)
                        {
                            cellPosition.Y = row.First().RowNumber * (cellHeight + lineWidth) + lineWidth;

                            foreach (GrainCellModel grainCell in row)
                            {
                                cellPosition.X = grainCell.ColumnNumber * (cellWidth + lineWidth) + lineWidth;

                                double grainCellEnergyPercentage = grainCell.Energy / grainCell.NeighboringCells.Count;

                                grainCellEnergyLevelColorSolidBrush.Color = Color.FromArgb((int)(255 * grainCellEnergyPercentage), (int)(255 * grainCellEnergyPercentage), 0);
                                imageGraphics.FillRectangle(grainCellEnergyLevelColorSolidBrush, new Rectangle(cellPosition, cellSize));

                                if (HasGrainCellPositionOnImageChanged(grainCell, cellPosition, cellSize))
                                {
                                    grainCell.StartPositionOnImage = new System.Windows.Point(cellPosition.X, cellPosition.Y);
                                    grainCell.EndPositionOnImage = grainCell.StartPositionOnImage + new System.Windows.Vector(cellSize.Width, cellSize.Height);
                                }
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

        public BitmapImage GetDislocationDensityBitmapImage(int cellWidth, int cellHeight, int lineWidth)
        {
            ResetImageMemoryStream();
            bitmapImage = new BitmapImage();

            int imageWidth, imageHeight;
            SetImageWidthAndHeight(out imageWidth, out imageHeight, cellWidth, cellHeight, lineWidth);

            double maximumDislocationDensity = GetMaximumDislocationDensity();

            using (var image = new Bitmap(imageWidth, imageHeight))
            {
                using (var imageGraphics = Graphics.FromImage(image))
                {
                    imageGraphics.Clear(Color.FromArgb(ZeroState.Color.R, ZeroState.Color.G, ZeroState.Color.B));

                    DrawBlackOrWhiteLinesGridOnImage(imageWidth, imageHeight, lineWidth, cellWidth, cellHeight, imageGraphics);

                    using (var grainCellEnergyLevelColorSolidBrush = new SolidBrush(Color.Yellow))
                    {
                        Point cellPosition = new Point();
                        Size cellSize = new Size(cellWidth, cellHeight);

                        foreach (var row in CurrentState)
                        {
                            cellPosition.Y = row.First().RowNumber * (cellHeight + lineWidth) + lineWidth;

                            foreach (GrainCellModel grainCell in row)
                            {
                                cellPosition.X = grainCell.ColumnNumber * (cellWidth + lineWidth) + lineWidth;

                                double grainCellDislocationDensityPercentage = maximumDislocationDensity != 0.0 ? grainCell.DislocationDensity / maximumDislocationDensity : 0.0;

                                //Console.WriteLine($"R: {grainCell.RowNumber} C: {grainCell.ColumnNumber} DD: {grainCell.DislocationDensity} ({grainCellDislocationDensityPercentage * 100}%) MAX: {maximumDislocationDensity}");

                                if (grainCellDislocationDensityPercentage <= 0.5)
                                    grainCellEnergyLevelColorSolidBrush.Color = Color.FromArgb((int)(255 * grainCellDislocationDensityPercentage * 2), 255, 0);
                                else
                                    grainCellEnergyLevelColorSolidBrush.Color = Color.FromArgb(255, (int)(255 * (1.0 - grainCellDislocationDensityPercentage) * 2), 0);

                                imageGraphics.FillRectangle(grainCellEnergyLevelColorSolidBrush, new Rectangle(cellPosition, cellSize));

                                if (HasGrainCellPositionOnImageChanged(grainCell, cellPosition, cellSize))
                                {
                                    grainCell.StartPositionOnImage = new System.Windows.Point(cellPosition.X, cellPosition.Y);
                                    grainCell.EndPositionOnImage = grainCell.StartPositionOnImage + new System.Windows.Vector(cellSize.Width, cellSize.Height);
                                }
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

        private double GetMaximumDislocationDensity()
        {
            double maximumDislocationDensity = 0;

            foreach (var row in CurrentState)
                foreach (GrainCellModel grainCell in row)
                    if (grainCell.DislocationDensity > maximumDislocationDensity)
                        maximumDislocationDensity = grainCell.DislocationDensity;

            return maximumDislocationDensity;
        }

        private void ResetImageMemoryStream()
        {
            imageMemoryStream?.Dispose();
            imageMemoryStream = new MemoryStream();
        }

        private void SetImageWidthAndHeight(out int imageWidth, out int imageHeight, int cellWidth, int cellHeight, int lineWidth)
        {
            imageWidth = ColumnCount * cellWidth + (ColumnCount + 1) * lineWidth;
            imageHeight = RowCount * cellHeight + (RowCount + 1) * lineWidth;
        }

        private void DrawBlackOrWhiteLinesGridOnImage(int imageWidth, int imageHeight, int lineWidth, int cellWidth, int cellHeight, Graphics graphics, bool whiteLines = false)
        {
            if (lineWidth > 0)
            {
                using (var blackPen = new Pen(whiteLines ? Color.White : Color.Black, lineWidth))
                {
                    Point lineFirstPoint = new Point();
                    Point lineSecondPoint = new Point();

                    lineFirstPoint.X = 0;
                    lineSecondPoint.X = imageWidth;

                    for (int row = 0; row < RowCount + 1; row++)
                    {
                        lineFirstPoint.Y = row * (cellHeight + lineWidth) + lineWidth / 2;
                        lineSecondPoint.Y = lineFirstPoint.Y;

                        graphics.DrawLine(blackPen, lineFirstPoint, lineSecondPoint);
                    }

                    lineFirstPoint.Y = 0;
                    lineSecondPoint.Y = imageHeight;

                    for (int column = 0; column < ColumnCount + 1; column++)
                    {
                        lineFirstPoint.X = column * (cellWidth + lineWidth) + lineWidth / 2;
                        lineSecondPoint.X = lineFirstPoint.X;

                        graphics.DrawLine(blackPen, lineFirstPoint, lineSecondPoint);
                    }
                }
            }
        }

        private bool HasGrainCellPositionOnImageChanged(ICell grainCell, Point cellPosition, Size cellSize)
        {
            return grainCell.StartPositionOnImage.X != cellPosition.X ||
                    grainCell.StartPositionOnImage.Y != cellPosition.Y ||
                    grainCell.EndPositionOnImage.X != cellPosition.X + cellSize.Width ||
                    grainCell.EndPositionOnImage.Y != cellPosition.Y + cellSize.Height;
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

        public void PutGrainNucleusesUniformly(int countInColumn, int countInRow)
        {
            Clear();

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

        public void PutGrainNucleusesRandomly(int grainCount)
        {
            CreateNewStaticRandom();
            Clear();

            while (grainCount > 0)
            {
                int randomRow = staticRandom.Next(RowCount);
                int randomColumn = staticRandom.Next(ColumnCount);

                if (CurrentState[randomRow][randomColumn].State == ZeroState)
                {
                    AddNewGrain();
                    CurrentState[randomRow][randomColumn].State = grains.Last();
                    PopulatedCellsCount++;
                    grainCount--;
                }
            }
        }

        public int PutGrainNucleusesRandomlyWithRadius(int grainCount, int radius)
        {
            if (radius > 0)
            {
                CreateNewStaticRandom();
                Clear();

                int failsCount = 0;
                bool probablyPossibleToPutANucleus = true;

                while (grainCount > 0)
                {
                    if (failsCount > 10 && !probablyPossibleToPutANucleus)
                    {
                        if (IsPossibleToPutGrainNucleusWithRadiusAnywhere(radius))
                            probablyPossibleToPutANucleus = true;
                        else
                            break;
                    }

                    int randomRow = staticRandom.Next(RowCount);
                    int randomColumn = staticRandom.Next(ColumnCount);

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

                return PopulatedCellsCount;
            }
            else
                throw new ArgumentException("Radius must be grater than 0");
        }

        private bool IsPossibleToPutGrainNucleusWithRadiusAnywhere(int radius)
        {
            if (radius > 0)
            {
                foreach (var row in CurrentState)
                    foreach (var grainCell in row)
                        if (IsNoGrainWithinARadius((GrainCellModel)grainCell, radius))
                            return true;

                return false;
            }
            else
                throw new ArgumentException("Radius must be grater than 0");
        }

        private bool IsNoGrainWithinARadius(GrainCellModel centerCell, int radius)
        {
            if (radius <= 0)
                throw new ArgumentException("Radius must be grater than 0");

            if (BoundaryCondition == BoundaryConditionModel.Periodic)
                return IsNoGrainWithinARadiusForPeriodicBc(centerCell, radius);
            else
                return IsNoGrainWithinARadiusForAbsorbingBc(centerCell, radius);
        }

        private bool IsNoGrainWithinARadiusForPeriodicBc(GrainCellModel centerCell, int radius)
        {
            if (radius <= 0)
                throw new ArgumentException("Radius must be grater than 0");

            var upperLeftGrainCell = GetUpperLeftGrainCellFromSquarePerimiter(centerCell, radius);

            for (int rowInSquarePerimiter = 0, y = radius;
                rowInSquarePerimiter < 2 * radius + 1;
                rowInSquarePerimiter++, y--)
            {
                int rowInGrid = upperLeftGrainCell.RowNumber + rowInSquarePerimiter;

                if (rowInGrid < 0)
                    rowInGrid += RowCount;

                if (rowInGrid >= RowCount)
                    rowInGrid -= RowCount;

                for (int columnInSquarePerimiter = 0, x = -1 * radius;
                    columnInSquarePerimiter < 2 * radius + 1;
                    columnInSquarePerimiter++, x++)
                {
                    int columnInGrid = upperLeftGrainCell.ColumnNumber + columnInSquarePerimiter;

                    if (columnInGrid < 0)
                        columnInGrid += ColumnCount;

                    if (columnInGrid >= ColumnCount)
                        columnInGrid -= ColumnCount;

                    var cellInSquare = CurrentState[rowInGrid][columnInGrid];

                    double distanceFromCenter = Math.Pow(x, 2) + Math.Pow(y, 2);

                    if (distanceFromCenter <= Math.Pow(radius + 0.5, 2))
                        if (cellInSquare.State != ZeroState)
                            return false;
                }
            }

            return true;
        }

        private bool IsNoGrainWithinARadiusForAbsorbingBc(GrainCellModel centerCell, int radius)
        {
            if (radius <= 0)
                throw new ArgumentException("Radius must be grater than 0");

            int upperLeftGrainCellRow = centerCell.RowNumber - radius;
            int upperLeftGrainCellColumn = centerCell.ColumnNumber - radius;

            for (int row = upperLeftGrainCellRow, y = radius;
                    row <= upperLeftGrainCellRow + 2 * radius;
                    row++, y--)
            {
                if (row >= 0 && row < RowCount)
                {
                    for (int column = upperLeftGrainCellColumn, x = -1 * radius;
                        column <= upperLeftGrainCellColumn + 2 * radius;
                        column++, x++)
                    {
                        if (column >= 0 && column < ColumnCount)
                        {
                            var cellInSquare = CurrentState[row][column];

                            double distanceFromCenter = Math.Pow(x, 2) + Math.Pow(y, 2);

                            if (distanceFromCenter <= Math.Pow(radius + 0.5, 2))
                                if (cellInSquare.State != ZeroState)
                                    return false;
                        }
                    }
                }
            }

            return true;
        }

        public void Clear()
        {
            SetToInitialState();
            RemoveAllGrainsExceptInitial();
        }

        private void SetToInitialState()
        {
            foreach (var row in CurrentState)
                foreach (var grainCell in row)
                    if (grainCell.State != ZeroState)
                    {
                        grainCell.State = ZeroState;
                        PopulatedCellsCount--;
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

        public void SmoothTheGrainsEdgesWithMonteCarloMethod(double kTParameter, int iterations = 1)
        {
            CreateNewStaticRandom();

            for (int i = 0; i < iterations; i++)
            {
                bool[,] isGrainCellSelected = new bool[RowCount, ColumnCount];
                int selectedGrainCellsCount = 0;

                while (selectedGrainCellsCount < CellCount)
                {
                    int randomRow = staticRandom.Next(RowCount);
                    int randomColumn = staticRandom.Next(ColumnCount);

                    if (!isGrainCellSelected[randomRow, randomColumn])
                    {
                        isGrainCellSelected[randomRow, randomColumn] = true;
                        selectedGrainCellsCount++;
                        var selectedGrainCell = (GrainCellModel)CurrentState[randomRow][randomColumn];

                        if (selectedGrainCell.IsOnGrainBoundary)
                        {
                            double energyBefore = selectedGrainCell.Energy;

                            var previousState = selectedGrainCell.State;
                            SetCellGrainToRandomFromNeighborhood(selectedGrainCell);

                            double energyAfter = selectedGrainCell.Energy;

                            double probabilityOfTheChangeAcceptance = GetProbabilityOfTheChangeAcceptance(energyBefore, energyAfter, kTParameter);

                            if (GetRandomDouble(0.0, 1.0) > probabilityOfTheChangeAcceptance)
                                selectedGrainCell.State = previousState;
                        }
                    }
                }
            }
        }

        private void SetCellGrainToRandomFromNeighborhood(GrainCellModel grainCell)
        {
            CreateNewStaticRandom();

            var grainsCounts = grainCell.NeighboringCells.StatesCounts;

            do
            {
                int randomNeighboringGrainInt = staticRandom.Next(grainsCounts.Count);

                grainCell.State = grainsCounts.ElementAt(randomNeighboringGrainInt).Key;
            } while (grainCell.State == ZeroState);
        }

        private double GetProbabilityOfTheChangeAcceptance(double energyBefore, double energyAfter, double kTParameter)
        {
            double energyChangeDifference = energyAfter - energyBefore;

            if (energyChangeDifference <= 0)
                return 1.0;
            else
                return Math.Exp(-energyChangeDifference / kTParameter);
        }

        private double GetRandomDouble(double minimum, double maximum)
        {
            CreateNewStaticRandom();

            return staticRandom.NextDouble() * (maximum - minimum) + minimum;
        }

        public void SpreadDislocations(double strengtheningVariableA, double recoveryVariableB, double durationTimeInSeconds, double timeStepInSeconds, double firstDislocationSetPercentage)
        {
            StringBuilder stringBuilder = new StringBuilder(); //TODO: Ogarnąć to porządnie

            ClearDislocations();
            AddNeighboringCellsToCellsState(CurrentState, false);

            const double ProbabilityThatGrainCellOnBoundaryGetsDislocationPackage = 0.8;
            const double ProbabilityThatGrainCellNotOnBoundaryGetsDislocationPackage = 1.0 - ProbabilityThatGrainCellOnBoundaryGetsDislocationPackage;

            double timeInSeconds = 0;
            double dislocationPool = GetDislocationPool(strengtheningVariableA, recoveryVariableB, timeInSeconds);

            stringBuilder.AppendLine($"{timeInSeconds} {dislocationPool}"); //TODO: Ogarnąć to porządnie

            for (timeInSeconds += timeStepInSeconds; timeInSeconds <= durationTimeInSeconds; timeInSeconds += timeStepInSeconds)
            {
                dislocationPool = GetDislocationPool(strengtheningVariableA, recoveryVariableB, timeInSeconds) - dislocationPool;

                stringBuilder.AppendLine($"{timeInSeconds} {GetDislocationPool(strengtheningVariableA, recoveryVariableB, timeInSeconds)}"); //TODO: Ogarnąć to porządnie
                File.WriteAllText("timeInSeconds_dislocationPool.txt", stringBuilder.ToString()); //TODO: Ogarnąć to porządnie

                double secondDislocationSet = dislocationPool * ((100.0 - firstDislocationSetPercentage) / 100.0);

                double meanDislocationPerGrainCell = dislocationPool / CellCount;
                double firstDislocationPackagePerCell = meanDislocationPerGrainCell * firstDislocationSetPercentage / 100.0;

                foreach (var row in CurrentState)
                    foreach (GrainCellModel grainCell in row)
                        grainCell.DislocationDensity += firstDislocationPackagePerCell;

                while (secondDislocationSet > 0)
                {
                    double secondDislocationPackagePerCell = GetRandomDouble(0.0, firstDislocationPackagePerCell);
                    int randomRow = staticRandom.Next(RowCount);
                    int randomColumn = staticRandom.Next(ColumnCount);

                    var selectedGrainCell = (GrainCellModel)CurrentState[randomRow][randomColumn];

                    if (secondDislocationSet >= secondDislocationPackagePerCell)
                    {
                        if (selectedGrainCell.IsOnGrainBoundary)
                        {
                            if (GetRandomDouble(0.0, 1.0) <= ProbabilityThatGrainCellOnBoundaryGetsDislocationPackage)
                            {
                                selectedGrainCell.DislocationDensity += secondDislocationPackagePerCell;
                                secondDislocationSet -= secondDislocationPackagePerCell;
                            }
                        }
                        else
                        {
                            if (GetRandomDouble(0.0, 1.0) <= ProbabilityThatGrainCellNotOnBoundaryGetsDislocationPackage)
                            {
                                selectedGrainCell.DislocationDensity += secondDislocationPackagePerCell;
                                secondDislocationSet -= secondDislocationPackagePerCell;
                            }
                        }
                    }
                    else
                    {
                        if (selectedGrainCell.IsOnGrainBoundary)
                        {
                            if (GetRandomDouble(0.0, 1.0) <= ProbabilityThatGrainCellOnBoundaryGetsDislocationPackage)
                            {
                                selectedGrainCell.DislocationDensity += secondDislocationSet;
                                secondDislocationSet -= secondDislocationSet;
                            }
                        }
                        else
                        {
                            if (GetRandomDouble(0.0, 1.0) <= ProbabilityThatGrainCellNotOnBoundaryGetsDislocationPackage)
                            {
                                selectedGrainCell.DislocationDensity += secondDislocationSet;
                                secondDislocationSet -= secondDislocationSet;
                            }
                        }
                    }
                }
            }
        }

        private double GetDislocationPool(double strengtheningVariableA, double recoveryVariableB, double timeInSeconds)
            => (strengtheningVariableA / recoveryVariableB) + (1 - (strengtheningVariableA / recoveryVariableB)) * Math.Exp(-recoveryVariableB * timeInSeconds);

        private void ClearDislocations()
        {
            foreach (var row in CurrentState)
                foreach (GrainCellModel grainCell in row)
                    grainCell.DislocationDensity = 0;
        }

        public void NucleateRecrystallizedCells(double criticalDislocationDensity)
        {
            CopyCurrentStateToPrevious(false);

            foreach (var row in PreviousState)
                foreach (GrainCellModel grainCellToNucleate in row)
                    if (grainCellToNucleate.IsOnGrainBoundary && grainCellToNucleate.DislocationDensity > criticalDislocationDensity)
                    {
                        AddNewGrain();
                        var grainCellToNucleateFromCurrentState = (GrainCellModel)CurrentState[grainCellToNucleate.RowNumber][grainCellToNucleate.ColumnNumber];

                        grainCellToNucleateFromCurrentState.IsRecrystallized = true;
                        grainCellToNucleateFromCurrentState.DislocationDensity = criticalDislocationDensity;
                        grainCellToNucleateFromCurrentState.State = grains.Last();
                    }
        }

        public void Recrystallize()
        {
            CopyCurrentStateToPrevious(false);

            foreach (var row in PreviousState)
            {
                foreach (GrainCellModel grainCellToRecrystallize in row)
                {
                    if (!grainCellToRecrystallize.IsRecrystallized)
                    {
                        if (grainCellToRecrystallize.RecrystallizedNeighboringGrainCells.Any())
                        {
                            if (grainCellToRecrystallize.RecrystallizedNeighboringGrainsCounts.Count == 0)
                            {
                                foreach (var grainCell in grainCellToRecrystallize.RecrystallizedNeighboringGrainCells)
                                    Console.WriteLine($"{grainCell.RowNumber} {grainCell.ColumnNumber} {grainCell.IsRecrystallized}");

                                throw new ArgumentException("Coś poszło nie tak");
                            }

                            bool canRecrystallize = true;

                            List<GrainCellModel> neighboringGrainCellsFromPreviousState = grainCellToRecrystallize.NeighboringGrainCellsList;

                            foreach (var grainCell in neighboringGrainCellsFromPreviousState)
                                if (!grainCell.IsRecrystallized && grainCell.DislocationDensity >= grainCellToRecrystallize.DislocationDensity)
                                {
                                    canRecrystallize = false;
                                    break;
                                }

                            if (canRecrystallize)
                            {
                                var grainCellToRecrystallizeFromCurrentState = (GrainCellModel)CurrentState[grainCellToRecrystallize.RowNumber][grainCellToRecrystallize.ColumnNumber];

                                grainCellToRecrystallizeFromCurrentState.State = GetRecrystallizedGrainToExpand(grainCellToRecrystallize);
                                grainCellToRecrystallizeFromCurrentState.IsRecrystallized = true;
                                grainCellToRecrystallizeFromCurrentState.DislocationDensity = 0;
                            }
                        }
                    }
                }
            }
        }

        private ICellState GetRecrystallizedGrainToExpand(GrainCellModel grainCell)
        {
            CreateNewStaticRandom();

            var recrystallizedGrainsWithMaxCount = GetRecrystallizedGrainsWithMaxCount(grainCell);

            if (recrystallizedGrainsWithMaxCount.Count() == 1)
                return recrystallizedGrainsWithMaxCount.First();
            else
                return recrystallizedGrainsWithMaxCount.ElementAt(staticRandom.Next(recrystallizedGrainsWithMaxCount.Count));
        }

        private List<ICellState> GetRecrystallizedGrainsWithMaxCount(GrainCellModel grainCell)
        {
            var recrystallizedNeighboringGrainsCounts = grainCell.RecrystallizedNeighboringGrainsCounts;
            var grainsWithMaxCount = new List<ICellState>();

            if (recrystallizedNeighboringGrainsCounts.Count > 1)
            {
                int maxCount = 0;

                foreach (var grainCount in recrystallizedNeighboringGrainsCounts)
                    if (grainCount.Value > maxCount && grainCount.Key != ZeroState)
                        maxCount = grainCount.Value;

                foreach (var grainCount in recrystallizedNeighboringGrainsCounts)
                    if (grainCount.Value == maxCount && grainCount.Key != ZeroState)
                        grainsWithMaxCount.Add(grainCount.Key);
            }
            else
                grainsWithMaxCount.Add(recrystallizedNeighboringGrainsCounts.First().Key);

            return grainsWithMaxCount;
        }
    }
}