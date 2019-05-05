using Caliburn.Micro;
using GameOfLife.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BoundaryConditionModel = ElementaryCellularAutomaton.Models.BoundaryConditionModel;

namespace CellularAutomatonGUI.ViewModels
{
    public class GameOfLifeViewModel : Screen
    {
        private int columnCount = 30;
        private int rowCount = 30;
        private BindableCollection<CellsNeighborhoodTypeModel> cellsNeighborhoods = new BindableCollection<CellsNeighborhoodTypeModel>();
        private CellsNeighborhoodTypeModel selectedCellsNeighborhood = CellsNeighborhoodTypeModel.Moore;
        private BindableCollection<NumberOfCellsForRulesModel> birthRules;
        private NumberOfCellsForRulesModel selectedBirthRule;
        private BindableCollection<NumberOfCellsForRulesModel> birthVonNeumannRulesSafe;
        private BindableCollection<NumberOfCellsForRulesModel> birthMooreRulesSafe;
        private BindableCollection<NumberOfCellsForRulesModel> survivalRules;
        private NumberOfCellsForRulesModel selectedSurvivalRule;
        private BindableCollection<NumberOfCellsForRulesModel> survivalVonNeumannRulesSafe;
        private BindableCollection<NumberOfCellsForRulesModel> survivalMooreRulesSafe;
        private BindableCollection<BoundaryConditionModel> boundaryConditions = new BindableCollection<BoundaryConditionModel>();
        private BoundaryConditionModel selectedBoundaryCondition = BoundaryConditionModel.OutsideIsDead;
        private bool canStartStop = true;
        private bool canContinuePause = false;
        private string continuePauseContent = "Continue";
        private bool canShowNextStep = false;
        private string startStopContent = "Start";
        private int timeInterval = 500;
        private bool canTimeInterval = true;
        private int cellWidth = 12;
        private int cellHeight = 12;
        private int lineWidth = 2;
        private DispatcherTimer evolverAndDrawerDispatcherTimer;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;
        private ShellViewModel shellViewModel;

        private RuleModel vonNeumannRule = new RuleModel(CellsNeighborhoodTypeModel.VonNeumann);
        private RuleModel mooreRule = new RuleModel(CellsNeighborhoodTypeModel.Moore);
        private CellGrid2DModel cellGrid;

        public GameOfLifeViewModel(ShellViewModel shellViewModel)
        {
            this.shellViewModel = shellViewModel;

            cellsNeighborhoods.Add(CellsNeighborhoodTypeModel.Moore);
            cellsNeighborhoods.Add(CellsNeighborhoodTypeModel.VonNeumann);

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

            boundaryConditions.Add(BoundaryConditionModel.OutsideIsDead);
            boundaryConditions.Add(BoundaryConditionModel.OutsideIsAlive);
            boundaryConditions.Add(BoundaryConditionModel.Periodical);

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

                Application.Current.Dispatcher.Invoke(() =>
                        shellViewModel.CellGridBitmapImage = cellGrid?.GetBitmapImage(cellWidth, cellHeight, lineWidth)
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

        public BindableCollection<CellsNeighborhoodTypeModel> CellsNeighborhoods => cellsNeighborhoods;

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

        public BindableCollection<BoundaryConditionModel> BoundaryConditions => boundaryConditions;

        public BoundaryConditionModel SelectedBoundaryCondition
        {
            get => selectedBoundaryCondition;
            set => selectedBoundaryCondition = value;
        }

        public async void StartStop()
        {
            CanStartStop = false;

            if (startStopContent == "Start")
            {
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

                CanContinuePause = true;
                CanShowNextStep = true;
                StartStopContent = "Stop";
                ContinuePauseContent = "Continue";
            }
            else
            {
                evolverAndDrawerDispatcherTimer.Stop();

                CancelTasksAndResetCancellationTokenSource();

                CanTimeInterval = true;
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
                    CanTimeInterval = false;
                    evolverAndDrawerDispatcherTimer.Start();
                    ContinuePauseContent = "Pause";
                    break;

                case "Pause":
                    evolverAndDrawerDispatcherTimer.Stop();
                    CancelTasksAndResetCancellationTokenSource();
                    CanTimeInterval = true;
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

        public bool CanTimeInterval
        {
            get => canTimeInterval;
            set
            {
                canTimeInterval = value;
                NotifyOfPropertyChange(() => CanTimeInterval);
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
                Application.Current.Dispatcher.Invoke(() =>
                    shellViewModel.CellGridBitmapImage = cellGrid?.GetBitmapImage(cellWidth, cellHeight, lineWidth)
                )
            );
        }
    }
}