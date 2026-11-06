namespace PosPia.Modelos
{

    public class Cliente
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; }
        public string Empresa { get; set; }
        public string RFC { get; set; }


        public string Telefono { get; set; }
        public string Direccion { get; set; }

        public string InfoCompleta
        {
            get { return $"{Nombre} ({RFC})"; }
        }

        public Cliente()
        {
            IdCliente = 0;
            Nombre = string.Empty;
            Empresa = string.Empty;
            RFC = string.Empty;
            Telefono = string.Empty;
            Direccion = string.Empty;
        }
    }
}
