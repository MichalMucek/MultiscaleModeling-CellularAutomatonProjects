using Caliburn.Micro;
using GameOfLife.Models;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BoundaryConditionModel = ElementaryCellularAutomaton.Models.BoundaryConditionModel;
using Image = System.Windows.Controls.Image;

namespace CellularAutomatonGUI.ViewModels
{
    public class GameOfLifeViewModel : Screen
    {
        private int columnCount = 30;
        private int rowCount = 30;
        private CellsNeighborhoodTypeModel selectedCellsNeighborhood = CellsNeighborhoodTypeModel.Moore;
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
        private int timeInterval = 75;
        private int cellWidth = 12;
        private int cellHeight = 12;
        private int lineWidth = 2;
        private int cellCount = 1;
        private int randomCellsToSetAliveCount = 1;

        private DispatcherTimer evolverAndDrawerDispatcherTimer;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        private RuleModel vonNeumannRule = new RuleModel(CellsNeighborhoodTypeModel.VonNeumann);
        private RuleModel mooreRule = new RuleModel(CellsNeighborhoodTypeModel.Moore);
        private CellGrid2DModel cellGrid;

        public GameOfLifeViewModel()
        {
            CellsNeighborhoods.Add(CellsNeighborhoodTypeModel.Moore);
            CellsNeighborhoods.Add(CellsNeighborhoodTypeModel.VonNeumann);

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

            BoundaryConditions.Add(BoundaryConditionModel.OutsideIsDead);
            BoundaryConditions.Add(BoundaryConditionModel.OutsideIsAlive);
            BoundaryConditions.Add(BoundaryConditionModel.Periodical);

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

        public BindableCollection<CellsNeighborhoodTypeModel> CellsNeighborhoods { get; } = new BindableCollection<CellsNeighborhoodTypeModel>();

        public CellsNeighborhoodTypeModel SelectedCellsNeighborhood
        {
            get => selectedCellsNeighborhood;
            set
            {
                selectedCellsNeighborhood = value;

                switch (selectedCellsNeighborhood)
                {
                    case CellsNeighborhoodTypeModel.VonNeumann:
                        BirthRules = birthVonNeumannRulesSafe;
                        SurvivalRules = survivalVonNeumannRulesSafe;
                        break;

                    case CellsNeighborhoodTypeModel.Moore:
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

        public BoundaryConditionModel SelectedBoundaryCondition { get; set; } = BoundaryConditionModel.OutsideIsDead;

        public async void StartStop()
        {
            CanStartStop = false;

            if (startStopContent == "Start")
            {
                IsStopped = false;

                await Task.Run(() =>
                {
                    switch (SelectedCellsNeighborhood)
                    {
                        case CellsNeighborhoodTypeModel.VonNeumann:
                            cellGrid = new CellGrid2DModel(ColumnCount, RowCount, SelectedCellsNeighborhood, vonNeumannRule, SelectedBoundaryCondition);
                            break;

                        case CellsNeighborhoodTypeModel.Moore:
                            cellGrid = new CellGrid2DModel(ColumnCount, RowCount, SelectedCellsNeighborhood, mooreRule, SelectedBoundaryCondition);
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

        public void NegateCellState(object sender, MouseEventArgs e)
        {
            if (!isStopped && cellGrid != null)
            {
                Point mousePositionOverCellGridImage = new Point
                {
                    X = (int)e.GetPosition((Image)sender).X,
                    Y = (int)e.GetPosition((Image)sender).Y
                };

                cellGrid?.NegateCellState(mousePositionOverCellGridImage);
                RunDrawerTask();
            }
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

        public void ClearCellGridImage()
        {
            cellGrid?.KillAll();
            RunDrawerTask();
        }

        public void Randomize()
        {
            cellGrid?.Randomize(randomCellsToSetAliveCount);
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

        public void PutBeehiveInTheMiddle()
        {
            cellGrid?.PutBeehiveInTheMiddle();
            RunDrawerTask();
        }

        public void PutGliderInTheMiddle()
        {
            cellGrid?.PutGliderInTheMiddle();
            RunDrawerTask();
        }

        public void PutBlinkerInTheMiddle()
        {
            cellGrid?.PutBlinkerInTheMiddle();
            RunDrawerTask();
        }
    }
}