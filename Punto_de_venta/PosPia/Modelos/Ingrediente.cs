namespace PosPia.Modelos
{
    public class Ingrediente
    {
        public int IdIngrediente { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public double Stock { get; set; }
        public string UnidadMedida { get; set; }

        public Ingrediente()
        {
            Nombre = string.Empty;
            UnidadMedida = string.Empty;
        }
    }
}
