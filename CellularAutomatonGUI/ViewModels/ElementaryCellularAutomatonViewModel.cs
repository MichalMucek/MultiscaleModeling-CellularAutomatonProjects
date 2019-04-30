using Caliburn.Micro;
using ElementaryCellularAutomaton.Models;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace CellularAutomatonGUI.ViewModels
{
    [Export(typeof(ElementaryCellularAutomatonViewModel))]
    public class ElementaryCellularAutomatonViewModel : Screen
    {
        private int _size = 100;
        private RuleModel _rule = new RuleModel(30);
        private int _generations = 100;
        private bool _canStart = true;
        private ShellViewModel _shellViewModel;

        public ElementaryCellularAutomatonViewModel(ShellViewModel shellViewModel)
        {
            _shellViewModel = shellViewModel;

            BoundaryConditions.Add(BoundaryConditionModel.OutsideIsDead);
            BoundaryConditions.Add(BoundaryConditionModel.OutsideIsAlive);
            BoundaryConditions.Add(BoundaryConditionModel.Periodical);
        }

        public int Size
        {
            get => _size;
            set
            {
                _size = value;
                NotifyOfPropertyChange(() => Size);
            }
        }

        public int Rule
        {
            get => _rule.Value;
            set
            {
                _rule.Value = value;
                NotifyOfPropertyChange(() => Rule);
            }
        }

        public int Generations
        {
            get => _generations;
            set
            {
                _generations = value;
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
                var cellGrid = new CellGrid1DModel(_size, _rule, SelectedBoundaryCondition);

                for (int i = 2; i <= Generations; i++)
                    cellGrid.Evolve();

                CellGridImageFilename = cellGrid.GenerateImageFileAndGetFilename();
            });

            CanStart = true;
        }

        public bool CanStart
        {
            get => _canStart;
            private set
            {
                _canStart = value;
                NotifyOfPropertyChange(() => CanStart);
            }
        }

        public string CellGridImageFilename
        {
            get => _shellViewModel.CellGridImageFilename;
            private set => _shellViewModel.CellGridImageFilename = value;
        }
    }
}
