using Microsoft.UI.Xaml.Controls;
using PosPia.Modelos;

namespace PosPia
{

    public sealed partial class CorteResultadoDialog : ContentDialog
    {
        public CorteResultadoDialog(CorteDeCaja corte)
        {
            this.InitializeComponent();

            FondoTextBlock.Text = corte.FondoInicial.ToString("C2");
            TotalVendidoTextBlock.Text = corte.TotalVendido.ToString("C2");
            EfectivoEnCajaTextBlock.Text = (corte.FondoInicial + corte.TotalEfectivo).ToString("C2");
            CantidadTicketsTextBlock.Text = corte.CantidadTickets.ToString();
            TotalEfectivoTextBlock.Text = corte.TotalEfectivo.ToString("C2");
            TotalTransferenciaTextBlock.Text = corte.TotalTransferencia.ToString("C2");

            VentasDetalladasListView.ItemsSource = corte.VentasDetalladas;
        }
    }
}
