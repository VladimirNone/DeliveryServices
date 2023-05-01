using DbManager.Data;
using DbManager.Data.DTOs;
using DbManager.Data.Nodes;
using DbManager.Neo4j.Interfaces;
using DbManager.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
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

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDTO data)
        {
            var users = await _repositoryFactory.GetRepository<User>().GetNodesByPropertyAsync("Login", new[] { data.Login });
            if(users.Count != 1)
            {
                return BadRequest();
            }

            var user = users[0];

            if(_pswService.CheckPassword(data.Login, data.Password, user.PasswordHash.ToArray()))
            {
                user.RefreshToken = Guid.NewGuid();
                user.RefreshTokenCreated = DateTime.Now;

                await _repositoryFactory.GetRepository<User>().UpdateNodeAsync(user);

                Response.Cookies.Append("X-UserId", user.Id.ToString(),
                            new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(60), SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None });
                Response.Cookies.Append("X-Refresh-Token", user.RefreshToken.ToString(),
                            new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(60), SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None });
                Response.Cookies.Append("X-Refresh-Token-Created", user.RefreshTokenCreated.ToString(),
                            new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(60), SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None });

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
            if(inputRefreshToken == userNode.RefreshToken.ToString() && userNode.RefreshTokenCreated.AddDays(60) > DateTime.Now)
            {
                var userRoleAsString = await userRepo.GetUserRole(userId);

                var jwtTokenInfo = _jwtService.GenerateAccessJwtToken(userId, userRoleAsString);
                var refreshToken = userNode.RefreshToken.ToString();
                userNode.RefreshTokenCreated = DateTime.Now;

                await userRepo.UpdateNodeAsync(userNode);

                Response.Cookies.Append("X-Refresh-Token", refreshToken,
                            new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(60), SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None });
                Response.Cookies.Append("X-Refresh-Token-Created", userNode.RefreshTokenCreated.ToString(),
                            new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(60), SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None });

                return Ok(jwtTokenInfo);
            }
            else
            {
                return BadRequest("You refresh token don't work. You need to login or signup to system");
            }
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(UserLoginDTO data)
        {
            var user = new User() { 
                Login = data.Login, 
                Name = data.Login, 
                RefreshToken = Guid.NewGuid(), 
                RefreshTokenCreated = DateTime.Now, 
                PasswordHash = _pswService.GetPasswordHash(data.Login, data.Password).ToList() 
            };

            await _repositoryFactory.GetRepository<User>().AddNodeAsync(user);
            await _repositoryFactory.GetRepository<User>().SetNewNodeType<Client>(user.Id.ToString());

            Response.Cookies.Append("X-UserId", user.Id.ToString(),
                        new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(60), SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None });
            Response.Cookies.Append("X-Refresh-Token", user.RefreshToken.ToString(),
                        new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(60), SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None });
            Response.Cookies.Append("X-Refresh-Token-Created", user.RefreshTokenCreated.ToString(),
                        new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(60), SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None });

            return Ok();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Delete("X-UserId",
                        new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(60), SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None });
            Response.Cookies.Delete("X-Refresh-Token",
                        new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(60), SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None });
            Response.Cookies.Delete("X-Refresh-Token-Created",
                        new CookieOptions() { HttpOnly = true, Secure = true, Expires = DateTime.Now.AddDays(60), SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None });

            return Ok();
        }
    }
}
