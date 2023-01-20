﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Models
{
    public class TransaccionActualizacionViewModel : TransaccionCreacionViewModel
    {
        public int CuentaAnteriorId { get; set; }

        public decimal MontoAnterior { get; set; }
    }
}