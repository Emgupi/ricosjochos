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
    public sealed partial class InventarioWindow : Window
    {
        private Ingrediente? _selectedIngrediente;
        private ObservableCollection<Ingrediente> _ingredientesCollection;
        private User _currentUser;

        private static readonly Regex _regexNombre = new Regex(@"^[a-zA-Z\sñÑáéíóúÁÉÍÓÚ]+$");
        private static readonly Regex _regexStock = new Regex(@"^[0-9]*\.?[0-9]*$");

        public InventarioWindow(User user)
        {
            this.InitializeComponent();
            _currentUser = user;
            this.Title = "Gestión de Inventario (Materia Prima)";

            _ingredientesCollection = new ObservableCollection<Ingrediente>();
            MateriaPrimaGrid.ItemsSource = _ingredientesCollection;

        }


        private bool _isLoaded = false;
        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                CargarIngredientes();
            }
        }

        private void CargarIngredientes()
        {
            try
            {
                var ingredientesDesdeDB = DatabaseHelper.GetAllIngredientes();
                _ingredientesCollection.Clear();
                foreach (var ingrediente in ingredientesDesdeDB)
                {
                    _ingredientesCollection.Add(ingrediente);
                }
            }
            catch (Exception ex)
            {
                ShowDialog("Error de Base de Datos", $"No se pudieron cargar los ingredientes: {ex.Message}");
            }
        }

        private void MateriaPrimaGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedIngrediente = MateriaPrimaGrid.SelectedItem as Ingrediente;

            if (_selectedIngrediente != null)
            {
                TxtNombre.Text = _selectedIngrediente.Nombre;
                CmbUnidad.SelectedItem = _selectedIngrediente.UnidadMedida;
                TxtStock.Text = _selectedIngrediente.Stock.ToString("N5");
            }
            else
            {
                OnLimpiarClick(null, null);
            }
        }

        private void OnAgregarClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNombre.Text) || string.IsNullOrWhiteSpace(TxtStock.Text) || CmbUnidad.SelectedItem == null)
            {
                ShowDialog("Error de Validación", "Todos los campos (Nombre, Cantidad Stock y Unidad) son obligatorios.");
                return;
            }

            var nombre = TxtNombre.Text.Trim();
            var unidad = CmbUnidad.SelectedItem.ToString() ?? "pz";

            if (!_regexNombre.IsMatch(nombre))
            {
                ShowDialog("Error de Formato", "El campo 'Nombre' solo puede contener letras y espacios.");
                return;
            }

            if (!double.TryParse(TxtStock.Text, out double stock))
            {
                ShowDialog("Error de Formato", "La cantidad de 'Stock' debe ser un número válido.");
                return;
            }

            if (stock <= 0)
            {
                ShowDialog("Error de Validación", "La cantidad de 'Stock' debe ser mayor a 0.");
                return;
            }

            double maxStock;
            switch (unidad)
            {
                case "kg":
                case "lt":
                    maxStock = 100;
                    break;
                case "g":
                case "ml":
                    maxStock = 100000;
                    break;
                case "pz":
                    maxStock = 1000;
                    break;
                default:
                    maxStock = 1000;
                    break;
            }

            if (stock > maxStock)
            {
                ShowDialog("Error de Límite", $"La cantidad para '{unidad}' no puede ser mayor a {maxStock}.");
                return;
            }

            try
            {
                var nuevoIngrediente = new Ingrediente
                {
                    Nombre = nombre,
                    UnidadMedida = unidad,
                    Stock = stock,
                    Descripcion = ""
                };

                nuevoIngrediente = DatabaseHelper.AgregarIngrediente(nuevoIngrediente);
                CargarIngredientes();

                ShowDialog("Éxito", $"Ingrediente '{nuevoIngrediente.Nombre}' agregado correctamente.");
                OnLimpiarClick(null, null);
            }
            catch (Exception ex)
            {
                ShowDialog("Error al Agregar", $"No se pudo guardar el ingrediente: {ex.Message}");
            }
        }

        private void OnModificarClick(object sender, RoutedEventArgs e)
        {
            if (_selectedIngrediente == null)
            {
                ShowDialog("Error", "Debe seleccionar un ingrediente para modificar.");
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtNombre.Text) || string.IsNullOrWhiteSpace(TxtStock.Text) || CmbUnidad.SelectedItem == null)
            {
                ShowDialog("Error de Validación", "Todos los campos son obligatorios.");
                return;
            }

            var nombre = TxtNombre.Text.Trim();
            var unidad = CmbUnidad.SelectedItem.ToString() ?? "pz";

            if (!_regexNombre.IsMatch(nombre))
            {
                ShowDialog("Error de Formato", "El campo 'Nombre' solo puede contener letras y espacios.");
                return;
            }

            if (!double.TryParse(TxtStock.Text, out double stock))
            {
                ShowDialog("Error de Formato", "La cantidad de 'Stock' debe ser un número válido.");
                return;
            }

            if (stock < 0)
            {
                ShowDialog("Error de Validación", "La cantidad de 'Stock' no puede ser negativa.");
                return;
            }

            double maxStock;
            switch (unidad)
            {
                case "kg":
                case "lt":
                    maxStock = 100;
                    break;
                case "g":
                case "ml":
                    maxStock = 100000;
                    break;
                case "pz":
                    maxStock = 1000;
                    break;
                default:
                    maxStock = 1000;
                    break;
            }

            if (stock > maxStock)
            {
                ShowDialog("Error de Límite", $"La cantidad para '{unidad}' no puede ser mayor a {maxStock}.");
                return;
            }

            try
            {
                string nombreProducto = _selectedIngrediente.Nombre;

                _selectedIngrediente.Nombre = nombre;
                _selectedIngrediente.UnidadMedida = unidad;
                _selectedIngrediente.Stock = stock;

                DatabaseHelper.ActualizarIngrediente(_selectedIngrediente);
                CargarIngredientes();

                ShowDialog("Éxito", $"Ingrediente '{nombreProducto}' modificado correctamente.");
                OnLimpiarClick(null, null);
            }
            catch (Exception ex)
            {
                ShowDialog("Error al Modificar", $"No se pudo actualizar el ingrediente: {ex.Message}");
            }
        }

        private async void OnEliminarClick(object sender, RoutedEventArgs e)
        {
            if (_selectedIngrediente == null)
            {
                ShowDialog("Error", "Debe seleccionar un ingrediente para eliminar.");
                return;
            }

            string nombreProducto = _selectedIngrediente.Nombre;

            ContentDialog confirmDialog = new ContentDialog
            {
                Title = "¿Eliminar Ingrediente?",
                Content = $"¿Estás seguro de que deseas eliminar '{nombreProducto}' del inventario?\nEsta acción no se puede deshacer.",
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
                    DatabaseHelper.EliminarIngrediente(_selectedIngrediente.IdIngrediente);
                    CargarIngredientes();
                    ShowDialog("Éxito", $"Ingrediente '{nombreProducto}' eliminado.");
                    OnLimpiarClick(null, null);
                }
                catch (Exception ex)
                {
                    ShowDialog("Error al Eliminar", $"No se pudo eliminar el ingrediente. Es posible que esté siendo usado en una receta.\n\nError: {ex.Message}");
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
            CmbUnidad.SelectedIndex = -1;
            TxtStock.Text = string.Empty;

            MateriaPrimaGrid.SelectedItem = null;
            _selectedIngrediente = null;
        }


        private void TxtNombre_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (args.NewText.Any(char.IsDigit))
            {
                args.Cancel = true;
            }
        }

        private void TxtStock_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (!_regexStock.IsMatch(args.NewText))
            {
                args.Cancel = true;
            }
        }


        private async void ShowDialog(string title, string content)
        {
            if (RootGrid.XamlRoot == null) return;

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
