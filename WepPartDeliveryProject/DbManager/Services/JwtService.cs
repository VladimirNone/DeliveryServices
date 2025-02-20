using DbManager.Data.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WepPartDeliveryProject;

namespace DbManager.Services;

public class JwtService
{
    private readonly ApplicationSettings _appSettings;

    public JwtService(IOptions<ApplicationSettings> appSettingsOptions)
    {
        _appSettings = appSettingsOptions.Value;
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
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha512Signature)
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

        var userRolesName = token.Claims.Where(h => h.Type == "role").ToList();

        if (userRolesName.Any(h=>h.Value == roleName))
        {
            return true;
        }

        return false;
    }
}