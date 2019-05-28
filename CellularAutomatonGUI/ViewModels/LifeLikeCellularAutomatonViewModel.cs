using Caliburn.Micro;
using LifeLikeCellularAutomaton.Models;
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
    public class LifeLikeCellularAutomatonViewModel : Screen
    {
        private int columnCount = 31;
        private int rowCount = 31;
        private CellNeighborhoodTypeModel selectedCellNeighborhood = CellNeighborhoodTypeModel.Moore;
        private BindableCollection<NumberOfCellsForRulesModel> birthRules;
        private NumberOfCellsForRulesModel selectedBirthRule;
        private BindableCollection<NumberOfCellsForRulesModel> birthVonNeumannRulesSafe;
        private BindableCollection<NumberOfCellsForRulesModel> birthMooreRulesSafe;
        private BindableCollection<NumberOfCellsForRulesModel> survivalRules;
        private NumberOfCellsForRulesModel selectedSurvivalRule;
        private BindableCollection<NumberOfCellsForRulesModel> survivalVonNeumannRulesSafe;
        private BindableCollection<NumberOfCellsForRulesModel> survivalMooreRulesSafe;
        private bool canSetOrUnsetCellGrid = true;
        private bool isCellGridUnset = true;
        private bool canContinueOrPauseEvolution = false;
        private string continueOrPauseEvolutionContent = "Start";
        private bool canShowNextEvolutionStep = false;
        private string setOrUnsetCellGridContent = "Set";
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

        public LifeLikeCellularAutomatonViewModel()
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

            cellGrid = new CellGrid2DModel(columnCount, rowCount, selectedCellNeighborhood, mooreRule, SelectedBoundaryCondition);
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
                CreateNewCellGridTask();
                RunDrawerTask();
                NotifyOfPropertyChange(() => ColumnCount);
            }
        }

        public int RowCount
        {
            get => rowCount;
            set
            {
                rowCount = value;
                CreateNewCellGridTask();
                RunDrawerTask();
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

        public void SetOrUnsetCellGrid()
        {
            CanSetOrUnsetCellGrid = false;

            switch (setOrUnsetCellGridContent)
            {
                case "Set":
                    SetCellGrid();
                    RunDrawerTask();
                    break;

                case "Unset":
                    UnsetCellGrid();
                    break;
            }

            CanSetOrUnsetCellGrid = true;
        }

        private async void SetCellGrid()
        {
            await CreateNewCellGridTask();
            IsCellGridUnset = false;

            CellCount = rowCount * columnCount;
            CanContinueOrPauseEvolution = true;
            CanShowNextEvolutionStep = true;
            SetOrUnsetCellGridContent = "Unset";
            ContinueOrPauseEvolutionContent = "Start";
        }

        private Task CreateNewCellGridTask()
        {
            return Task.Run(() =>
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
            });
        }

        private void UnsetCellGrid()
        {
            evolverAndDrawerDispatcherTimer.Stop();

            CancelTasksAndResetCancellationTokenSource();

            IsCellGridUnset = true;
            CanContinueOrPauseEvolution = false;
            CanShowNextEvolutionStep = false;
            SetOrUnsetCellGridContent = "Set";
        }

        public bool CanSetOrUnsetCellGrid
        {
            get => canSetOrUnsetCellGrid;
            set
            {
                canSetOrUnsetCellGrid = value;
                NotifyOfPropertyChange(() => CanSetOrUnsetCellGrid);
            }
        }

        public bool IsCellGridUnset
        {
            get => isCellGridUnset;
            set
            {
                isCellGridUnset = value;
                IsCellGridSet = !value;
                NotifyOfPropertyChange(() => IsCellGridUnset);
            }
        }

        public bool IsCellGridSet
        {
            get => !isCellGridUnset;
            set => NotifyOfPropertyChange(() => IsCellGridSet);
        }

        public string SetOrUnsetCellGridContent
        {
            get => setOrUnsetCellGridContent;
            set
            {
                setOrUnsetCellGridContent = value;
                NotifyOfPropertyChange(() => SetOrUnsetCellGridContent);
            }
        }

        private void CancelTasksAndResetCancellationTokenSource()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }

        public void ContinueOrPauseEvolution()
        {
            switch (continueOrPauseEvolutionContent)
            {
                case "Start":
                case "Continue":
                    ContinueEvolution();
                    break;

                case "Pause":
                    PauseEvolution();
                    break;
            }
        }

        private void ContinueEvolution()
        {
            CanShowNextEvolutionStep = false;
            evolverAndDrawerDispatcherTimer.Start();
            ContinueOrPauseEvolutionContent = "Pause";
        }

        private void PauseEvolution()
        {
            evolverAndDrawerDispatcherTimer.Stop();
            CancelTasksAndResetCancellationTokenSource();
            CanShowNextEvolutionStep = true;
            ContinueOrPauseEvolutionContent = "Continue";
        }

        public bool CanContinueOrPauseEvolution
        {
            get => canContinueOrPauseEvolution;
            set
            {
                canContinueOrPauseEvolution = value;
                NotifyOfPropertyChange(() => CanContinueOrPauseEvolution);
            }
        }

        public string ContinueOrPauseEvolutionContent
        {
            get => continueOrPauseEvolutionContent;
            set
            {
                continueOrPauseEvolutionContent = value;
                NotifyOfPropertyChange(() => ContinueOrPauseEvolutionContent);
            }
        }

        public async void ShowNextEvolutionStep()
        {
            CanShowNextEvolutionStep = false;
            CanContinueOrPauseEvolution = false;
            ContinueOrPauseEvolutionContent = "Continue";

            await RunEvolverAndDrawerTask();

            CanShowNextEvolutionStep = true;
            CanContinueOrPauseEvolution = true;
        }

        public bool CanShowNextEvolutionStep
        {
            get => canShowNextEvolutionStep;
            set
            {
                canShowNextEvolutionStep = value;
                NotifyOfPropertyChange(() => CanShowNextEvolutionStep);
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
                if (!isCellGridUnset && cellGrid != null)
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