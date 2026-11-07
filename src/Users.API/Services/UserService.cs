using Microsoft.AspNetCore.Identity;
using Users.API.Data;
using Users.API.Models;

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
}
