using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Graphics;
using PosPia;
using Windows.ApplicationModel.VoiceCommands;

namespace Punto_de_venta
{
    public sealed partial class Menu_principal : Window
    {
        private User _currentUser;

        public Menu_principal(User user)
        {
            this.InitializeComponent();
            _currentUser = user;
            SetWindowSize(800, 700);
            this.Title = $"Ricos Jochos - {_currentUser.Username}";
            ApplyRoleRestrictions();
        }

        private void SetWindowSize(int width, int height)
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new SizeInt32(width, height));
        }
        private void ApplyRoleRestrictions()
        {
            RoleTextBlock.Text = $"Sesión: {_currentUser.Username}  -  {_currentUser.Role}";

            InventarioButton.Visibility = _currentUser.Role == "Gerente"
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void OnRealizarVentaClick(object sender, RoutedEventArgs e)
        {
            var ventanaLibreria = new PosPia.MainWindow();
            ventanaLibreria.Activate();
        }

        private void OnGestionInventarioClick(object sender, RoutedEventArgs e)
        {
            InventarioWindow inventarioWindow = new InventarioWindow(_currentUser);
            inventarioWindow.Activate();
            this.Close();
        }

        private void OnCatalogoClientesClick(object sender, RoutedEventArgs e)
        {
            ClientesWindow clientesWindow = new ClientesWindow(_currentUser);
            clientesWindow.Activate();
            this.Close();
        }

        private async void OnCerrarSesionClick(object sender, RoutedEventArgs e)
        {
            ContentDialog confirmDialog = new ContentDialog
            {
                Title = "¿Cerrar Sesión?",
                Content = "¿Estás seguro de que deseas cerrar la sesión y volver a la pantalla de inicio?",
                PrimaryButtonText = "Cerrar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };
            ContentDialogResult result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Login loginWindow = new Login();
                loginWindow.Activate();
                this.Close();
            }
        }
    }
}
