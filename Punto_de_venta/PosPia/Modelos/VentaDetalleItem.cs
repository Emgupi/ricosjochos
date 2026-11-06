namespace PosPia.Modelos
{
    public class VentaDetalladaItem
    {
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public double Subtotal { get; set; }
        public string NombreCliente { get; set; }

        public VentaDetalladaItem()
        {
            NombreProducto = string.Empty;
            NombreCliente = string.Empty;
        }
    }
}
