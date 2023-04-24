using DbManager.Data.DTOs;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Interfaces;
using DbManager.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDTO data)
        {
            return Ok();
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(UserLoginDTO data)
        {
            return Ok();
        }
    }
}
