using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PosPia.Modelos
{
    public class CarritoItem : INotifyPropertyChanged
    {

        private int _cantidad;

        public int IdProducto { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public double PrecioUnitario { get; set; }

        public int MaximaCantidad { get; set; }

        public int Cantidad
        {
            get { return _cantidad; }
            set
            {
                if (_cantidad != value)
                {
                    _cantidad = value;

                    OnPropertyChanged();

                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }


        public double Subtotal
        {
            get { return Cantidad * PrecioUnitario; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
