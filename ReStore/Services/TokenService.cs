using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ReStore.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ReStore.Services
{
    public class TokenService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;

        public TokenService(UserManager<User> userManager,IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        //building token 
        public async Task<string> GenerateToken(User user)
        {   //Payload content
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Name,user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));   
            }
            //Signature
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWTSettings:TokenKey"]));//the same key is used to sign the key is the same for decrypting
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenOptions = new JwtSecurityToken(
                    issuer: null,
                    audience: null,
                    claims: claims,
                    expires: DateTime.Now.AddDays(7),
                    signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }
    }
}
