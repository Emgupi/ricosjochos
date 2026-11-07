using System.Collections.Generic;

namespace PosPia.Modelos
{

    public class CorteDeCaja
    {
        public int CantidadTickets { get; set; }
        public double FondoInicial { get; set; }
        public double TotalVendido { get; set; }
        public double TotalEfectivo { get; set; }
        public double TotalTransferencia { get; set; }

        public List<VentaDetalladaItem> VentasDetalladas { get; set; }

        public CorteDeCaja()
        {
            CantidadTickets = 0;
            TotalVendido = 0;
            TotalEfectivo = 0;
            TotalTransferencia = 0;

            VentasDetalladas = new List<VentaDetalladaItem>();
        }
    }
}
