namespace ManejoPresupuesto.Servicios
{
    public class ServicioUsuarios: IServicioUsuario
    {
        public int ObtenerUsuarioId()
        {
            return 1;
        }
    }

    public interface IServicioUsuario
    {
        int ObtenerUsuarioId();
    }
}
