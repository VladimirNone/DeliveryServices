using DbManager.Data;
using DbManager.Data.DTOs;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Interfaces;
using DbManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        private void AddCookieDataToResponse(string RefreshToken, string? userId = null)
        {
            if(userId != null) 
            {
                Response.Cookies.Append("X-UserId", userId,
                            new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(60), SameSite = SameSiteMode.None });
            }

            Response.Cookies.Append("X-Refresh-Token", RefreshToken,
                        new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(60), SameSite = SameSiteMode.None });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginInDTO data)
        {
            var users = await _repositoryFactory.GetRepository<User>().GetNodesByPropertyAsync("Login", new[] { data.Login });
            if(users.Count != 1)
            {
                return BadRequest();
            }

            var user = users[0];

            if (user.IsBlocked)
            {
                return BadRequest("Ваш аккаунт был заблокирован!");
            }

            if(_pswService.CheckPassword(data.Login, data.Password, user.PasswordHash.ToArray()))
            {
                user.RefreshToken = Guid.NewGuid();
                user.RefreshTokenCreated = DateTime.Now;

                await _repositoryFactory.GetRepository<User>().UpdateNodeAsync(user);

                AddCookieDataToResponse(user.RefreshToken.ToString(), user.Id.ToString());

                return Ok();
            }

            return Unauthorized("Вы ввели не верный логин или пароль");
        }

        [HttpPost("refreshAccessToken")]
        public async Task<IActionResult> RefreshAccessToken()
        {
            var userRepo = (IUserRepository)_repositoryFactory.GetRepository<User>(true);

            var userId = Request.Cookies["X-UserId"];
            if (userId == null)
            {
                return BadRequest("You don't have refresh token. You need to login or signup to system");
            }

            var inputRefreshToken = Request.Cookies["X-Refresh-Token"];

            var userNode = await userRepo.GetNodeAsync(userId);

            if (userNode.IsBlocked)
            {
                return BadRequest("Ваш аккаунт был заблокирован!");
            }
            var l = userNode.RefreshToken.ToString();
            if (inputRefreshToken == userNode.RefreshToken.ToString() && userNode.RefreshTokenCreated.AddDays(60) > DateTime.Now)
            {
                var userRoles = await userRepo.GetUserRoles(userId);

                var jwtTokenInfo = _jwtService.GenerateAccessJwtToken(userId, userRoles);

                userNode.RefreshTokenCreated = DateTime.Now;

                await userRepo.UpdateNodeAsync(userNode);

                AddCookieDataToResponse(userNode.RefreshToken.ToString());
                
                return Ok(jwtTokenInfo);
            }
            else
            {
                return BadRequest("You refresh token don't work. You need to login or signup to system");
            }
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(UserLoginInDTO data)
        {
            var user = new User() { 
                Login = data.Login, 
                Name = data.Login, 
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

            return Ok();
        }
    }
}
