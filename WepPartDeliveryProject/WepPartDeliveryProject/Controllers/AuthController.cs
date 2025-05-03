using DbManager.Data;
using DbManager.Data.DTOs;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Interfaces;
using DbManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WepPartDeliveryProject.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPasswordService _pswService;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IRepositoryFactory repositoryFactory, IPasswordService passwordService, JwtService jwtService, ILogger<AuthController> logger)
        {
            _repositoryFactory = repositoryFactory;
            _pswService = passwordService;
            _jwtService = jwtService;
            _logger = logger;
        }

        private void AddCookieDataToResponse(string RefreshToken, string? userId = null)
        {
            if(userId != null) 
            {
                Response.Cookies.Append("X-UserId", userId,
                            new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(30), SameSite = SameSiteMode.None });
            }

            Response.Cookies.Append("X-Refresh-Token", RefreshToken,
                        new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(30), SameSite = SameSiteMode.None });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginInDTO data)
        {
            var users = await _repositoryFactory.GetRepository<User>().GetNodesByPropertyAsync("Login", new[] { data.Login });
            if(users.Count != 1)
            {
                _logger.LogInformation("Вы ввели не верный логин");
                return BadRequest("Вы ввели не верный логин");
            }

            var user = users[0];

            if (user.IsBlocked)
            {
                _logger.LogInformation("Login Ваш аккаунт был заблокирован!");
                return BadRequest("Ваш аккаунт был заблокирован!");
            }

            if(_pswService.CheckPassword(data.Login, data.Password, user.PasswordHash.ToArray()))
            {
                if(user.RefreshTokenCreated.AddDays(60) < DateTime.Now)
                {
                    _logger.LogInformation($"попали сюда, хотя не должны. {user.RefreshTokenCreated}");
                    user.RefreshToken = Guid.NewGuid();
                    user.RefreshTokenCreated = DateTime.Now;

                    await ((IUserRepository)_repositoryFactory.GetRepository<User>()).UpdateRefreshTokenAsync(user);
                }

                AddCookieDataToResponse(user.RefreshToken.ToString(), user.Id.ToString());

                return Ok();
            }

            _logger.LogInformation($"попали сюда, хотя не должны. Не проверился пароль");
            return Unauthorized("Вы ввели не верный логин или пароль");
        }

        [HttpPost("refreshAccessToken")]
        public async Task<IActionResult> RefreshAccessToken()
        {
            var userRepo = (IUserRepository)_repositoryFactory.GetRepository<User>();

            var userId = Request.Cookies["X-UserId"];
            if (userId == null)
            {
                _logger.LogInformation("У вас отсутсвует refresh token. Вам необходимо авторизоваться или зарегистрироваться.");
                return BadRequest("У вас отсутсвует refresh token. Вам необходимо авторизоваться или зарегистрироваться.");
            }


            var inputRefreshToken = Request.Cookies["X-Refresh-Token"];

            var userNode = await userRepo.GetNodeAsync(userId);

            if (userNode.IsBlocked)
            {
                _logger.LogInformation("RefreshAccessToken Ваш аккаунт был заблокирован!");
                return BadRequest("Ваш аккаунт был заблокирован!"); 
            }

            if (inputRefreshToken == userNode.RefreshToken.ToString() && userNode.RefreshTokenCreated.AddDays(60) > DateTime.Now)
            {
                var userRoles = await userRepo.GetUserRoles(userId);

                var jwtTokenInfo = _jwtService.GenerateAccessJwtToken(userId, userRoles);

                AddCookieDataToResponse(userNode.RefreshToken.ToString());
                
                return Ok(jwtTokenInfo);
            }

            _logger.LogInformation("You refresh token don't work. You need to login or signup to system");
            return BadRequest("You refresh token don't work. You need to login or signup to system");
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(UserSignupInDTO data)
        {
            var user = new User() { 
                Login = data.Login, 
                Name = data.Name,
                Born = data.Born,
                Address = data.Address,
                PhoneNumber = data.PhoneNumber,
                RefreshToken = Guid.NewGuid(), 
                RefreshTokenCreated = DateTime.Now, 
                PasswordHash = _pswService.GetPasswordHash(data.Login, data.Password).ToList(), 
            };

            await _repositoryFactory.GetRepository<User>().AddNodeAsync(user);
            await _repositoryFactory.GetRepository<User>().SetNewNodeType<Client>(user.Id.ToString());

            AddCookieDataToResponse(user.RefreshToken.ToString(), user.Id.ToString());

            return Ok();
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Delete("X-UserId",
                        new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(60), SameSite = SameSiteMode.None });
            Response.Cookies.Delete("X-Refresh-Token",
                        new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(60), SameSite = SameSiteMode.None });

            return await Task.FromResult(Ok());
        }
    }
}
