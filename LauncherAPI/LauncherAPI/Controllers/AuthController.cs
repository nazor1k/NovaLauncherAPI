using AutoMapper;
using BLL;
using DomainCore;
using JWTToken.Model;
using LauncherAPI.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LauncherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public AuthController(TokenService tokenService, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var appUser = _mapper.Map<ApplicationUser>(model);
            var token = _tokenService.CreateToken(appUser);
            var result = await _userManager.CreateAsync(appUser, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            appUser.RefreshToken = _tokenService.CreateRefreshToken();
            appUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(appUser);

            return Ok(new { token, appUser.RefreshToken });
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.EmailOrUserName);

            if (user == null || await _userManager.CheckPasswordAsync(user, model.Password) == false)
            {
                return BadRequest("Invalid username or password");
            }

            var token = _tokenService.CreateToken(user);
            user.RefreshToken = _tokenService.CreateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            return Ok(new { token, user.RefreshToken });
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO tokenRefreshModel)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(tokenRefreshModel.RefreshToken);

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid refresh token" });
            }

            if (user.RefreshToken != tokenRefreshModel.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return Unauthorized(new { Message = "Invalid refresh token" });
            }

            var newToken = _tokenService.CreateToken(user);
            var newRefreshToken = _tokenService.CreateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Failed" });
            }
            return Ok(new { Token = newToken, RefreshToken = newRefreshToken, Email = user.Email, Name = user.UserName, ExpiresAs = DateTime.Now.AddMinutes(15) });
        }
    }
}
