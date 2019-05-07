using Caliburn.Micro;
using ElementaryCellularAutomaton.Models;
using System.Threading.Tasks;

namespace CellularAutomatonGUI.ViewModels
{
    public class ElementaryCellularAutomatonViewModel : Screen
    {
        private int size = 100;
        private RuleModel rule = new RuleModel(30);
        private int generations = 100;
        private bool canStart = true;
        public CellGridImageViewModel CellGridImageViewModel { get; }

        public ElementaryCellularAutomatonViewModel()
        {
            CellGridImageViewModel = new CellGridImageViewModel();

            BoundaryConditions.Add(BoundaryConditionModel.OutsideIsDead);
            BoundaryConditions.Add(BoundaryConditionModel.OutsideIsAlive);
            BoundaryConditions.Add(BoundaryConditionModel.Periodic);
        }

        public int Size
        {
            get => size;
            set
            {
                size = value;
                NotifyOfPropertyChange(() => Size);
            }
        }

        public int Rule
        {
            get => rule.Value;
            set
            {
                rule.Value = value;
                NotifyOfPropertyChange(() => Rule);
            }
        }

        public int Generations
        {
            get => generations;
            set
            {
                generations = value;
                NotifyOfPropertyChange(() => Generations);
            }
        }

        public BindableCollection<BoundaryConditionModel> BoundaryConditions { get; } = new BindableCollection<BoundaryConditionModel>();

        public BoundaryConditionModel SelectedBoundaryCondition { get; set; } = BoundaryConditionModel.OutsideIsDead;

        public async void Start()
        {
            CanStart = false;

            await Task.Run(() =>
            {
                var cellGrid = new CellGrid1DModel(size, rule, SelectedBoundaryCondition);

                for (int i = 2; i <= Generations; i++)
                    cellGrid.Evolve();

                CellGridImageViewModel.BitmapImage = cellGrid.GetBitmapImage();
            });

            CanStart = true;
        }

        public bool CanStart
        {
            get => canStart;
            private set
            {
                canStart = value;
                NotifyOfPropertyChange(() => CanStart);
            }
        }
    }
}