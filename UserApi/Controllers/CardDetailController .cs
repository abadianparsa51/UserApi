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
using UserApi.DTOs;
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
        public async Task<IActionResult> AddCard([FromBody] CardDetailDTO cardDetailDto)
        {
            try
            {
                // بازیابی شناسه کاربر از توکن
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return BadRequest("User ID not found in token.");

                if (string.IsNullOrEmpty(cardDetailDto.CardNumber) || cardDetailDto.CardNumber.Length < 6)
                {
                    return BadRequest("Card number is required and must be at least 6 digits.");
                }

                // استخراج پیش‌شماره کارت (6 رقم اول)
                var prefix = cardDetailDto.CardNumber.Substring(0, 6);

                // جستجوی بانک مرتبط با پیش‌شماره کارت
                var bankInfo = await _context.CardPrefixes
                    .Where(cp => cp.Prefix == prefix)
                    .Select(cp => new { cp.BankId, cp.BankName })
                    .FirstOrDefaultAsync();

                if (bankInfo == null)
                {
                    _logger.LogWarning("Invalid card prefix: {Prefix}", prefix);
                    return BadRequest("Invalid card prefix.");
                }

                // بررسی کارت تکراری
                var isDuplicateCard = await _context.CardDetails
                    .AnyAsync(c => c.CardNumber == cardDetailDto.CardNumber && c.UserId == userId);

                if (isDuplicateCard)
                {
                    _logger.LogWarning("Duplicate card attempt: {CardNumber} by user {UserId}", cardDetailDto.CardNumber, userId);
                    return BadRequest("This card is already added.");
                }

                // ایجاد شیء جدید برای کارت
                var card = new CardDetail
                {
                    CardNumber = cardDetailDto.CardNumber,
                    ExpirationDate = cardDetailDto.ExpirationDate,
                    CVV2 = cardDetailDto.CVV2,
                    BankId = bankInfo.BankId, // تنظیم BankId
                    BankName = bankInfo.BankName, // تنظیم BankName
                    UserId = userId // تخصیص شناسه کاربر
                };

                // افزودن کارت جدید به پایگاه داده
                await _context.CardDetails.AddAsync(card);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Card added successfully: {CardNumber} for user {UserId}", cardDetailDto.CardNumber, userId);

                // بازگشت پاسخ موفقیت همراه با اطلاعات بانک
                return Ok(new
                {
                    message = "Card added successfully.",
                    cardDetails = new
                    {
                        card.CardNumber,
                        card.BankId,
                        card.BankName,
                        card.ExpirationDate
                    }
                });
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

                // Query database for card details of the logged-in user using the email, 
                // including the bank name associated with each card
                var userCards = await _context.CardDetails
                    .Where(c => c.User.Email == userEmail)
                    .Include(c => c.Bank)  // Include the associated bank
                    .ToListAsync();

                // Select only necessary data and include bank name
                var userCardsWithBank = userCards.Select(card => new
                {
                    card.CardNumber,
                    card.ExpirationDate,
                    card.UserId,
                    BankName = card.Bank.Name  // Include the bank name
                });

                return Ok(userCardsWithBank);
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

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> UpdateCard(int id, [FromBody] CardDetailDTO updatedCardDto)
        {
            try
            {
                var card = await _context.CardDetails.FindAsync(id);

                if (card == null)
                    return NotFound(new { message = "Card not found." });

                card.CardNumber = updatedCardDto.CardNumber;
                card.ExpirationDate = updatedCardDto.ExpirationDate;

                _context.CardDetails.Update(card);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Card updated successfully.", card });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the card.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Internal server error." });
            }
        }

    }
}
