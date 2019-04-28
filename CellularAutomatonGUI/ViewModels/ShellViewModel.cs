using Caliburn.Micro;
using ElementaryCellularAutomaton.Models;
using System.Threading.Tasks;

namespace CellularAutomatonGUI.ViewModels
{
    public class ShellViewModel : Screen
    {
        private int _size = 255;
        private RuleModel _rule = new RuleModel(30);
        private int _generations = 300;
        private BindableCollection<BoundaryConditionModel> _boundaryConditions = new BindableCollection<BoundaryConditionModel>();
        private BoundaryConditionModel _selectedBoundaryCondition = BoundaryConditionModel.OutsideIsDead;
        private string _cellGridImageFilename;
        private bool _canStart = true;


        public ShellViewModel()
        {
            _boundaryConditions.Add(BoundaryConditionModel.OutsideIsDead);
            _boundaryConditions.Add(BoundaryConditionModel.OutsideIsAlive);
            _boundaryConditions.Add(BoundaryConditionModel.Periodical);
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

        public BindableCollection<BoundaryConditionModel> BoundaryConditions => _boundaryConditions;

        public BoundaryConditionModel SelectedBoundaryCondition
        {
            get => _selectedBoundaryCondition;
            set => _selectedBoundaryCondition = value;
        }

        public bool CanStart
        {
            get => _canStart;
            set
            {
                _canStart = value;
                NotifyOfPropertyChange(() => CanStart);
            }
        }

        public async void Start()
        {
            CanStart = false;

            await Task.Run(() =>
            {
                var cellGrid = new CellGrid1DModel(_size, _rule, _selectedBoundaryCondition);

                for (int i = 2; i <= Generations; i++)
                    cellGrid.Evolve();

                CellGridImageFilename = cellGrid.GenerateImageFileAndGetFilename();
            });

            CanStart = true;
        }

        public string CellGridImageFilename
        {
            get => _cellGridImageFilename;
            set
            {
                _cellGridImageFilename = value;
                NotifyOfPropertyChange(() => CellGridImageFilename);
            }
        }
    }
}
