using DbManager.Data.DTOs;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Interfaces;
using DbManager.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace WepPartDeliveryProject.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPasswordService _pswService;

        public AuthController(IRepositoryFactory repositoryFactory, IPasswordService passwordService)
        {
            _repositoryFactory = repositoryFactory;
            _pswService = passwordService;
        }

        [Authorize(Policy = "age-policy")]
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDTO data)
        {
            var res = HttpContext.User;
            return Ok();
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(UserLoginDTO data)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Fake User"),
                new Claim("age", "25", ClaimValueTypes.Integer),
                new Claim(ClaimTypes.Authentication, "true")
            };

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaims(claims);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext
                .SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(40)
                });

            //_logger.LogInformation(4, "User logged in.");

            return Ok();
        }
    }
}
