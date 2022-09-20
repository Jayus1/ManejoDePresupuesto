﻿using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string connectionString;
        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tiposCuentas)
        {
            using var connection = new SqlConnection(connectionString);
            var id =  await connection.QuerySingleAsync<int>
                                ($@"INSERT INTO TiposCuentas (Nombre, UsuarioId, Orden) VALUES (@Nombre, @UsuarioId, 0);
                                    SELECT SCOPE_IDENTITY();", tiposCuentas);

            tiposCuentas.Id = id;
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var con = new SqlConnection(connectionString);
            var existe = await con.QueryFirstOrDefaultAsync<int>(
                @"SELECT 1
                  From TiposCuentas
                  WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId;",
                  new { nombre, usuarioId });

            return existe == 1;
        }
        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<TipoCuenta>(@"SELECT Id,Nombre, Orden FROM TiposCuentas WHERE UsuarioId = @UsuarioId", new { usuarioId });
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync($"UPDATE TiposCuentas " +
                                          $"SET Nombre = @Nombre " +
                                          $"WHERE Id = @Id", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>($"SELECT Id, Nombre, Orden" +
                $"From TiposCuentas" +
                $"WHERE Id = @Id and UsuarioId = @usuarioId",
                new { id, usuarioId });
        }

    }

    public interface IRepositorioTiposCuentas
    {
        Task Actualizar(TipoCuenta tipoCuenta);
        Task Crear(TipoCuenta tiposCuentas);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
    }
}