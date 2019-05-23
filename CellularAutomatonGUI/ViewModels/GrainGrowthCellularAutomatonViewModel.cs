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
        private int columnCount = 11;
        private int rowCount = 11;
        private NucleationMethodModel selectedNucleationMethod = NucleationMethodModel.Uniform;
        private int nucleusesInColumnCount = 1;
        private int nucleusesInRowCount = 1;
        private bool isUniformMethodSelected = true;
        private int randomNucleusesCount = 1;
        private bool isRandomMethodSelected = false;
        private bool isRandomWithRadiusMethodSelected = false;
        private int nucleusRadius = 1;
        private bool canNucleate = false;
        private bool canStartStop = true;
        private bool isStopped = true;
        private bool canContinuePause = false;
        private string continuePauseContent = "Continue";
        private bool canShowNextStep = false;
        private string startStopContent = "Start";
        private int timeInterval = 250;
        private int cellWidth = 5;
        private int cellHeight = 5;
        private int lineWidth = 1;
        private BitmapImage grainCellGridBitmapImage;
        private bool growthIsDone = false;

        private DispatcherTimer evolverAndDrawerDispatcherTimer;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        private GrainCellGrid2DModel grainCellGrid;

        public GrainGrowthCellularAutomatonViewModel()
        {
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.Moore);
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.VonNeumann);
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.RandomPentagonal);
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.LeftHexagonal);
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.RightHexagonal);
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.RandomHexagonal);
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.Radial);

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
                    StartStop();

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
            }
        }

        public int GrainCellCount => rowCount * columnCount;

        public BindableCollection<BoundaryConditionModel> BoundaryConditions { get; } = new BindableCollection<BoundaryConditionModel>();

        public BoundaryConditionModel SelectedBoundaryCondition { get; set; } = BoundaryConditionModel.Absorbing;

        public BindableCollection<CellNeighborhoodTypeModel> CellNeighborhoods { get; } = new BindableCollection<CellNeighborhoodTypeModel>();

        public CellNeighborhoodTypeModel SelectedCellNeighborhood { get; set; } = CellNeighborhoodTypeModel.VonNeumann;

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
            get => isUniformMethodSelected;
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
            get => isRandomMethodSelected;
            set
            {
                isRandomMethodSelected = value;
                NotifyOfPropertyChange(() => IsRandomMethodSelected);
            }
        }

        public bool IsRandomWithRadiusMethodSelected
        {
            get => isRandomWithRadiusMethodSelected;
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

        public async void Nucleate()
        {
            CanNucleate = false;

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

            CanNucleate = true;
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

        public void Reset()
        {
            StartStop();
            StartStop();
        }

        public bool CanReset => IsStarted;

        public async void StartStop()
        {
            CanStartStop = false;

            if (startStopContent == "Start")
            {
                IsStopped = false;

                await Task.Run(() =>
                {
                    grainCellGrid = new GrainCellGrid2DModel(columnCount, rowCount, SelectedCellNeighborhood, SelectedBoundaryCondition);

                    RunDrawerTask();
                });

                CanContinuePause = true;
                CanShowNextStep = true;
                CanNucleate = true;
                StartStopContent = "Stop";
                ContinuePauseContent = "Continue";
            }
            else if (startStopContent == "Stop")
            {
                evolverAndDrawerDispatcherTimer.Stop();

                CancelTasksAndResetCancellationTokenSource();

                IsStopped = true;
                CanContinuePause = false;
                CanShowNextStep = false;
                CanNucleate = false;
                StartStopContent = "Start";
            }

            CanStartStop = true;
        }

        public bool CanStartStop
        {
            get => canStartStop;
            set
            {
                canStartStop = value;
                NotifyOfPropertyChange(() => CanStartStop);
            }
        }

        public bool IsStopped
        {
            get => isStopped;
            set
            {
                isStopped = value;
                IsStarted = !value;
                NotifyOfPropertyChange(() => IsStopped);
            }
        }

        public bool IsStarted
        {
            get => !isStopped;
            set
            {
                NotifyOfPropertyChange(() => IsStarted);
                NotifyOfPropertyChange(() => CanNucleate);
                NotifyOfPropertyChange(() => CanReset);
            }
        }

        public string StartStopContent
        {
            get => startStopContent;
            set
            {
                startStopContent = value;
                NotifyOfPropertyChange(() => StartStopContent);
            }
        }

        private void CancelTasksAndResetCancellationTokenSource()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }

        public void ContinuePause()
        {
            switch (continuePauseContent)
            {
                case "Continue":
                    CanNucleate = false;
                    evolverAndDrawerDispatcherTimer.Start();
                    ContinuePauseContent = "Pause";
                    break;

                case "Pause":
                    CanNucleate = true;
                    evolverAndDrawerDispatcherTimer.Stop();
                    CancelTasksAndResetCancellationTokenSource();
                    ContinuePauseContent = "Continue";
                    break;
            }
        }

        public bool CanContinuePause
        {
            get => canContinuePause;
            set
            {
                canContinuePause = value;
                NotifyOfPropertyChange(() => CanContinuePause);
            }
        }

        public string ContinuePauseContent
        {
            get => continuePauseContent;
            set
            {
                continuePauseContent = value;
                NotifyOfPropertyChange(() => ContinuePauseContent);
            }
        }

        public async void ShowNextStep()
        {
            CanShowNextStep = false;

            await RunEvolverAndDrawerTask();

            if (!growthIsDone)
                CanShowNextStep = true;
        }

        public bool CanShowNextStep
        {
            get => canShowNextStep;
            set
            {
                canShowNextStep = value;
                NotifyOfPropertyChange(() => CanShowNextStep);
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
                if (!isStopped && grainCellGrid != null && IsClickingOnImageEnabled)
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