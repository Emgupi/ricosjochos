using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PosPia.Modelos;
using PosPia;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Punto_de_venta;
using System.Text.RegularExpressions; 

namespace Punto_de_venta
{
    public sealed partial class ClientesWindow : Window
    {
        private Cliente? _selectedCliente;
        private ObservableCollection<Cliente> _clientesCollection;
        private User _currentUser;
        private bool _isLoaded = false;


        private static readonly Regex _regexNombre = new Regex(@"^[a-zA-Z\sñÑáéíóúÁÉÍÓÚ]+$");
        // Telefono: Solo 10 dígitos numéricos (exactamente)
        private static readonly Regex _regexTelefono = new Regex(@"^\d{10}$");
        // Teléfono (para "Mientras Escribes"): Solo permite números
        private static readonly Regex _regexSoloNumeros = new Regex("^[0-9]*$");
        // RFC: 12 (moral) o 13 (física) caracteres, solo letras y números.
        private static readonly Regex _regexRFC = new Regex(@"^[a-zA-Z0-9]{12,13}$");
        // RFC (para "Mientras Escribes"): Solo permite letras y números
        private static readonly Regex _regexSoloAlfanumerico = new Regex("^[a-zA-Z0-9]*$");


        public ClientesWindow(User user)
        {
            this.InitializeComponent();
            _currentUser = user;
            this.Title = "Gestión de Clientes";

            _clientesCollection = new ObservableCollection<Cliente>();
            ClientesGrid.ItemsSource = _clientesCollection;
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                CargarClientes();
            }
        }

        private void CargarClientes()
        {
            try
            {
                var clientesDesdeDB = DatabaseHelper.GetAllClientes();
                _clientesCollection.Clear();
                foreach (var cliente in clientesDesdeDB)
                {
                    _clientesCollection.Add(cliente);
                }
            }
            catch (Exception ex)
            {
                ShowDialog("Error de Base de Datos", $"No se pudieron cargar los clientes: {ex.Message}");
            }
        }

        private void ClientesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedCliente = ClientesGrid.SelectedItem as Cliente;

