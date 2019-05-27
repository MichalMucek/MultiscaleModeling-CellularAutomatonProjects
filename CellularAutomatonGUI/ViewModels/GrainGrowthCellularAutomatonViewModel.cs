using Caliburn.Micro;
using CellularAutomaton2D.Models;
using GrainGrowthCellularAutomaton.Models;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;
using MessageBox = System.Windows.MessageBox;

namespace CellularAutomatonGUI.ViewModels
{
    public class GrainGrowthCellularAutomatonViewModel : Screen
    {
        private int columnCount = 31;
        private int rowCount = 31;
        private NucleationMethodModel selectedNucleationMethod = NucleationMethodModel.Uniform;
        private int nucleusesInColumnCount = 1;
        private int nucleusesInRowCount = 1;
        private int neighborhoodRadius = 1;
        private bool isRadialNeighborhoodSelected = false;
        private bool isUniformMethodSelected = true;
        private int randomNucleusesCount = 1;
        private bool isRandomMethodSelected = false;
        private bool isRandomWithRadiusMethodSelected = false;
        private int nucleusRadius = 1;
        private bool canClearGrainCellGrid = false;
        private bool canNucleate = false;
        private bool canSetOrUnsetGrainCellGrid = true;
        private bool isGrainCellGridUnset = true;
        private bool canContinueOrPauseGrowth = false;
        private string continueOrPauseGrowthContent = "Start";
        private bool canShowNextGrowthStep = false;
        private string setOrUnsetGrainCellGridContent = "Set";
        private int timeInterval = 250;
        private int cellWidth = 12;
        private int cellHeight = 12;
        private int lineWidth = 1;
        private BitmapImage grainCellGridBitmapImage;
        private bool growthIsDone = false;

        private DispatcherTimer evolverAndDrawerDispatcherTimer;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        private GrainCellGrid2DModel grainCellGrid;

