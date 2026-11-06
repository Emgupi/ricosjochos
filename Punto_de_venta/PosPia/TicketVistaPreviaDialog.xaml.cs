using Microsoft.UI.Xaml.Controls;
using PosPia.Modelos;
using System;
using System.Collections.ObjectModel;

namespace PosPia
{

    public sealed partial class TicketVistaPreviaDialog : ContentDialog
    {
        public TicketVistaPreviaDialog(
            int idVenta,
            Cliente? cliente,
            double subtotal,
            double iva,
            double total,
            ObservableCollection<CarritoItem> items)
        {
            this.InitializeComponent();

            FechaTextBlock.Text = DateTime.Now.ToString("g");
            IdVentaTextBlock.Text = $"Venta ID: {idVenta:D4}";
            ClienteTextBlock.Text = $"Cliente: {cliente?.Nombre ?? "Público General"}";

            ItemsTicketListView.ItemsSource = items;

            SubtotalTicketTextBlock.Text = subtotal.ToString("C2");
            IvaTicketTextBlock.Text = iva.ToString("C2");
            TotalTicketTextBlock.Text = total.ToString("C2");
        }
    }
}
