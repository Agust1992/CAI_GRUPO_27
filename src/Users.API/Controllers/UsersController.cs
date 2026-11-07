using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    // INICIO DEL POST PARA REGISTRAR UN USUARIO
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request)
    {
        var response = await _userService.RegisterAsync(request);

        return StatusCode(201, response);
    }

    // INICIO DEL POST PARA INICIAR SESIÓN
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserRequest request)
    {
        var response = await _userService.LoginAsync(request);

        return Ok(response);
    }

}