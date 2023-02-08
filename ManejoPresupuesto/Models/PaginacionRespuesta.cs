namespace ManejoPresupuesto.Models
{
    public class PaginacionRespuesta
    {
        public int Pagina { get; set; } = 1;
        public int RecordsPorPagina { get; set; } = 10;
        public int CantidadTotalDeRecords { get; set; }
        public int CantidadTotalDePaginas => (int)Math.Ceiling((double)CantidadTotalDeRecords / RecordsPorPagina);
        public string BaseURL { get; set; }
    }
    public class PaginacionRespuesta<T> : PaginacionRespuesta
    {
        public IEnumerable<T> Elementos { get; set; }
    }
}