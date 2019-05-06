using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using BoundaryConditionModel = ElementaryCellularAutomaton.Models.BoundaryConditionModel;

namespace GameOfLife.Models
{
    public class CellGrid2DModel
    {
        private List<List<CellModel>> currentState;
        private List<List<CellModel>> previousState;
        public int ColumnCount { get; private set; }
        public int RowCount { get; private set; }
        public CellsNeighborhoodTypeModel NeighborhoodType { get; private set; }
        public RuleModel Rule { get; private set; }
        public BoundaryConditionModel BoundaryCondition { get; private set; }

        private MemoryStream imageMemoryStream;
        private BitmapImage bitmapImage;

        public CellGrid2DModel(int columnCount, int rowCount, CellsNeighborhoodTypeModel neighborhoodType, RuleModel rule, BoundaryConditionModel boundaryCondition)
        {
            ColumnCount = columnCount;
            RowCount = rowCount;
            NeighborhoodType = neighborhoodType;
            Rule = rule;
            BoundaryCondition = boundaryCondition;

            currentState = new List<List<CellModel>>();
            previousState = new List<List<CellModel>>();
            CreateCells();
            PreparePreviousStateLists();
            AddNeighborhoodToCellsState(currentState);
        }

        private void CreateCells()
        {
            for (int row = 0, cellId = 0; row < RowCount; row++)
            {
                currentState.Add(new List<CellModel>());

                for (int column = 0; column < ColumnCount; column++, cellId++)
                {
                    currentState[row].Add(new CellModel(cellId, column, row, false));
                }
            }
        }

        private void PreparePreviousStateLists()
        {
            for (int row = 0; row < RowCount; row++)
            {
                previousState.Add(new List<CellModel>());
            }
        }

        private void AddNeighborhoodToCellsState(List<List<CellModel>> cellsState)
        {
            for (int row = 0, cellId = 0; row < RowCount; row++)
            {
                for (int column = 0; column < ColumnCount; column++, cellId++)
                {
                    switch (BoundaryCondition)
                    {
                        case BoundaryConditionModel.OutsideIsDead:
                        case BoundaryConditionModel.OutsideIsAlive:
                            CellModel outsideCell = new CellModel(BoundaryCondition == BoundaryConditionModel.OutsideIsAlive ? true : false);

                            cellsState[row][column].Neighborhood = new CellsNeighborhood
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

                        case BoundaryConditionModel.Periodical:
                            cellsState[row][column].Neighborhood = new CellsNeighborhood
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

            foreach (var row in currentState)
            {
                foreach (var evolvingCell in row)
                {
                    CellModel oldCell = previousState[evolvingCell.RowNumber][evolvingCell.ColumnNumber];

                    if (evolvingCell.IsAlive)
                        evolvingCell.IsAlive = Rule.WillSurvive(oldCell.Neighborhood.AliveNeighboursCount);
                    else
                        evolvingCell.IsAlive = Rule.WillBeBorn(oldCell.Neighborhood.AliveNeighboursCount);
                }
            }
        }

        private void CopyCurrentStateToPrevious()
        {
            previousState = currentState.ConvertAll(row => new List<CellModel>(row.ConvertAll(cell => new CellModel(cell))));

            AddNeighborhoodToCellsState(previousState);
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

            Bitmap image = new Bitmap(imageWidth, imageHeight);
            SolidBrush royalBlueSolidBrush = new SolidBrush(Color.RoyalBlue);
            Pen blackPen = new Pen(Color.Black, lineWidth);

            using (var imageGraphics = Graphics.FromImage(image))
            {
                imageGraphics.Clear(Color.White);

                if (lineWidth > 0)
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

                for (int row = 0; row < RowCount; row++)
                {
                    cellPosition.Y = row * (cellHeight + lineWidth) + lineWidth;

                    for (int column = 0; column < ColumnCount; column++)
                    {
                        cellPosition.X = column * (cellWidth + lineWidth) + lineWidth;

                        if (currentState[row][column].IsAlive)
                            imageGraphics.FillRectangle(royalBlueSolidBrush, new Rectangle(cellPosition, cellSize));

                        currentState[row][column].StartPositionOnImage = cellPosition;
                        currentState[row][column].EndPositionOnImage = cellPosition + cellSize;
                    }
                }

                imageGraphics.Dispose();
                royalBlueSolidBrush.Dispose();
                blackPen.Dispose();
            }

            bitmapImage.BeginInit();
            image.Save(imageMemoryStream, ImageFormat.Png);
            imageMemoryStream.Seek(0, SeekOrigin.Begin);
            bitmapImage.StreamSource = imageMemoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        public void NegateCellState(Point mousePositionOverImage)
        {
            bool cellIsFound = false;

            foreach (var row in currentState)
            {
                foreach (var cell in row)
                {
                    if (cell.StartPositionOnImage.X <= mousePositionOverImage.X &&
                        cell.StartPositionOnImage.Y <= mousePositionOverImage.Y &&
                        cell.EndPositionOnImage.X >= mousePositionOverImage.X &&
                        cell.EndPositionOnImage.Y >= mousePositionOverImage.Y)
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
            foreach (var row in currentState)
                foreach (var cell in row)
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

                if (!currentState[randomRow][randomColumn].IsAlive)
                {
                    currentState[randomRow][randomColumn].Revive();
                    cellCount--;
                }
            }
        }

        public bool PutBeehiveInTheMiddle()
        {
            KillAll();

            try
            {
                currentState[RowCount / 2 - 1][ColumnCount / 2].Revive(); currentState[RowCount / 2 - 1][ColumnCount / 2 + 1].Revive();
                currentState[RowCount / 2][ColumnCount / 2 - 1].Revive(); currentState[RowCount / 2][ColumnCount / 2 + 2].Revive();
                currentState[RowCount / 2 + 1][ColumnCount / 2].Revive(); currentState[RowCount / 2 + 1][ColumnCount / 2 + 1].Revive();

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
                currentState[RowCount / 2 - 1][ColumnCount / 2].Revive(); currentState[RowCount / 2 - 1][ColumnCount / 2 + 1].Revive();
                currentState[RowCount / 2][ColumnCount / 2 - 1].Revive(); currentState[RowCount / 2][ColumnCount / 2].Revive();
                currentState[RowCount / 2 + 1][ColumnCount / 2 + 1].Revive();

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
                currentState[RowCount / 2 - 1][ColumnCount / 2].Revive();
                currentState[RowCount / 2][ColumnCount / 2].Revive();
                currentState[RowCount / 2 + 1][ColumnCount / 2].Revive();

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