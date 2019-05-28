using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using CellularAutomaton2D;
using CellularAutomaton2D.Models;

namespace LifeLikeCellularAutomaton.Models
{
    public class CellGrid2DModel : ICellGrid
    {
        public List<List<ICell>> CurrentState { get; private set; }
        public List<List<ICell>> PreviousState { get; private set; }
        public ICellState ZeroState { get; private set; }
        public CellStateModel AliveState { get; private set; }
        public int ColumnCount { get; private set; }
        public int RowCount { get; private set; }
        public CellNeighborhoodTypeModel NeighborhoodType { get; private set; }
        public RuleModel Rule { get; private set; }
        public BoundaryConditionModel BoundaryCondition { get; private set; }
        public int CellCount => ColumnCount * RowCount;
        public int PopulatedCellsCount { get; private set; } = 0;
        public bool IsFullyPopulated => PopulatedCellsCount == CellCount;

        private MemoryStream imageMemoryStream;
        private BitmapImage bitmapImage;

        public CellGrid2DModel(int columnCount, int rowCount, CellNeighborhoodTypeModel neighborhoodType,
            RuleModel rule, BoundaryConditionModel boundaryCondition)
        {
            ColumnCount = columnCount;
            RowCount = rowCount;
            ZeroState = new CellStateModel();
            AliveState = new CellStateModel(1);
            AliveState.SetColor(System.Windows.Media.Colors.RoyalBlue);
            NeighborhoodType = neighborhoodType;
            Rule = rule;
            BoundaryCondition = boundaryCondition;

            CurrentState = new List<List<ICell>>();
            PreviousState = new List<List<ICell>>();
            CreateNewCellsForCurrentState();
            CreateRowsInPreviousState();
            AddNeighboringCellsToCellsState(CurrentState);
        }

        private void CreateNewCellsForCurrentState()
        {
            for (int row = 0, cellId = 0; row < RowCount; row++)
            {
                CurrentState.Add(new List<ICell>());

                for (int column = 0; column < ColumnCount; column++, cellId++)
                    CurrentState[row].Add(new CellModel(cellId, column, row, (CellStateModel)ZeroState,
                        AliveState, (CellStateModel)ZeroState));
            }
        }

        private void CreateRowsInPreviousState()
        {
            for (int row = 0; row < RowCount; row++)
                PreviousState.Add(new List<ICell>());
        }

