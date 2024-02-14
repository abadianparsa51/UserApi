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
                var token = GenerateJwtToken(newUser, requestDto.Email);

                // Retrieve the user ID
                var userId = newUser.Id;

                // Return a success response with the token and user ID
                var response = new RegistrationRequestResponse
                {
                    Result = true,
                    Token = token,
                    UserId = userId // Assuming there's a property for UserId in the response object
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
                    // Generate JWT token with email claim
                    var token = GenerateJwtToken(existUser, existUser.Email);

                    // Return login response with token, user ID, and email
                    var loginResponse = new LoginRequestResponse
                    {
                        Token = token,
                        UserId = existUser.Id,
                        Email = existUser.Email,
                        Result = true
                    };

                    return Ok(loginResponse);
                }

                return BadRequest("Invalid email or password.");
            }

            return BadRequest("Invalid email or password.");
        }



        private string GenerateJwtToken(ApplicationUser user, string userEmail)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret); // Assuming _jwtConfig.Secret is a property of type string

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, userEmail) // Add email claim
        }),
                Expires = DateTime.UtcNow.AddDays(_jwtConfig.ExpiryInDays),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


    }
}