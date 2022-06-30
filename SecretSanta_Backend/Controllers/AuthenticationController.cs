using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SecretSanta_Backend.Interfaces;
using SecretSanta_Backend.Models;
using SecretSanta_Backend.ModelsDTO;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecretSanta_Backend.Controllers
{
    [ApiController]
    [Route("login")]
    public class AuthenticationController : Controller
    {
        private IRepositoryWrapper _repository;
        private readonly IConfiguration _config;
        private readonly ILogger<AdminController> _logger;

        public AuthenticationController(ILogger<AdminController> logger, IConfiguration configuration, 
            IRepositoryWrapper repository)
        {
            _logger = logger;
            _config = configuration;
            _repository = repository;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Auth([FromBody] MemberLogin data)
        {
            try
            {
                if (data.UserName == null)
                {
                    _logger.LogError("Member username recived from client is null.");
                    return BadRequest("Null email");
                }
                if (data.Password == null)
                {
                    _logger.LogError("Member password recived from client is null.");
                    return BadRequest("Null password");
                }

                var checkedMember = ValidMember(data);

                if (checkedMember is null)
                {
                    _logger.LogError("Wrong login or password.");
                    return BadRequest(new { message = "Wrong login or password" });
                }

                var memberWithEmail = await _repository.Member.FindByCondition(x => x.Email == checkedMember.Email).ToListAsync();
                if (memberWithEmail.Count == 0)
                {
                    _logger.LogInformation("Member recived from client is new.");
                    _repository.Member.Create(checkedMember);
                    await _repository.SaveAsync();
                    return Ok(new { Token = GenerateJwtToken(checkedMember), Message = "Success" });
                }
                return Ok(new { Token = GenerateJwtToken(memberWithEmail.First()), 
                    Message = "Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside CreateMember action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        private Member? ValidMember(MemberLogin data)
        {
            string ldapServer = _config["LdapServer"];
            var userName = string.Format("uid={0},dc=example,dc=com", data.UserName);
            SearchResult result = null;
            Member member = null;

            var de = new DirectoryEntry(ldapServer, userName, data.Password, AuthenticationTypes.ServerBind);
            using (var ds = new DirectorySearcher(de))
            {
                ds.Filter = string.Format("(uid={0})", data.UserName);
                try
                {
                    result = ds.FindOne();
                    var email = result.Properties["mail"][0].ToString();
                    var name = result.Properties["cn"][0].ToString();
                    var surname = result.Properties["sn"][0].ToString();
                    member = new Member {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Surname = surname,
                        Patronymic = " ",
                        Email = email,
                        Role = "user"                                                       
                    };
                }
                catch (Exception ex)
                {
                    return null;
                    Console.WriteLine(ex.Message);
                }
            };
            return member;
        }

        private string GenerateJwtToken(Member member)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("UserID", member.Id.ToString()),
                    new Claim(ClaimTypes.Email, member.Email.ToString()),
                    new Claim(ClaimTypes.Role, member.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
