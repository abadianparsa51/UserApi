using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        private readonly ILogger<CardDetailController> _logger;

        public CardDetailController(UserManager<ApplicationUser> userManager, 
                                    ApiDbContext context,
                                    ILogger<CardDetailController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddCard([FromBody] CardDetailDto cardDetailDto)
        {
            try
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

                // After adding the card successfully, retrieve all cards for the user
                var userCards = await GetUserCards(user.Id);

                // Create a custom response object containing the user ID and card details
                var response = new
                {
                    UserId = user.Id,
                    Cards = userCards
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a card.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        [HttpGet("user-cards")]
        public async Task<ActionResult<IEnumerable<CardDetail>>> GetUserCards(string userId)
        {
            try
            {
                var userCards = await _context.CardDetails
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                return Ok(userCards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user cards.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCard(int id)
        {
            try
            {
                var card = await _context.CardDetails.FindAsync(id);

                if (card == null)
                    return NotFound("Card not found.");

                _context.CardDetails.Remove(card);
                await _context.SaveChangesAsync();

                return Ok("Card deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the card.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCard(int id, [FromBody] CardDetailDto updatedCardDto)
        {
            try
            {
                var card = await _context.CardDetails.FindAsync(id);

                if (card == null)
                    return NotFound("Card not found.");

                card.CardNumber = updatedCardDto.CardNumber;
                card.ExpirationDate = updatedCardDto.ExpirationDate;

                _context.CardDetails.Update(card);
                await _context.SaveChangesAsync();

                return Ok("Card updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the card.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }


    }
}
