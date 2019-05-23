using Caliburn.Micro;
using GameOfLife.Models;
using CellularAutomaton2D.Models;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;

namespace CellularAutomatonGUI.ViewModels
{
    public class GameOfLifeViewModel : Screen
    {
        private int columnCount = 30;
        private int rowCount = 30;
        private CellNeighborhoodTypeModel selectedCellNeighborhood = CellNeighborhoodTypeModel.Moore;
        private BindableCollection<NumberOfCellsForRulesModel> birthRules;
        private NumberOfCellsForRulesModel selectedBirthRule;
        private BindableCollection<NumberOfCellsForRulesModel> birthVonNeumannRulesSafe;
        private BindableCollection<NumberOfCellsForRulesModel> birthMooreRulesSafe;
        private BindableCollection<NumberOfCellsForRulesModel> survivalRules;
        private NumberOfCellsForRulesModel selectedSurvivalRule;
        private BindableCollection<NumberOfCellsForRulesModel> survivalVonNeumannRulesSafe;
        private BindableCollection<NumberOfCellsForRulesModel> survivalMooreRulesSafe;
        private bool canStartStop = true;
        private bool isStopped = true;
        private bool canContinuePause = false;
        private string continuePauseContent = "Continue";
        private bool canShowNextStep = false;
        private string startStopContent = "Start";
        private int timeIntervalInMilliseconds = 100;
        private int cellWidth = 12;
        private int cellHeight = 12;
        private int lineWidth = 1;
        private int cellCount = 1;
        private int randomCellsToSetAliveCount = 1;

        private DispatcherTimer evolverAndDrawerDispatcherTimer;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        private RuleModel vonNeumannRule = new RuleModel(CellNeighborhoodTypeModel.VonNeumann);
        private RuleModel mooreRule = new RuleModel(CellNeighborhoodTypeModel.Moore);
        private CellGrid2DModel cellGrid;

        public GameOfLifeViewModel()
        {
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.Moore);
            CellNeighborhoods.Add(CellNeighborhoodTypeModel.VonNeumann);

            birthVonNeumannRulesSafe = new BindableCollection<NumberOfCellsForRulesModel>(vonNeumannRule.Birth.ToList());
            birthMooreRulesSafe = new BindableCollection<NumberOfCellsForRulesModel>(mooreRule.Birth.ToList());

            survivalVonNeumannRulesSafe = new BindableCollection<NumberOfCellsForRulesModel>(vonNeumannRule.Survival.ToList());
            survivalMooreRulesSafe = new BindableCollection<NumberOfCellsForRulesModel>(mooreRule.Survival.ToList());

            birthRules = birthMooreRulesSafe;
            survivalRules = survivalMooreRulesSafe;

            birthRules[3].Chosen = true;
            SurvivalRules[2].Chosen = true;
            SurvivalRules[3].Chosen = true;

            SelectedBirthRule = birthRules[0];
            SelectedSurvivalRule = survivalRules[0];

            BoundaryConditions.Add(BoundaryConditionModel.Absorbing);
            BoundaryConditions.Add(BoundaryConditionModel.CounterAbsorbing);
            BoundaryConditions.Add(BoundaryConditionModel.Periodic);

