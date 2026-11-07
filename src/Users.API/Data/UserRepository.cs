using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.Models;

namespace Users.API.Data;

// Busca y guarda usuarios en la tabla users.
public class UserRepository
{
    private readonly IConfiguration _config;

    public UserRepository(IConfiguration config)
    {
        _config = config;
    }

    private SqliteConnection CreateConnection()
    {
        var connectionString =
            _config.GetConnectionString("DefaultConnection")
            ?? "Data Source=users.db";

        return new SqliteConnection(connectionString);
    }

    // Busca un usuario por email; devuelve null si no existe.
    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = CreateConnection();

        const string sql = """
            SELECT
                id AS Id,
                nombre AS Nombre,
                apellido AS Apellido,
                email AS Email,
                password_hash AS PasswordHash,
                fecha_registro AS FechaRegistro,
                activo AS Activo,
                intentos_fallidos AS IntentosFallidos
            FROM users
            WHERE email = @Email;
            """;

        var datos = await connection.QuerySingleOrDefaultAsync<UserData>(
            sql,
            new { Email = email });

        if (datos is null)
        {
            return null;
        }

        return new User
        {
            Id = Guid.Parse(datos.Id),
            Nombre = datos.Nombre,
            Apellido = datos.Apellido,
            Email = datos.Email,
            PasswordHash = datos.PasswordHash,
            FechaRegistro = DateTime.Parse(
                datos.FechaRegistro,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind),
            Activo = datos.Activo == 1,
            IntentosFallidos = datos.IntentosFallidos
        };
    }

    // Guarda un usuario nuevo en la tabla users.
    public async Task CreateAsync(User user)
    {
        using var connection = CreateConnection();

        const string sql = """
            INSERT INTO users (
                id, nombre, apellido, email, password_hash,
                fecha_registro, activo, intentos_fallidos
            )
            VALUES (
                @Id, @Nombre, @Apellido, @Email, @PasswordHash,
                @FechaRegistro, @Activo, @IntentosFallidos
            );
            """;

        var datos = new
        {
            Id = user.Id.ToString(),
            user.Nombre,
            user.Apellido,
            user.Email,
            user.PasswordHash,
            FechaRegistro = user.FechaRegistro.ToString(
                "O",
                CultureInfo.InvariantCulture),
            Activo = user.Activo ? 1 : 0,
            user.IntentosFallidos
        };

        await connection.ExecuteAsync(sql, datos);
    }

    // Recibe los datos como SQLite los entrega.
    private class UserData
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FechaRegistro { get; set; } = string.Empty;
        public int Activo { get; set; }
        public int IntentosFallidos { get; set; }
    }

    // Actualiza el contador de intentos y si la cuenta sigue activa.
    public async Task UpdateLoginStatusAsync(User user)
    {
        using var connection = CreateConnection();

        const string sql = """
        UPDATE users
        SET intentos_fallidos = @IntentosFallidos,
            activo = @Activo
        WHERE id = @Id;
        """;

        var datos = new
        {
            Id = user.Id.ToString(),
            user.IntentosFallidos,
            Activo = user.Activo ? 1 : 0
        };

        await connection.ExecuteAsync(sql, datos);
    }
}