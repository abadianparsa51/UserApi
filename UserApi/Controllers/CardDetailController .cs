using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UserApi.Data;
using UserApi.Models;
using UserApi.Models.DTOs;

namespace UserApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class CardDetailController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApiDbContext _context;
        public CardDetailController(UserManager<ApplicationUser> userManager, ApiDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddCard([FromBody] CardDetailDto cardDetailDto)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
                return BadRequest("User not found.");

            var card = new CardDetail
            {
                CardNumber = cardDetailDto.CardNumber,
                ExpirationDate = cardDetailDto.ExpirationDate,
                UserId = user.Id
            };

            _context.CardDetails.Add(card);
            await _context.SaveChangesAsync();

            return Ok("Card added successfully.");
        }

        [HttpGet("user-cards")]
        public async Task<IActionResult> GetUserCards()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
                return BadRequest("User not found.");

            var userCards = _context.CardDetails.Where(c => c.UserId == user.Id).ToList();
            return Ok(userCards);
        }
    }
}
 