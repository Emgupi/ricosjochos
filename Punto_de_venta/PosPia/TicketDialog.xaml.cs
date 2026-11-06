using Microsoft.UI.Xaml.Controls;
using PosPia.Modelos;
using System.Collections.ObjectModel; 

namespace PosPia
{
    public sealed partial class TicketDialog : ContentDialog
    {

        public TicketDialog(int idVenta, double totalPagado, ObservableCollection<CarritoItem> items)
        {
            this.InitializeComponent();

            this.Title = $"Ticket de Venta (ID: {idVenta})";
            ItemsListView.ItemsSource = items;

            TotalTicketTextBlock.Text = totalPagado.ToString("C2");
        }
    }
}
