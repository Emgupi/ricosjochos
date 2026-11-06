using Microsoft.UI.Xaml;
using PosPia;

namespace Punto_de_venta
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            DatabaseHelper.InitializeDatabase();
            DatabaseHelper.SeedInitialData();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var login = new Login();
            login.Activate();
        }
    }
}
