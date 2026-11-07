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

}
