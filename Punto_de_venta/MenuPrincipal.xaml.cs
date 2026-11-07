using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using Windows.Graphics;
using PosPia;

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

        private async void OnAperturaCajaClick(object sender, RoutedEventArgs e)
        {
            var monto = new TextBox
            {
                PlaceholderText = "Fondo inicial (ej. 500)",
                InputScope = new InputScope { Names = { new InputScopeName(InputScopeNameValue.Number) } }
            };

            var panel = new StackPanel { Spacing = 10 };
            panel.Children.Add(new TextBlock
            {
                Text = "Captura el efectivo con el que abres la caja hoy.",
                TextWrapping = TextWrapping.Wrap
            });
            panel.Children.Add(monto);

            var dialog = new ContentDialog
            {
                Title = "Apertura de caja",
                Content = panel,
                PrimaryButtonText = "Guardar fondo",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Light
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
                return;

            if (!double.TryParse(monto.Text, out double fondo) || fondo < 0)
            {
                var error = new ContentDialog
                {
                    Title = "Monto no válido",
                    Content = "Escribe un número mayor o igual a 0.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.Content.XamlRoot,
                    RequestedTheme = ElementTheme.Light
                };
                await error.ShowAsync();
                return;
            }

            if (!DatabaseHelper.GuardarFondoDelDia(fondo))
            {
                var abierta = new ContentDialog
                {
                    Title = "Caja ya abierta",
                    Content = "Ya hay una apertura activa. Primero haz el corte de caja.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.Content.XamlRoot,
                    RequestedTheme = ElementTheme.Light
                };
                await abierta.ShowAsync();
            }
        }

        private async void OnRealizarVentaClick(object sender, RoutedEventArgs e)
        {
            if (!DatabaseHelper.HayAperturaHoy())
            {
                var aviso = new ContentDialog
                {
                    Title = "Caja sin abrir",
                    Content = "Primero haz la apertura de caja y captura el fondo.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.Content.XamlRoot,
                    RequestedTheme = ElementTheme.Light
                };
                await aviso.ShowAsync();
                return;
            }

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
