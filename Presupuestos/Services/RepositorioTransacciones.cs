using Dapper;
using Microsoft.Data.SqlClient;
using Presupuestos.Models;

namespace Presupuestos.Services
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int anio);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSermana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
    }
    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("InsertarTransacciones",
            new
            {
                transaccion.UsuarioId,
                transaccion.FechaTransaccion,
                transaccion.Monto,
                transaccion.CategoriaId,
                transaccion.CuentaId,
                transaccion.Nota

            }, commandType: System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.ExecuteAsync("Transacciones_Actualizar",
            new
            {
                transaccion.Id,
                transaccion.FechaTransaccion,
                transaccion.Monto,
                transaccion.CategoriaId,
                transaccion.CuentaId,
                transaccion.Nota,
                montoAnterior,
                cuentaAnteriorId

            }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(@"
                SELECT Transacciones.*, cat.TipoOperacionId 
                FROM Transacciones 
                INNER JOIN Categorias cat ON cat.Id = Transacciones.CategoriaId 
                WHERE Transacciones.Id = @Id AND Transacciones.UsuarioId = @UsuarioId"
                , new { id, usuarioId }
            );
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Borrar", new { id }, commandType: System.Data.CommandType.StoredProcedure);

        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(@"
                SELECT t.id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria,
                cu.Nombre as Cuenta, c.TipoOperacionId 
                FROM Transacciones t
                INNER JOIN Categorias c
                ON c.id = t.CategoriaId
                INNER JOIN Cuentas cu
                ON cu.id = t.CuentaId
                WHERE t.CuentaId = @CuentaId ANd t.UsuarioId = @UsuarioId
                AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin;",
                modelo
            );
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(@"
                SELECT t.id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria,
                cu.Nombre as Cuenta, c.TipoOperacionId , Nota
                FROM Transacciones t
                INNER JOIN Categorias c
                ON c.id = t.CategoriaId
                INNER JOIN Cuentas cu
                ON cu.id = t.CuentaId
                WHERE t.UsuarioId = @UsuarioId
                AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                ORDER BY t.FechaTransaccion DESC;",
                modelo
            );
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSermana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"
                SELECT DATEDIFF(D, @fechaInicio, FechaTransaccion) / 7 + 1 as Semana,
                SUM(Monto) as Monto, cat.TipoOperacionId
                FROM Transacciones
                INNER JOIN Categorias cat
                ON cat.id = Transacciones.CategoriaId
                WHERE Transacciones.UsuarioId = @usuarioId AND
                FechaTransaccion BETWEEN @fechaInicio AND @fechaFin
                GROUP BY DATEDIFF(D, @fechaInicio, FechaTransaccion) / 7 + 1, cat.TipoOperacionId",
                modelo
            );
        }

        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int anio)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"
                SELECT MONTH(FechaTransaccion) as Mes, SUM(Monto) as Monto, cat.TipoOperacionId
                FROM Transacciones
                INNER JOIN Categorias cat
                ON cat.id = Transacciones.CategoriaId
                WHERE Transacciones.UsuarioId = @usuarioId AND YEAR(FechaTransaccion) = @anio
                GROUP BY MONTH(FechaTransaccion), cat.TipoOperacionId;",
                new { usuarioId, anio }
            );
        }
    }
}
