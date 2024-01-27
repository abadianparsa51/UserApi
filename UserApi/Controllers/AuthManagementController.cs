﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
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
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;


         public AuthManagementController(
             ILogger<AuthManagementController> logger,
             UserManager<IdentityUser>userManager,
             IOptionsMonitor<JwtConfig>optionsMonitor
             )
        {
            _logger = logger;
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
        }
        [HttpPost]
        [Route("Register")]

        public async Task<IActionResult> Register([FromBody]
        UserRegistrationRequestDto requestDto)
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
            var newUser = new IdentityUser
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

                // Return a success response with the token
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
            if (ModelState.IsValid) {
                var existUser = await _userManager.FindByEmailAsync(requestDto.Email);

                if (existUser == null)
                
                    return BadRequest("invalid auth");

                var isPasswordValid = await _userManager.CheckPasswordAsync(existUser, requestDto.Password);
                    if (isPasswordValid)
                {
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
        [HttpPost("signout")]
        [Authorize] // Only authenticated users can sign out
        public async Task<IActionResult> SignOut()
        {
            // Sign the user out (based on your authentication scheme)
            await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);

            // Optionally, perform additional sign-out logic

            return Ok(new { message = "Successfully signed out." });
        }
        private string GenerateJwtToken( IdentityUser user)
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
    }
}