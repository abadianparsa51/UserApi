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
                // Retrieve user ID from the token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                    return BadRequest("User ID not found in token.");

                if (string.IsNullOrEmpty(cardDetailDto.CardNumber) || cardDetailDto.CardNumber.Length < 6)
                {
                    return BadRequest("Card number is required and must be at least 6 digits.");
                }

                // Check for duplicate cards
                var existingCard = await _context.CardDetails
                    .AnyAsync(c => c.CardNumber == cardDetailDto.CardNumber && c.UserId == userId);

                if (existingCard)
                {
                    _logger.LogWarning("Duplicate card attempt: {CardNumber} by user {UserId}", cardDetailDto.CardNumber, userId);
                    return BadRequest("This card is already added.");
                }

                // Validate the card prefix
                var prefix = cardDetailDto.CardNumber.Substring(0, 6);
                var prefixExists = await _context.CardPrefixes.AnyAsync(cp => cp.Prefix == prefix);

                if (!prefixExists)
                {
                    _logger.LogWarning("Invalid card prefix: {Prefix}", prefix);
                    return BadRequest("Invalid card prefix.");
                }

                // Create a new CardDetail object
                var card = new CardDetail
                {
                    CardNumber = cardDetailDto.CardNumber,
                    ExpirationDate = cardDetailDto.ExpirationDate,
                    UserId = userId // Assign the user ID retrieved from the token
                };

                // Add the new card to the context
                _context.CardDetails.Add(card);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Card added successfully." });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a card.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }


        [HttpGet("user-cards")]
        public async Task<ActionResult<IEnumerable<CardDetail>>> GetUserCards()
        {
            try
            {
                // Extract user email from the JWT token
                var userEmailClaim = User.FindFirst(ClaimTypes.Email);
                if (userEmailClaim == null || string.IsNullOrEmpty(userEmailClaim.Value))
                {
                    return BadRequest("User email not found in token.");
                }
                var userEmail = userEmailClaim.Value;

                // Query database for card details of the logged-in user using the email
                var userCards = await _context.CardDetails
                    .Where(c => c.User.Email == userEmail)
                    .ToListAsync();

                return Ok(userCards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user cards.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }





        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCard(int id)
        {
            try
            {
                // Retrieve user ID from the token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                    return BadRequest("User ID not found in token.");

                // Find the card to be deleted
                var card = await _context.CardDetails
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (card == null)
                {
                    _logger.LogWarning("Card with ID {Id} not found for user {UserId}", id, userId);
                    return NotFound("Card not found.");
                }

                // Optionally, you can add a check for validation before deleting
                // For example, you could ensure the card is not already deleted, expired, etc.

                // Remove the card from the context
                _context.CardDetails.Remove(card);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Card deleted successfully." });
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