        public GrainGrowthCellularAutomatonViewModel()
        {
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.VonNeumann);
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.Moore);
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.RandomPentagonal);
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.LeftHexagonal);
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.RightHexagonal);
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.RandomHexagonal);
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.Radial);
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.RadialWithCenterOfMass);

            BoundaryConditions.Add(BoundaryConditionModel.Absorbing);
            BoundaryConditions.Add(BoundaryConditionModel.Periodic);

            NucleationMethods.Add(NucleationMethodModel.Uniform);
            NucleationMethods.Add(NucleationMethodModel.Random);
            NucleationMethods.Add(NucleationMethodModel.RandomWithRadius);

            evolverAndDrawerDispatcherTimer = new DispatcherTimer();
            evolverAndDrawerDispatcherTimer.Interval = TimeSpan.FromMilliseconds(timeInterval);
            evolverAndDrawerDispatcherTimer.Tick += EvolverAndDrawer_Tick;

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(1, 1);

            CreateGrainCellGridPreview();
            RunDrawerTask();
        }

        private async void EvolverAndDrawer_Tick(object sender, EventArgs e)
        {
            try
            {
                await RunEvolverAndDrawerTask();
            }
            catch (TaskCanceledException taskCanceledException)
            {
                Console.WriteLine(taskCanceledException.StackTrace);
            }
        }

        private Task RunEvolverAndDrawerTask()
        {
            return Task.Run(() =>
            {
                grainCellGrid.Evolve();
                GrowthIsDone = grainCellGrid.IsFullyPopulated;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        GrainCellGridBitmapImage = grainCellGrid?.GetBitmapImage(cellWidth, cellHeight, lineWidth)
                );
            }, cancellationToken);
        }

        public bool GrowthIsDone
        {
            get => growthIsDone;
            set
            {
                growthIsDone = value;

                if (growthIsDone)
                    SetOrUnsetGrainCellGrid();

                NotifyOfPropertyChange(() => GrowthIsDone);
            }
        }

        public int ColumnCount
        {
            get => columnCount;
            set
            {
                columnCount = value;
                NotifyOfPropertyChange(() => ColumnCount);
                NotifyOfPropertyChange(() => NucleusRadiusMaximum);
                NotifyOfPropertyChange(() => GrainCellCount);

                CreateGrainCellGridPreview();
                RunDrawerTask();
            }
        }

        public int RowCount
        {
            get => rowCount;
            set
            {
                rowCount = value;
                NotifyOfPropertyChange(() => RowCount);
                NotifyOfPropertyChange(() => NucleusRadiusMaximum);
                NotifyOfPropertyChange(() => GrainCellCount);

                CreateGrainCellGridPreview();
                RunDrawerTask();
            }
        }

        private void CreateGrainCellGridPreview()
        {
            grainCellGrid = new GrainCellGrid2DModel(columnCount, rowCount, CellNeighborhoodTypeModel.VonNeumann, BoundaryConditionModel.Absorbing);
        }

        public int GrainCellCount => rowCount * columnCount;

        public BindableCollection<BoundaryConditionModel> BoundaryConditions { get; } = new BindableCollection<BoundaryConditionModel>();

        public BoundaryConditionModel SelectedBoundaryCondition { get; set; } = BoundaryConditionModel.Absorbing;

        public BindableCollection<CellNeighborhoodTypeModel> CellNeighborhoods { get; } = new BindableCollection<CellNeighborhoodTypeModel>();

        private CellNeighborhoodTypeModel selectedCellNeighborhood = CellNeighborhoodTypeModel.VonNeumann;

        public CellNeighborhoodTypeModel SelectedCellNeighborhood
        {
            get => selectedCellNeighborhood;
            set
            {
                selectedCellNeighborhood = value;

                switch (selectedCellNeighborhood)
                {
                    case CellNeighborhoodTypeModel.Radial:
                    case CellNeighborhoodTypeModel.RadialWithCenterOfMass:
                        IsRadialNeighborhoodSelected = true;
                        break;

                    default:
                        IsRadialNeighborhoodSelected = false;
                        break;
                }
            }
        }

        public int NeighborhoodRadius
        {
            get => neighborhoodRadius;
            set
            {
                neighborhoodRadius = value;
                NotifyOfPropertyChange(() => NeighborhoodRadius);
            }
        }

        public bool IsRadialNeighborhoodSelected
        {
            get => isRadialNeighborhoodSelected && isGrainCellGridUnset;
            set
            {
                isRadialNeighborhoodSelected = value;
                NotifyOfPropertyChange(() => IsRadialNeighborhoodSelected);
            }
        }

        public BindableCollection<NucleationMethodModel> NucleationMethods { get; } = new BindableCollection<NucleationMethodModel>();

        public NucleationMethodModel SelectedNucleationMethod
        {
            get => selectedNucleationMethod;
            set
            {
                selectedNucleationMethod = value;

                switch (selectedNucleationMethod)
                {
                    case NucleationMethodModel.Uniform:
                        IsUniformMethodSelected = true;
                        IsRandomMethodSelected = false;
                        IsRandomWithRadiusMethodSelected = false;
                        break;

                    case NucleationMethodModel.Random:
                        IsRandomMethodSelected = true;
                        IsUniformMethodSelected = false;
                        IsRandomWithRadiusMethodSelected = false;
                        break;

                    case NucleationMethodModel.RandomWithRadius:
                        IsRandomMethodSelected = true;
                        IsRandomWithRadiusMethodSelected = true;
                        IsUniformMethodSelected = false;
                        break;
                }
            }
        }

        public int NucleusesInColumnCount
        {
            get => nucleusesInColumnCount;
            set
            {
                nucleusesInColumnCount = value;
                NotifyOfPropertyChange(() => NucleusesInColumnCount);
            }
        }

        public int NucleusesInRowCount
        {
            get => nucleusesInRowCount;
            set
            {
                nucleusesInRowCount = value;
                NotifyOfPropertyChange(() => NucleusesInRowCount);
            }
        }

        public bool IsUniformMethodSelected
        {
            get => isUniformMethodSelected && IsGrainCellGridSet;
            set
            {
                isUniformMethodSelected = value;
                NotifyOfPropertyChange(() => IsUniformMethodSelected);
            }
        }

        public int RandomNucleusesCount
        {
            get => randomNucleusesCount;
            set
            {
                randomNucleusesCount = value;
                NotifyOfPropertyChange(() => RandomNucleusesCount);
            }
        }

        public bool IsRandomMethodSelected
        {
            get => isRandomMethodSelected && IsGrainCellGridSet;
            set
            {
                isRandomMethodSelected = value;
                NotifyOfPropertyChange(() => IsRandomMethodSelected);
            }
        }

        public bool IsRandomWithRadiusMethodSelected
        {
            get => isRandomWithRadiusMethodSelected && IsGrainCellGridSet;
            set
            {
                isRandomWithRadiusMethodSelected = value;
                NotifyOfPropertyChange(() => IsRandomWithRadiusMethodSelected);
            }
        }

        public int NucleusRadius
        {
            get => nucleusRadius;
            set
            {
                nucleusRadius = value;
                NotifyOfPropertyChange(() => NucleusRadius);
            }
        }

        public int NucleusRadiusMaximum
        {
            get
            {
                if (columnCount < rowCount)
                    return columnCount / 2;
                else
                    return rowCount / 2;
            }
        }

        public bool IsClickingOnImageEnabled { get; set; } = true;

        public async void ClearGrainCellGrid()
        {
            await Task.Run(() => grainCellGrid.Clear());
            RunDrawerTask();
        }

        public bool CanClearGrainCellGrid
        {
            get => canClearGrainCellGrid;
            set
            {
                canClearGrainCellGrid = value;
                NotifyOfPropertyChange(() => CanClearGrainCellGrid);
            }
        }

        public async void Nucleate()
        {
            CanClearGrainCellGrid = false;
            CanNucleate = false;
            CanSetOrUnsetGrainCellGrid = false;
            CanContinueOrPauseGrowth = false;
            CanShowNextGrowthStep = false;

            await Task.Run(() =>
            {
                switch (selectedNucleationMethod)
                {
                    case NucleationMethodModel.Uniform:
                        grainCellGrid.PutGrainNucleusesUniformly(nucleusesInColumnCount, nucleusesInRowCount);
                        break;

                    case NucleationMethodModel.Random:
                        grainCellGrid.PutGrainNucleusesRandomly(randomNucleusesCount);
                        break;

                    case NucleationMethodModel.RandomWithRadius:
                        uint putNucleusesCount = grainCellGrid.PutGrainNucleusesRandomlyWithRadius(randomNucleusesCount, nucleusRadius);

                        if (putNucleusesCount < randomNucleusesCount)
                            if (putNucleusesCount == 1)
                                MessageBox.Show($"{putNucleusesCount} nucleus out of {randomNucleusesCount} with radius of {nucleusRadius} were possible to put.", "Random nucleation result");
                            else
                                MessageBox.Show($"{putNucleusesCount} nucleuses out of {randomNucleusesCount} with radius of {nucleusRadius} were possible to put.", "Random nucleation result");
                        break;
                }
            });

            RunDrawerTask();

            CanClearGrainCellGrid = true;
            CanNucleate = true;
            CanSetOrUnsetGrainCellGrid = true;
            CanContinueOrPauseGrowth = true;
            CanShowNextGrowthStep = true;
        }

        public bool CanNucleate
        {
            get => canNucleate;
            set
            {
                canNucleate = value;
                NotifyOfPropertyChange(() => CanNucleate);
            }
        }

        public void SetOrUnsetGrainCellGrid()
        {
            switch (setOrUnsetGrainCellGridContent)
            {
                case "Set":
                    SetGrainCellGrid();
                    RunDrawerTask();
                    break;

                case "Unset":
                    UnsetGrainCellGrid();
                    break;
            }
        }

        private async void SetGrainCellGrid()
        {
            CanSetOrUnsetGrainCellGrid = false;

            await CreateNewGrainCellGrid();
            IsGrainCellGridUnset = false;

            CanContinueOrPauseGrowth = true;
            CanShowNextGrowthStep = true;
            CanClearGrainCellGrid = true;
            CanNucleate = true;
            SetOrUnsetGrainCellGridContent = "Unset";
            ContinueOrPauseGrowthContent = "Start";

            CanSetOrUnsetGrainCellGrid = true;
        }

        private Task CreateNewGrainCellGrid()
        {
            return Task.Run(() =>
            {
                switch (SelectedCellNeighborhood)
                {
                    case CellNeighborhoodTypeModel.Radial:
                    case CellNeighborhoodTypeModel.RadialWithCenterOfMass:
                        grainCellGrid = new GrainCellGrid2DModel(
                            columnCount,
                            rowCount,
                            SelectedCellNeighborhood,
                            SelectedBoundaryCondition,
                            neighborhoodRadius);
                        break;

                    default:
                        grainCellGrid = new GrainCellGrid2DModel(
                            columnCount,
                            rowCount,
                            SelectedCellNeighborhood,
                            SelectedBoundaryCondition);
                        break;
                }
            });
        }

        private void UnsetGrainCellGrid()
        {
            CanSetOrUnsetGrainCellGrid = false;

            evolverAndDrawerDispatcherTimer.Stop();
            CancelTasksAndResetCancellationTokenSource();

            IsGrainCellGridUnset = true;
            CanContinueOrPauseGrowth = false;
            CanShowNextGrowthStep = false;
            CanClearGrainCellGrid = false;
            CanNucleate = false;
            SetOrUnsetGrainCellGridContent = "Set";

            CanSetOrUnsetGrainCellGrid = true;
        }

        public bool CanSetOrUnsetGrainCellGrid
        {
            get => canSetOrUnsetGrainCellGrid;
            set
            {
                canSetOrUnsetGrainCellGrid = value;
                NotifyOfPropertyChange(() => CanSetOrUnsetGrainCellGrid);
            }
        }

        public bool IsGrainCellGridUnset
        {
            get => isGrainCellGridUnset;
            set
            {
                isGrainCellGridUnset = value;
                IsGrainCellGridSet = !value;
                NotifyOfPropertyChange(() => IsGrainCellGridUnset);
                NotifyOfPropertyChange(() => IsRadialNeighborhoodSelected);
                NotifyOfPropertyChange(() => IsUniformMethodSelected);
                NotifyOfPropertyChange(() => IsRandomMethodSelected);
                NotifyOfPropertyChange(() => IsRandomWithRadiusMethodSelected);
            }
        }

        public bool IsGrainCellGridSet
        {
            get => !isGrainCellGridUnset;
            set
            {
                NotifyOfPropertyChange(() => IsGrainCellGridSet);
                NotifyOfPropertyChange(() => CanNucleate);
            }
        }

        public string SetOrUnsetGrainCellGridContent
        {
            get => setOrUnsetGrainCellGridContent;
            set
            {
                setOrUnsetGrainCellGridContent = value;
                NotifyOfPropertyChange(() => SetOrUnsetGrainCellGridContent);
            }
        }

        private void CancelTasksAndResetCancellationTokenSource()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }

        public void ContinueOrPauseGrowth()
        {
            switch (continueOrPauseGrowthContent)
            {
                case "Start":
                case "Continue":
                    ContinueGrowth();
                    break;

                case "Pause":
                    PauseGrowth();
                    break;
            }
        }

        private void ContinueGrowth()
        {
            CanShowNextGrowthStep = false;
            CanClearGrainCellGrid = false;
            CanNucleate = false;
            evolverAndDrawerDispatcherTimer.Start();
            ContinueOrPauseGrowthContent = "Pause";
        }

        private void PauseGrowth()
        {
            evolverAndDrawerDispatcherTimer.Stop();
            CancelTasksAndResetCancellationTokenSource();
            CanShowNextGrowthStep = true;
            CanClearGrainCellGrid = true;
            CanNucleate = true;
            ContinueOrPauseGrowthContent = "Continue";
        }

        public bool CanContinueOrPauseGrowth
        {
            get => canContinueOrPauseGrowth;
            set
            {
                canContinueOrPauseGrowth = value;
                NotifyOfPropertyChange(() => CanContinueOrPauseGrowth);
            }
        }

        public string ContinueOrPauseGrowthContent
        {
            get => continueOrPauseGrowthContent;
            set
            {
                continueOrPauseGrowthContent = value;
                NotifyOfPropertyChange(() => ContinueOrPauseGrowthContent);
            }
        }

        public async void ShowNextGrowthStep()
        {
            CanShowNextGrowthStep = false;
            CanContinueOrPauseGrowth = false;
            CanClearGrainCellGrid = false;
            CanNucleate = false;

            if (continueOrPauseGrowthContent == "Start")
                ContinueOrPauseGrowthContent = "Continue";

            await RunEvolverAndDrawerTask();

            if (!growthIsDone)
            {
                CanShowNextGrowthStep = true;
                CanContinueOrPauseGrowth = true;
                CanClearGrainCellGrid = true;
                CanNucleate = true;
            }
        }

        public bool CanShowNextGrowthStep
        {
            get => canShowNextGrowthStep;
            set
            {
                canShowNextGrowthStep = value;
                NotifyOfPropertyChange(() => CanShowNextGrowthStep);
            }
        }

        public int TimeInterval
        {
            get => timeInterval;
            set
            {
                timeInterval = value;
                evolverAndDrawerDispatcherTimer.Interval = TimeSpan.FromMilliseconds(value);
                NotifyOfPropertyChange(() => TimeInterval);
            }
        }

        public int CellWidth
        {
            get => cellWidth;
            set
            {
                cellWidth = value;
                NotifyOfPropertyChange(() => CellWidth);
                RunDrawerTask();
            }
        }

        public int CellHeight
        {
            get => cellHeight;
            set
            {
                cellHeight = value;
                NotifyOfPropertyChange(() => CellHeight);
                RunDrawerTask();
            }
        }

        public int LineWidth
        {
            get => lineWidth;
            set
            {
                lineWidth = value;
                NotifyOfPropertyChange(() => LineWidth);
                RunDrawerTask();
            }
        }

        private async void RunDrawerTask()
        {
            await Task.Run(() =>
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    GrainCellGridBitmapImage = grainCellGrid?.GetBitmapImage(cellWidth, cellHeight, lineWidth)
                )
            );
        }

        public BitmapImage GrainCellGridBitmapImage
        {
            get => grainCellGridBitmapImage;
            set
            {
                grainCellGridBitmapImage = value;
                NotifyOfPropertyChange(() => GrainCellGridBitmapImage);
            }
        }

        public async void PutGrainNucleus(object sender, MouseEventArgs e)
        {
            var imageControl = sender as Image;

            await Task.Run(() =>
            {
                if (!isGrainCellGridUnset && grainCellGrid != null && IsClickingOnImageEnabled)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        Size imageControlSize = new Size
                        {
                            Width = (int)imageControl.ActualWidth,
                            Height = (int)imageControl.ActualHeight
                        };

                        Point mousePositionOverCellGridImageControl = new Point
                        {
                            X = (int)e.GetPosition(imageControl).X,
                            Y = (int)e.GetPosition(imageControl).Y
                        };

                        Point mousePositionOverCellGridBitmapImage = new Point
                        {
                            X = (int)((grainCellGridBitmapImage.PixelWidth / (double)imageControlSize.Width) * mousePositionOverCellGridImageControl.X),
                            Y = (int)((grainCellGridBitmapImage.PixelHeight / (double)imageControlSize.Height) * mousePositionOverCellGridImageControl.Y)
                        };

                        grainCellGrid?.PutGrainNucleus(mousePositionOverCellGridBitmapImage);
                    });
                }
            });

            RunDrawerTask();
        }
    }
}