using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserApi.Configurations;
using UserApi.Models;
using UserApi.Models.DTOs;

namespace UserApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthManagementController : ControllerBase
    {
        private readonly ILogger<AuthManagementController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtConfig _jwtConfig;


        public AuthManagementController(
            ILogger<AuthManagementController> logger,
            UserManager<ApplicationUser> userManager,
            IOptionsMonitor<JwtConfig> optionsMonitor
            )
        {
            _logger = logger;
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto requestDto)
        {
            // Check if the request is valid
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid request.");
            }

            // Check if the email already exists
            var emailExist = await _userManager.FindByEmailAsync(requestDto.Email);
            if (emailExist != null)
            {
                return BadRequest("Email already exists.");
            }

            // Create a new user
            var newUser = new ApplicationUser
            {
                UserName = requestDto.Email,
                Email = requestDto.Email,
            };

            // Attempt to create the user
            var isCreated = await _userManager.CreateAsync(newUser, requestDto.Password);

            if (isCreated.Succeeded)
            {
                // If the user is created successfully, generate a JWT token
                var token = GenerateJwtToken(newUser);

                // Retrieve the user ID
                var userId = newUser.Id;

                // Return a success response with the token and user ID
                var response = new RegistrationRequestResponse
                {
                    Result = true,
                    Token = token,
              
                };

                return Ok(response);
            }

            // If user creation fails, gather and return error messages
            var errors = string.Join(", ", isCreated.Errors.Select(e => e.Description));
            return BadRequest($"Error creating user. Errors: {errors}");
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto requestDto)
        {
            if (ModelState.IsValid)
            {
                var existUser = await _userManager.FindByEmailAsync(requestDto.Email);

                if (existUser == null)
                    return BadRequest("Invalid authentication.");

                var isPasswordValid = await _userManager.CheckPasswordAsync(existUser, requestDto.Password);
                if (isPasswordValid)
                {
                    // Retrieve the user ID
                    var userId = existUser.Id;

                    // Generate JWT token
                    var token = GenerateJwtToken(existUser);

                    return Ok(new LoginRequestResponse()
                    {
                        Token = token,
                        
                        Result = true,
                    });
                }

                return BadRequest("Invalid email or password.");
            }

            return BadRequest("Invalid email or password.");
        }



        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescription = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                  new Claim("Id",user.Id ?? string.Empty),
                  new Claim(JwtRegisteredClaimNames.Sub ,user.Email??String.Empty),
                  new Claim(JwtRegisteredClaimNames.Email ,user.Email ?? String.Empty),
                  new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString())

                }),
                Expires = DateTime.UtcNow.AddHours(4),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
                )
            };
            var token = jwtTokenHandler.CreateToken(tokenDescription);
            var jwtToken = jwtTokenHandler.WriteToken(token);
            return jwtToken;
        }

        [HttpGet]
        [Route("UserDetails")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetUserDetails()
        {
            // Get user details from claims
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;


            if (userEmail != null)
            {
                var userDetails = new UserLoginRequestDto // Assuming you have a UserDetails model class
                {
                    Email = userEmail,

                };

                return Ok(userDetails);
            }
            else
            {
                return BadRequest("User details not found.");
            }
        }
    }
}