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

    public string GenerateJwtToken(string login, string role)
    {
        var secretKey = Encoding.ASCII.GetBytes(_appSettings.JwtSecretKey);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, login),
                new Claim(ClaimTypes.Role, role),
            }),
            Expires = DateTime.UtcNow.AddMinutes(15), // Время истечения токена
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
}