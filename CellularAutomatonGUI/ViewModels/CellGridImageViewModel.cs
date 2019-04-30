using Caliburn.Micro;
using System.ComponentModel.Composition;

namespace CellularAutomatonGUI.ViewModels
{
    [Export(typeof(CellGridImageViewModel))]
    public class CellGridImageViewModel : Screen
    {
        private static string _cellGridImageFilename;

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
