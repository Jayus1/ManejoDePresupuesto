namespace ManejoPresupuesto.Models
{
    public class ReporteTransaccionesDetalladas
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public IEnumerable<TransaccionesPorFechas> TransaccioneAgrupadas { get; set; }
        public decimal BalanceDepositos => TransaccioneAgrupadas.Sum(x => x.BalanceDepositos);
        public decimal BalanceRetiros => TransaccioneAgrupadas.Sum(x => x.BalanceRetiros);
        public decimal Total => BalanceDepositos - BalanceRetiros;
        public class TransaccionesPorFechas
        {
            public DateTime FechaTransaccion { get; set; }
            public IEnumerable<Transaccion> Transacciones { get; set; }
            public Decimal BalanceDepositos => Transacciones.Where(x => x.TipoOperacionId == TipoOperacion.Ingresos)
                .Sum(x => x.Monto);
            public Decimal BalanceRetiros => Transacciones.Where(x => x.TipoOperacionId == TipoOperacion.Gastos)
                .Sum(x => x.Monto);
        }
    }
}
