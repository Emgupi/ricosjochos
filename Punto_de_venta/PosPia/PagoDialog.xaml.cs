using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace PosPia
{

    public sealed partial class PagoDialog : ContentDialog
    {
        private double _totalAPagar = 0;

        public bool FueConfirmado { get; private set; } = false;
        public string MetodoPago { get; private set; } = "Efectivo";

        public PagoDialog(double total)
        {
            this.InitializeComponent();

            _totalAPagar = total;
            TotalPagarTextBlock.Text = total.ToString("C2");

            this.IsPrimaryButtonEnabled = false;
        }

        private void MetodoPago_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MetodoPagoRadioButtons.SelectedIndex == 0) 
            {
                MetodoPago = "Efectivo";
                EfectivoPanel.Visibility = Visibility.Visible;
   
                ValidarMontoEfectivo();
            }
            else 
            {
                MetodoPago = "Transferencia";
                EfectivoPanel.Visibility = Visibility.Collapsed;
                this.IsPrimaryButtonEnabled = true;
                CambioTextBlock.Text = "$0.00";
            }
        }

        private void EfectivoRecibido_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidarMontoEfectivo();
        }

        private void ValidarMontoEfectivo()
        {
            if (double.TryParse(EfectivoRecibidoTextBox.Text, out double recibido))
            {
                if (recibido >= _totalAPagar)
                {
                    double cambio = recibido - _totalAPagar;
                    CambioTextBlock.Text = cambio.ToString("C2");
                    this.IsPrimaryButtonEnabled = true; 
                }
                else
                {
                    CambioTextBlock.Text = "$0.00";
                    this.IsPrimaryButtonEnabled = false; 
                }
            }
            else
            {

                CambioTextBlock.Text = "$0.00";
                this.IsPrimaryButtonEnabled = false;
            }
        }


        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

            FueConfirmado = true;

        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            FueConfirmado = false;
        }
    }
}