            if (_selectedCliente != null)
            {
                TxtNombre.Text = _selectedCliente.Nombre;
                TxtEmpresa.Text = _selectedCliente.Empresa;
                TxtTelefono.Text = _selectedCliente.Telefono;
                TxtRFC.Text = _selectedCliente.RFC;
                TxtDireccion.Text = _selectedCliente.Direccion;
            }
            else
            {
                OnLimpiarClick(null, null);
            }
        }

        private void OnAgregarClick(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos(out string errorMsg))
            {
                ShowDialog("Error de Validación", errorMsg);
                return;
            }

            try
            {
                var nuevoCliente = new Cliente
                {
                    Nombre = TxtNombre.Text.Trim(),
                    Empresa = TxtEmpresa.Text.Trim(),
                    RFC = TxtRFC.Text.Trim().ToUpper(),
                    Telefono = TxtTelefono.Text.Trim(),
                    Direccion = TxtDireccion.Text.Trim()
                };

                nuevoCliente = DatabaseHelper.AgregarCliente(nuevoCliente);

                CargarClientes();

                ShowDialog("Éxito", $"Cliente '{nuevoCliente.Nombre}' agregado correctamente.");
                OnLimpiarClick(null, null);
            }
            catch (Exception ex)
            {
                ShowDialog("Error al Agregar", $"No se pudo guardar el cliente: {ex.Message}");
            }
        }

        private void OnModificarClick(object sender, RoutedEventArgs e)
        {
            if (_selectedCliente == null)
            {
                ShowDialog("Error", "Debe seleccionar un cliente para modificar.");
                return;
            }

            if (!ValidarCampos(out string errorMsg))
            {
                ShowDialog("Error de Validación", errorMsg);
                return;
            }

            try
            {
                string nombreCliente = _selectedCliente.Nombre;

                _selectedCliente.Nombre = TxtNombre.Text.Trim();
                _selectedCliente.Empresa = TxtEmpresa.Text.Trim();
                _selectedCliente.RFC = TxtRFC.Text.Trim().ToUpper();
                _selectedCliente.Telefono = TxtTelefono.Text.Trim();
                _selectedCliente.Direccion = TxtDireccion.Text.Trim();

                DatabaseHelper.ActualizarCliente(_selectedCliente);

                CargarClientes();

                ShowDialog("Éxito", $"Cliente '{nombreCliente}' modificado correctamente.");
                OnLimpiarClick(null, null);
            }
            catch (Exception ex)
            {
                ShowDialog("Error al Modificar", $"No se pudo actualizar el cliente: {ex.Message}");
            }
        }

        private async void OnEliminarClick(object sender, RoutedEventArgs e)
        {
            if (_selectedCliente == null)
            {
                ShowDialog("Error", "Debe seleccionar un cliente para eliminar.");
                return;
            }

            string nombreCliente = _selectedCliente.Nombre;

            ContentDialog confirmDialog = new ContentDialog
            {
                Title = "¿Eliminar Cliente?",
                Content = $"¿Estás seguro de que deseas eliminar a '{nombreCliente}'?\nEsta acción no se puede deshacer.",
                PrimaryButtonText = "Eliminar",
                CloseButtonText = "Cancelar",
                XamlRoot = RootGrid.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };

            ContentDialogResult result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    DatabaseHelper.EliminarCliente(_selectedCliente.IdCliente);
                    CargarClientes();
                    ShowDialog("Éxito", $"Cliente '{nombreCliente}' eliminado.");
                    OnLimpiarClick(null, null);
                }
                catch (Exception ex)
                {
                    ShowDialog("Error al Eliminar", $"No se pudo eliminar el cliente. Es posible que esté asociado a una venta.\n\nError: {ex.Message}");
                }
            }
        }


        private void OnRegresarClick(object sender, RoutedEventArgs e)
        {
            Menu_principal menu = new Menu_principal(_currentUser);
            menu.Activate();
            this.Close();
        }

        private void OnLimpiarClick(object? sender, RoutedEventArgs? e)
        {
            TxtNombre.Text = string.Empty;
            TxtEmpresa.Text = string.Empty;
            TxtTelefono.Text = string.Empty;
            TxtRFC.Text = string.Empty;
            TxtDireccion.Text = string.Empty;

            ClientesGrid.SelectedItem = null;
            _selectedCliente = null;
        }


        private bool ValidarCampos(out string errorMsg)
        {
            errorMsg = string.Empty;
            var nombre = TxtNombre.Text.Trim();
            var rfc = TxtRFC.Text.Trim();
            var telefono = TxtTelefono.Text.Trim();

            // 1. Campos obligatorios
            if (string.IsNullOrWhiteSpace(nombre))
            {
                errorMsg = "El campo 'Nombre' es obligatorio.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(rfc))
            {
                errorMsg = "El campo 'RFC' es obligatorio.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(telefono) && !_regexTelefono.IsMatch(telefono))
            {
                errorMsg = "El 'Teléfono' debe tener exactamente 10 dígitos numéricos (sin espacios ni símbolos).";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(nombre) && !_regexNombre.IsMatch(nombre))
            {
                errorMsg = "El 'Nombre' solo puede contener letras y espacios.";
                return false;
            }

            if (!_regexRFC.IsMatch(rfc))
            {
                errorMsg = "El 'RFC' debe tener 12 o 13 caracteres (solo letras y números, sin guiones ni espacios).";
                return false;
            }

            return true;
        }


        private void TxtNombre_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (args.NewText.Any(char.IsDigit))
            {
                args.Cancel = true;
            }
        }

        private void TxtTelefono_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (!_regexSoloNumeros.IsMatch(args.NewText))
            {
                args.Cancel = true;
            }
        }

        private void TxtRFC_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (!_regexSoloAlfanumerico.IsMatch(args.NewText))
            {
                args.Cancel = true; 
            }

        }


        private async void ShowDialog(string title, string content)
        {
            if (this.Content == null) return;

            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "Aceptar",
                XamlRoot = RootGrid.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };
            await dialog.ShowAsync();
        }
    }
}
