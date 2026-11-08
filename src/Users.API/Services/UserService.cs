using Microsoft.AspNetCore.Identity;
using Users.API.Data;
using Users.API.Models;
using Users.API.DTOs;
using Users.API.Exceptions;

namespace Users.API.Services;

// Tiene las reglas del usuario y usa el repositorio y el hasher.
public class UserService
{
    private readonly UserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(
        UserRepository userRepository,
        IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    // Registra un usuario y devuelve sus datos públicos.
    public async Task<RegisterUserResponse> RegisterAsync(
        RegisterUserRequest request)
    {
        var usuarioExistente =
            await _userRepository.GetByEmailAsync(request.Email);

        if (usuarioExistente is not null)
        {
            throw new BusinessRuleException(
                "USR-001",
                "El email ya está registrado.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            FechaRegistro = DateTime.UtcNow,
            Activo = true,
            IntentosFallidos = 0
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.CreateAsync(user);

        return new RegisterUserResponse
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellido = user.Apellido,
            Email = user.Email,
            FechaRegistro = user.FechaRegistro,
            Activo = user.Activo
        };
    }

    // Comprueba los datos de inicio de sesión y devuelve los datos del usuario.
    public async Task<LoginUserResponse> LoginAsync(LoginUserRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null)
        {
            throw new BusinessRuleException(
                "USR-003",
                "Credenciales incorrectas.");
        }

        if (!user.Activo)
        {
            if (user.IntentosFallidos >= 3)
            {
                throw new BusinessRuleException(
                    "USR-004",
                    "Usuario bloqueado por demasiados intentos fallidos.");
            }

            throw new BusinessRuleException(
                "USR-005",
                "Usuario bloqueado por detección de fraude.");
        }

        var resultado = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (resultado == PasswordVerificationResult.Failed)
        {
            user.IntentosFallidos++;

            if (user.IntentosFallidos >= 3)
            {
                user.Activo = false;
                await _userRepository.UpdateLoginStatusAsync(user);

                throw new BusinessRuleException(
                    "USR-004",
                    "Usuario bloqueado por demasiados intentos fallidos.");
            }

            await _userRepository.UpdateLoginStatusAsync(user);

            throw new BusinessRuleException(
                "USR-003",
                "Credenciales incorrectas.");
        }

        if (user.IntentosFallidos > 0)
        {
            user.IntentosFallidos = 0;
            await _userRepository.UpdateLoginStatusAsync(user);
        }

        return new LoginUserResponse
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellido = user.Apellido,
            Email = user.Email
        };
    }

    // Busca un usuario por ID y devuelve solo sus datos públicos.
    public async Task<UserLookupResponse?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null)
        {
            return null;
        }

        return new UserLookupResponse
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellido = user.Apellido,
            Email = user.Email
        };
    }

}
