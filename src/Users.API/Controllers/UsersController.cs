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
    /// <summary>
    /// Registra un usuario nuevo.
    /// </summary>
    /// <response code="201">El usuario se registró correctamente.</response>
    /// <response code="400">Los datos enviados son inválidos (USR-002).</response>
    /// <response code="409">El email ya está registrado (USR-001).</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError, "application/problem+json")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request)
    {
        var response = await _userService.RegisterAsync(request);

        return StatusCode(201, response);
    }

    // INICIO DEL POST PARA INICIAR SESIÓN
    /// <summary>
    /// Inicia sesión con el email y la contraseña.
    /// </summary>
    /// <response code="200">Las credenciales son correctas.</response>
    /// <response code="400">Los datos enviados son inválidos (USR-002).</response>
    /// <response code="401">Las credenciales no son válidas (USR-003).</response>
    /// <response code="403">El usuario está bloqueado (USR-004).</response>
    /// <response code="500">Ocurrió un error interno.</response>
    [ProducesResponseType(typeof(LoginUserResponse), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError, "application/problem+json")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserRequest request)
    {
        var response = await _userService.LoginAsync(request);

        return Ok(response);
    }

    // INICIO DEL GET PARA BUSCAR UN USUARIO POR ID.
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}