using Caliburn.Micro;
using CellularAutomatonGUI.ViewModels;
using System.Windows;

namespace CellularAutomatonGUI
{
    class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
