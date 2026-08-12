using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VerificarApi.Dtos.Auth;
using VerificarApi.Dtos.Users;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return Conflict(new { mensagem = ex.Message });
            }
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                var token = await _authService.LoginAsync(dto);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserResponseDto>> Me()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var perfil = await _userService.ObterPerfilAsync(userId);

            if (perfil == null)
                return NotFound();

            return Ok(perfil);
        }
    }
}