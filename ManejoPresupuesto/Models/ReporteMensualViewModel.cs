namespace ManejoPresupuesto.Models
{
    public class ReporteMensualViewModel
    {

        public IEnumerable<ResultadoObtenerPorMes> TransaccionesPorMes { get; set; }

        public Decimal Ingresos => TransaccionesPorMes.Sum(x => x.Ingreso);
        public Decimal Gastos => TransaccionesPorMes.Sum(x => x.Gastos);
        public decimal Total => Ingresos - Gastos;
        public int Año { get; set; }

    }
}