            evolverAndDrawerDispatcherTimer = new DispatcherTimer();
            evolverAndDrawerDispatcherTimer.Interval = TimeSpan.FromMilliseconds(timeIntervalInMilliseconds);
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
                cellGrid.Evolve();

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        CellGridBitmapImage = cellGrid?.GetBitmapImage(cellWidth, cellHeight, lineWidth)
                    );
            }, cancellationToken);
        }

        public int ColumnCount
        {
            get => columnCount;
            set
            {
                columnCount = value;
                NotifyOfPropertyChange(() => ColumnCount);
            }
        }

        public int RowCount
        {
            get => rowCount;
            set
            {
                rowCount = value;
                NotifyOfPropertyChange(() => RowCount);
            }
        }

        public BindableCollection<CellNeighborhoodTypeModel> CellNeighborhoods { get; } = new BindableCollection<CellNeighborhoodTypeModel>();

        public CellNeighborhoodTypeModel SelectedCellNeighborhood
        {
            get => selectedCellNeighborhood;
            set
            {
                selectedCellNeighborhood = value;

                switch (selectedCellNeighborhood)
                {
                    case CellNeighborhoodTypeModel.VonNeumann:
                        BirthRules = birthVonNeumannRulesSafe;
                        SurvivalRules = survivalVonNeumannRulesSafe;
                        break;

                    case CellNeighborhoodTypeModel.Moore:
                        BirthRules = birthMooreRulesSafe;
                        SurvivalRules = survivalMooreRulesSafe;
                        break;

                    default:
                        break;
                }

                SelectedBirthRule = BirthRules[0];
                SelectedSurvivalRule = SurvivalRules[0];
            }
        }

        public BindableCollection<NumberOfCellsForRulesModel> BirthRules
        {
            get => birthRules;
            set
            {
                birthRules = value;
                NotifyOfPropertyChange(() => BirthRules);
            }
        }

        public NumberOfCellsForRulesModel SelectedBirthRule
        {
            get => selectedBirthRule;
            set
            {
                selectedBirthRule = value;
                NotifyOfPropertyChange(() => SelectedBirthRule);
            }
        }

        public BindableCollection<NumberOfCellsForRulesModel> SurvivalRules
        {
            get => survivalRules;
            set
            {
                survivalRules = value;
                NotifyOfPropertyChange(() => SurvivalRules);
            }
        }

        public NumberOfCellsForRulesModel SelectedSurvivalRule
        {
            get => selectedSurvivalRule;
            set
            {
                selectedSurvivalRule = value;
                NotifyOfPropertyChange(() => SelectedSurvivalRule);
            }
        }

        public BindableCollection<BoundaryConditionModel> BoundaryConditions { get; } = new BindableCollection<BoundaryConditionModel>();

        public BoundaryConditionModel SelectedBoundaryCondition { get; set; } = BoundaryConditionModel.Absorbing;

        public async void StartStop()
        {
            CanStartStop = false;

            if (startStopContent == "Start")
            {
                IsStopped = false;

                await Task.Run(() =>
                {
                    switch (SelectedCellNeighborhood)
                    {
                        case CellNeighborhoodTypeModel.VonNeumann:
                            cellGrid = new CellGrid2DModel(ColumnCount, RowCount, SelectedCellNeighborhood, vonNeumannRule, SelectedBoundaryCondition);
                            break;

                        case CellNeighborhoodTypeModel.Moore:
                            cellGrid = new CellGrid2DModel(ColumnCount, RowCount, SelectedCellNeighborhood, mooreRule, SelectedBoundaryCondition);
                            break;
                    }

                    RunDrawerTask();
                });

                CellCount = RowCount * ColumnCount;
                CanContinuePause = true;
                CanShowNextStep = true;
                StartStopContent = "Stop";
                ContinuePauseContent = "Continue";
            }
            else
            {
                evolverAndDrawerDispatcherTimer.Stop();

                CancelTasksAndResetCancellationTokenSource();

                IsStopped = true;
                CanContinuePause = false;
                CanShowNextStep = false;
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
            set => NotifyOfPropertyChange(() => IsStarted);
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
                    evolverAndDrawerDispatcherTimer.Start();
                    ContinuePauseContent = "Pause";
                    break;

                case "Pause":
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

        public int TimeIntervalInMilliseconds
        {
            get => timeIntervalInMilliseconds;
            set
            {
                timeIntervalInMilliseconds = value;
                evolverAndDrawerDispatcherTimer.Interval = TimeSpan.FromMilliseconds(value);
                NotifyOfPropertyChange(() => TimeIntervalInMilliseconds);
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
                    CellGridBitmapImage = cellGrid?.GetBitmapImage(cellWidth, cellHeight, lineWidth)
                )
            );
        }

        private BitmapImage cellGridBitmapImage;

        public BitmapImage CellGridBitmapImage
        {
            get => cellGridBitmapImage;
            set
            {
                cellGridBitmapImage = value;
                NotifyOfPropertyChange(() => CellGridBitmapImage);
            }
        }

        public async void NegateCellState(object sender, MouseEventArgs e)
        {
            var imageControl = (Image)sender;

            await Task.Run(() =>
            {
                if (!isStopped && cellGrid != null)
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
                            X = (int)((cellGridBitmapImage.PixelWidth / (double)imageControlSize.Width) * mousePositionOverCellGridImageControl.X),
                            Y = (int)((cellGridBitmapImage.PixelHeight / (double)imageControlSize.Height) * mousePositionOverCellGridImageControl.Y)
                        };

                        cellGrid.NegateCellState(mousePositionOverCellGridBitmapImage);
                    });
                }
            });

            RunDrawerTask();
        }

        public int CellCount
        {
            get => cellCount;
            set
            {
                cellCount = value;
                NotifyOfPropertyChange(() => CellCount);
            }
        }

        public async void ClearCellGridImage()
        {
            await Task.Run(() => cellGrid?.KillAll());
            RunDrawerTask();
        }

        public async void Randomize()
        {
            await Task.Run(() => cellGrid?.Randomize(randomCellsToSetAliveCount));
            RunDrawerTask();
        }

        public int RandomCellsToSetAliveCount
        {
            get => randomCellsToSetAliveCount;
            set
            {
                randomCellsToSetAliveCount = value;
                NotifyOfPropertyChange(() => RandomCellsToSetAliveCount);
            }
        }

        public async void PutBeehiveInTheMiddle()
        {
            await Task.Run(() => cellGrid?.PutBeehiveInTheMiddle());
            RunDrawerTask();
        }

        public async void PutGliderInTheMiddle()
        {
            await Task.Run(() => cellGrid?.PutGliderInTheMiddle());
            RunDrawerTask();
        }

        public async void PutBlinkerInTheMiddle()
        {
            await Task.Run(() => cellGrid?.PutBlinkerInTheMiddle());
            RunDrawerTask();
        }
    }
}