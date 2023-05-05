using DbManager.Data.DTOs;
using DbManager.Neo4j.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WepPartDeliveryProject;

public class JwtService
{
    private readonly ApplicationSettings _appSettings;

    public JwtService(IConfiguration configuration)
    {
        // Fetch settings object from configuration
        _appSettings = new ApplicationSettings();
        configuration.GetSection("ApplicationSettings").Bind(_appSettings);
    }

    public JwtTokenInfoOutDTO GenerateAccessJwtToken(string userId, List<string> roles)
    {
        var secretKey = Encoding.ASCII.GetBytes(_appSettings.JwtSecretKey);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Sid, userId),
            }),
            Expires = DateTime.Now.AddMinutes(30), // Время истечения токена
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
        };
        foreach (var role in roles)
        {
            tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new JwtTokenInfoOutDTO { JwtToken = tokenString, ValidTo = token.ValidTo, RoleNames = roles };
    }

    public bool UserHasRole(string? jwtToken, string roleName)
    {
        if(jwtToken== null)
            return false;

        var clearJwtToken = jwtToken.Replace("Bearer ", "");
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(clearJwtToken);

        if (token.Payload.TryGetValue("role", out var userRoleName) && (string)userRoleName == roleName)
        {
            return true;
        }

        return false;
    }
}