        private void AddNeighboringCellsToCellsState(List<List<ICell>> cellsState)
        {
            for (int row = 0, cellId = 0; row < RowCount; row++)
            {
                for (int column = 0; column < ColumnCount; column++, cellId++)
                {
                    switch (BoundaryCondition)
                    {
                        case BoundaryConditionModel.Absorbing:
                        case BoundaryConditionModel.CounterAbsorbing:
                            CellModel outsideCell = new CellModel(BoundaryCondition == BoundaryConditionModel.CounterAbsorbing,
                                AliveState, (CellStateModel)ZeroState);

                            cellsState[row][column].NeighboringCells = new CellNeighborhood
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
                            cellsState[row][column].NeighboringCells = new CellNeighborhood
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
            CopyCurrentStateToPrevious();

            foreach (var row in CurrentState)
            {
                foreach (CellModel evolvingCell in row)
                {
                    CellModel oldCell = (CellModel)PreviousState[evolvingCell.RowNumber][evolvingCell.ColumnNumber];
                    int aliveCellsCount;

                    if (evolvingCell.IsAlive)
                    {
                        if (oldCell.NeighboringCells.StatesCounts.TryGetValue(AliveState, out aliveCellsCount))
                            evolvingCell.IsAlive = Rule.WillSurvive(aliveCellsCount);
                        else
                            evolvingCell.IsAlive = Rule.WillSurvive(0);
                    }
                    else
                    {
                        if (oldCell.NeighboringCells.StatesCounts.TryGetValue(AliveState, out aliveCellsCount))
                            evolvingCell.IsAlive = Rule.WillBeBorn(aliveCellsCount);
                        else
                            evolvingCell.IsAlive = Rule.WillBeBorn(0);
                    }
                }
            }
        }

        private void CopyCurrentStateToPrevious()
        {
            PreviousState = CurrentState.ConvertAll(
                row => new List<ICell>(row.ConvertAll(cell => new CellModel((CellModel)cell)))
            );

            AddNeighboringCellsToCellsState(PreviousState);
        }

        public BitmapImage GetBitmapImage(int cellWidth, int cellHeight, int lineWidth)
        {
            imageMemoryStream?.Dispose();
            imageMemoryStream = new MemoryStream();
            bitmapImage = new BitmapImage();

            int imageWidth = ColumnCount * cellWidth + (ColumnCount + 1) * lineWidth;
            int imageHeight = RowCount * cellHeight + (RowCount + 1) * lineWidth;

            Point cellPosition = new Point();
            Size cellSize = new Size(cellWidth, cellHeight);
            Point lineFirstPoint = new Point();
            Point lineSecondPoint = new Point();

            using (var image = new Bitmap(imageWidth, imageHeight))
            using (var imageGraphics = Graphics.FromImage(image))
            {
                imageGraphics.Clear(Color.FromArgb(ZeroState.Color.R, ZeroState.Color.G, ZeroState.Color.B));

                if (lineWidth > 0)
                {
                    using (var blackPen = new Pen(Color.Black, lineWidth))
                    {
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

                Color aliveCellColor = Color.FromArgb(AliveState.Color.R, AliveState.Color.G, AliveState.Color.B);

                using (var aliveCellColorSolidBrush = new SolidBrush(aliveCellColor))
                {
                    for (int row = 0; row < RowCount; row++)
                    {
                        cellPosition.Y = row * (cellHeight + lineWidth) + lineWidth;

                        for (int column = 0; column < ColumnCount; column++)
                        {
                            cellPosition.X = column * (cellWidth + lineWidth) + lineWidth;

                            CellModel cell = (CellModel)CurrentState[row][column];

                            if (cell.IsAlive)
                                imageGraphics.FillRectangle(aliveCellColorSolidBrush, new Rectangle(cellPosition, cellSize));

                            if (HasGrainCellPositionOnImageChanged(cell, cellPosition, cellSize))
                            {
                                cell.StartPositionOnImage = new System.Windows.Point(cellPosition.X, cellPosition.Y);
                                cell.EndPositionOnImage = cell.StartPositionOnImage + new System.Windows.Vector(cellSize.Width, cellSize.Height);
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

        private bool HasGrainCellPositionOnImageChanged(ICell cell, Point cellPosition, Size cellSize)
        {
            return cell.StartPositionOnImage.X != cellPosition.X ||
                    cell.StartPositionOnImage.Y != cellPosition.Y ||
                    cell.EndPositionOnImage.X != cellPosition.X + cellSize.Width ||
                    cell.EndPositionOnImage.Y != cellPosition.Y + cellSize.Height;
        }

        public void NegateCellState(Point mousePositionOverBitmapImage)
        {
            bool cellIsFound = false;

            foreach (var row in CurrentState)
            {
                foreach (CellModel cell in row)
                {
                    if (cell.StartPositionOnImage.X <= mousePositionOverBitmapImage.X &&
                        cell.StartPositionOnImage.Y <= mousePositionOverBitmapImage.Y &&
                        cell.EndPositionOnImage.X >= mousePositionOverBitmapImage.X &&
                        cell.EndPositionOnImage.Y >= mousePositionOverBitmapImage.Y)
                    {
                        cell.IsAlive = !cell.IsAlive;
                        cellIsFound = true;
                        break;
                    }
                }

                if (cellIsFound) break;
            }
        }

        public void KillAll()
        {
            foreach (var row in CurrentState)
                foreach (CellModel cell in row)
                    cell.Kill();
        }

        public void Randomize(int cellCount)
        {
            KillAll();

            Random random = new Random();

            while (cellCount > 0)
            {
                int randomRow = random.Next(RowCount);
                int randomColumn = random.Next(ColumnCount);

                CellModel cell = (CellModel)CurrentState[randomRow][randomColumn];

                if (cell.IsDead)
                {
                    cell.Revive();
                    cellCount--;
                }
            }
        }

        public bool PutBeehiveInTheMiddle()
        {
            KillAll();

            try
            {
                CurrentState[RowCount / 2 - 1][ColumnCount / 2].State = AliveState; CurrentState[RowCount / 2 - 1][ColumnCount / 2 + 1].State = AliveState;
                CurrentState[RowCount / 2][ColumnCount / 2 - 1].State = AliveState; CurrentState[RowCount / 2][ColumnCount / 2 + 2].State = AliveState;
                CurrentState[RowCount / 2 + 1][ColumnCount / 2].State = AliveState; CurrentState[RowCount / 2 + 1][ColumnCount / 2 + 1].State = AliveState;

                return true;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.StackTrace);

                return false;
            }
        }

        public bool PutGliderInTheMiddle()
        {
            KillAll();

            try
            {
                CurrentState[RowCount / 2 - 1][ColumnCount / 2].State = AliveState; CurrentState[RowCount / 2 - 1][ColumnCount / 2 + 1].State = AliveState;
                CurrentState[RowCount / 2][ColumnCount / 2 - 1].State = AliveState; CurrentState[RowCount / 2][ColumnCount / 2].State = AliveState;
                CurrentState[RowCount / 2 + 1][ColumnCount / 2 + 1].State = AliveState;

                return true;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.StackTrace);

                return false;
            }
        }

        public bool PutBlinkerInTheMiddle()
        {
            KillAll();

            try
            {
                CurrentState[RowCount / 2 - 1][ColumnCount / 2].State = AliveState;
                CurrentState[RowCount / 2][ColumnCount / 2].State = AliveState;
                CurrentState[RowCount / 2 + 1][ColumnCount / 2].State = AliveState;

                return true;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.StackTrace);

                return false;
            }
        }
    }
}