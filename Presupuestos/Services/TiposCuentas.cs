using Dapper;
using Microsoft.Data.SqlClient;
using Presupuestos.Models;

namespace Presupuestos.Services
{
    public interface IRepositorioTiposCuentas
    {
        Task Actualizar(Tipocuenta tipocuenta);
        Task Borrar(int id);
        Task Crear(Tipocuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<Tipocuenta>> Obtener(int usuarioId);
        Task<Tipocuenta> ObtenerPorId(int id, int usuarioId);
    }

    public class TiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string connectionString;
        public TiposCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Tipocuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO TiposCuentas(Nombre, UsuarioId, Orden)
            Values (@Nombre, @UsuarioId, 0);
            SELECT SCOPE_IDENTITY();", tipoCuenta);
            tipoCuenta.Id = id;
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1 FROM TiposCuentas WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId;", new { nombre, usuarioId });
            return existe == 1;
        }

        public async Task<IEnumerable<Tipocuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Tipocuenta>(@"SELECT * FROM TiposCuentas WHERE UsuarioId=@UsuarioId", new { usuarioId });
        }

        public async Task Actualizar(Tipocuenta tipocuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE TiposCuentas SET Nombre = @Nombre WHERE id = @id;", tipocuenta);
        }

        public async Task<Tipocuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Tipocuenta>(@"SELECT id, Nombre, Orden FROM TiposCuentas WHERE id = @id AND UsuarioId = @UsuarioId;", new { id, usuarioId });
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE TiposCuentas WHERE id = @id", new { id });

        }
    }
}
