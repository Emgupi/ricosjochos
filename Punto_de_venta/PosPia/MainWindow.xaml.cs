using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PosPia.Modelos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PosPia
{
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        
        private Cliente? _clienteActual; 
        private Cliente _clientePublicoGeneral; 

        
        public MainWindow()
        {
            this.InitializeComponent();

            var frameworkElement = this.Content as FrameworkElement;
            if (frameworkElement != null)
            {
                frameworkElement.DataContext = this;
            }

            ProductosMostrados = new ObservableCollection<Producto>();
            Carrito = new ObservableCollection<CarritoItem>();
            _todosLosProductos = new List<Producto>(); 
            _clientePublicoGeneral = new Cliente(); 

            ProductosGridView.ItemsSource = ProductosMostrados;
            CarritoListView.ItemsSource = Carrito;

            Carrito.CollectionChanged += Carrito_CollectionChanged;

            CargarDatosIniciales();
        }

        private void CargarDatosIniciales()
        {
            var categorias = DatabaseHelper.GetAllCategorias();
            CategoriasListView.ItemsSource = categorias;

            _todosLosProductos = DatabaseHelper.GetAllProductos();


            _clientePublicoGeneral = DatabaseHelper.GetAllClientes()
                .FirstOrDefault(c => c.RFC == "XAXX010101000") ?? new Cliente { Nombre = "Público General" };


            _clienteActual = _clientePublicoGeneral;
            ClienteActualTextBlock.Text = _clienteActual!.Nombre;

            FiltrarProductos(null);
        }

        

        private void TodosButton_Click(object sender, RoutedEventArgs e)
        {
            FiltrarProductos(null);
            CategoriasListView.SelectedItem = null;
        }

        private void CategoriasListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            string? categoriaClickeada = e.ClickedItem as string;
            FiltrarProductos(categoriaClickeada);
        }

        private void FiltrarProductos(string? categoria)
        {
            ProductosMostrados.Clear();
            List<Producto> productosAFiltrar;
            if (string.IsNullOrEmpty(categoria))
            {
                productosAFiltrar = _todosLosProductos;
            }
            else
            {
                productosAFiltrar = _todosLosProductos.Where(p => p.Categoria == categoria).ToList();
            }
            foreach (var producto in productosAFiltrar)
            {
                ProductosMostrados.Add(producto);
            }
        }

        private async void ProductosGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Producto? productoClickeado = e.ClickedItem as Producto;
            if (productoClickeado == null) return;

            if (!productoClickeado.EstaDisponible)
            {
                await ShowMessageAsync("Producto Agotado", "No hay suficientes ingredientes para preparar este producto.");
                return;
            }

            CarritoItem? itemEnCarrito = Carrito.FirstOrDefault(item => item.IdProducto == productoClickeado.IdProducto);

            if (itemEnCarrito != null)
            {

                await SumarUnoAlCarrito(itemEnCarrito);
            }
            else
            {
                int maxCantidad = DatabaseHelper.GetMaximaCantidadParaProducto(productoClickeado.IdProducto);

                if (maxCantidad <= 0)
                {
                    await ShowMessageAsync("Producto Agotado", "No hay suficientes ingredientes para preparar este producto.");
                    return;
                }

                CarritoItem nuevoItem = new CarritoItem
                {
                    IdProducto = productoClickeado.IdProducto,
                    Nombre = productoClickeado.Nombre,
                    PrecioUnitario = productoClickeado.Precio,
                    Cantidad = 1,
                    MaximaCantidad = maxCantidad 
                };
                nuevoItem.PropertyChanged += CarritoItem_PropertyChanged;
                Carrito.Add(nuevoItem);
            }
        }

        private void CarritoItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Cantidad" || e.PropertyName == "Subtotal")
            {
                ActualizarTotales();
            }
        }

        private void Carrito_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ActualizarTotales();
        }

        private async void BotonMas_Click(object sender, RoutedEventArgs e)
        {
            CarritoItem? item = (sender as Button)?.DataContext as CarritoItem;
            if (item != null)
            {
                await SumarUnoAlCarrito(item);
            }
        }

        private async Task SumarUnoAlCarrito(CarritoItem item)
        {
            if (item.Cantidad < item.MaximaCantidad)
            {
                item.Cantidad++; 
            }
            else
            {
                await ShowMessageAsync(
                    "Límite de Stock Alcanzado", 
                    $"No puedes agregar más de {item.MaximaCantidad} unidades de '{item.Nombre}'.\nNo hay suficientes ingredientes en el inventario.");
            }
        }

        private void BotonMenos_Click(object sender, RoutedEventArgs e)
        {
            CarritoItem? item = (sender as Button)?.DataContext as CarritoItem;
            if (item != null)
            {
                if (item.Cantidad > 1)
                {
                    item.Cantidad--; 
                }
                else
                {
                    item.PropertyChanged -= CarritoItem_PropertyChanged;
                    Carrito.Remove(item);
                }
            }
        }


        private void BotonLimpiar_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Carrito)
            {
                item.PropertyChanged -= CarritoItem_PropertyChanged;
            }
            Carrito.Clear();

            _clienteActual = _clientePublicoGeneral;
            ClienteActualTextBlock.Text = _clienteActual!.Nombre; 
        }

        private void ActualizarTotales()
        {
            double subtotal = Carrito.Sum(item => item.Subtotal);
            double iva = subtotal * 0.16;
            double total = subtotal + iva;

            SubtotalTextBlock.Text = subtotal.ToString("C2");
            IVATextBlock.Text = iva.ToString("C2");
            TotalTextBlock.Text = total.ToString("C2");

            PagarButton.IsEnabled = (total > 0);
        }

        private async void BotonPagar_Click(object sender, RoutedEventArgs e)
        {
            double subtotal = Carrito.Sum(item => item.Subtotal);
            double iva = subtotal * 0.16;
            double total = subtotal + iva;

            PagoDialog dialogoPago = new PagoDialog(total);

            FrameworkElement? fe = sender as FrameworkElement;
            if (fe == null) return;

            dialogoPago.XamlRoot = fe.XamlRoot;
            await dialogoPago.ShowAsync();

            if (dialogoPago.FueConfirmado)
            {
                int? idClienteParaVenta = (_clienteActual?.IdCliente == _clientePublicoGeneral?.IdCliente)
                    ? null
                    : _clienteActual?.IdCliente;

                int nuevaVentaId = DatabaseHelper.GuardarVenta(
                    subtotal,
                    iva,
                    total,
                    dialogoPago.MetodoPago,
                    Carrito,
                    idClienteParaVenta
                );

                if (nuevaVentaId > 0)
                {
                    TicketDialog dialogoTicket = new TicketDialog(nuevaVentaId, total, Carrito);
                    dialogoTicket.XamlRoot = fe.XamlRoot;
                    
                    ContentDialogResult resultadoTicket = await dialogoTicket.ShowAsync();
                    
                    if (resultadoTicket == ContentDialogResult.Primary)
                    {
                        TicketVistaPreviaDialog dialogoVistaPrevia = new TicketVistaPreviaDialog(
                            nuevaVentaId,
                            _clienteActual, 
                            subtotal,       
                            iva,            
                            total,          
                            Carrito         
                        );
                        dialogoVistaPrevia.XamlRoot = fe.XamlRoot;
                        await dialogoVistaPrevia.ShowAsync();
                    }
                    
                    BotonLimpiar_Click(this, new RoutedEventArgs());
                    
                    _todosLosProductos = DatabaseHelper.GetAllProductos();
                    FiltrarProductos(null); 
                    CategoriasListView.SelectedItem = null;
                }
                else
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Error de Base de Datos",
                        Content = "No se pudo guardar la venta. Revise la consola de depuración.",
                        CloseButtonText = "Aceptar",
                        XamlRoot = fe.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }


        private async void BotonPedido_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement? fe = sender as FrameworkElement;
            if (fe == null) return;

            SeleccionarClienteDialog dialogo = new SeleccionarClienteDialog();
            dialogo.XamlRoot = fe.XamlRoot;

            await dialogo.ShowAsync();


            if (dialogo.ClienteSeleccionado != null)
            {
                _clienteActual = dialogo.ClienteSeleccionado;
                ClienteActualTextBlock.Text = _clienteActual!.Nombre; 
            }
        }

        private async void BotonCorte_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement? fe = sender as FrameworkElement;
            if (fe == null) return;

            ContentDialog confirmDialog = new ContentDialog
            {
                Title = "Confirmar Corte de Caja",
                Content = "¿Desea realizar el corte de caja? Esta acción es solo para gerentes.",
                PrimaryButtonText = "Aceptar",
                CloseButtonText = "Cancelar",
                XamlRoot = fe.XamlRoot
            };

            var resultado = await confirmDialog.ShowAsync();

            
            if (resultado == ContentDialogResult.Primary)
            {

                CorteDeCaja corte = DatabaseHelper.GetCorteDeCajaHoy();

                CorteResultadoDialog resultadoDialog = new CorteResultadoDialog(corte);
                resultadoDialog.XamlRoot = fe.XamlRoot;
                await resultadoDialog.ShowAsync();
            }
        }
        private void RegresarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); 
        }


        private async Task ShowMessageAsync(string title, string content)
        {
            if (PagarButton.XamlRoot == null) return; 

            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "Aceptar",
                XamlRoot = PagarButton.XamlRoot
            };
            await dialog.ShowAsync();
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<Producto> ProductosMostrados { get; set; }
        public ObservableCollection<CarritoItem> Carrito { get; set; }
        private List<Producto> _todosLosProductos;
    }
}
