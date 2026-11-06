using Microsoft.UI.Xaml.Controls;
using PosPia.Modelos;
using System.Collections.Generic;

namespace PosPia
{
    public sealed partial class SeleccionarClienteDialog : ContentDialog
    {
        public Cliente? ClienteSeleccionado { get; private set; }

        public SeleccionarClienteDialog()
        {
            this.InitializeComponent();

            IsPrimaryButtonEnabled = false;

            List<Cliente> clientes = DatabaseHelper.GetAllClientes();
            ClientesListView.ItemsSource = clientes;
        }

        private void ClientesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            IsPrimaryButtonEnabled = true;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ClienteSeleccionado = ClientesListView.SelectedItem as Cliente;
        }
    }
}
