using Caliburn.Micro;
using BoundaryConditionModel = ElementaryCellularAutomaton.Models.BoundaryConditionModel;
using GameOfLife.Models;
using System.Linq;

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
        private int timeInterval = 500;
        private ShellViewModel shellViewModel;

        RuleModel vonNeumannRule = new RuleModel(CellsNeighborhoodTypeModel.VonNeumann);
        RuleModel mooreRule = new RuleModel(CellsNeighborhoodTypeModel.Moore);

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
            get  => birthRules;
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

        public int TimeInterval
        {
            get => timeInterval;
            set
            {
                timeInterval = value;
                NotifyOfPropertyChange(() => TimeInterval);
            }
        }
    }
}
