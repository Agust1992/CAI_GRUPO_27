using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.Models;

namespace Users.API.Data;

// Busca usuarios en la tabla users.
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

    // Busca un usuario por su email; devuelve null si no existe.
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

        return await connection.QuerySingleOrDefaultAsync<User>(
            sql,
            new { Email = email });
    }
    // Guarda un usuario nuevo en la tabla users.
    public async Task CreateAsync(User user)
    {
        using var connection = CreateConnection();

        const string sql = """
        INSERT INTO users (
            id,
            nombre,
            apellido,
            email,
            password_hash,
            fecha_registro,
            activo,
            intentos_fallidos
        )
        VALUES (
            @Id,
            @Nombre,
            @Apellido,
            @Email,
            @PasswordHash,
            @FechaRegistro,
            @Activo,
            @IntentosFallidos
        );
        """;

        await connection.ExecuteAsync(sql, user);
    }

}