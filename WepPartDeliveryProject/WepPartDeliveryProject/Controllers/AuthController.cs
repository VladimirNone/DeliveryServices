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
        private readonly JwtService _jwtService;

        public AuthController(IRepositoryFactory repositoryFactory, IPasswordService passwordService, JwtService jwtService)
        {
            _repositoryFactory = repositoryFactory;
            _pswService = passwordService;
            _jwtService = jwtService;
        }

        [Authorize]
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDTO data)
        {
            var res = HttpContext.User;
            return Ok();
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(UserLoginDTO data)
        {
            var jwtToken = _jwtService.GenerateJwtToken(data.Login, "client");

            return Ok(jwtToken);
        }
    }
}
