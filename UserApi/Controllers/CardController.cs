using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.Models.DTOs;
using Microsoft.Extensions.Logging; // Add this for logger

namespace UserApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly ILogger<CardController> _logger; // Inject logger

        // Constructor injection for ApiDbContext and ILogger
        public CardController(ApiDbContext context, ILogger<CardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Method to check the card prefix
        [HttpPost("CheckPrefix")]
        public async Task<IActionResult> CheckCardPrefix([FromBody] CheckCardPrefixRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.CardNumber) || request.CardNumber.Length < 6)
            {
                // If CardNumber is not valid, return BadRequest
                return BadRequest(new CheckCardPrefixResponseDto
                {
                    IsValid = false,
                    Message = "Invalid request. Card number must be at least 6 digits."
                });
            }

            // Extract the prefix (first 6 digits of the card number)
            var prefix = request.CardNumber.Substring(0, 6);

            try
            {
                // Check if the prefix exists in the CardPrefixes table
                var exists = await _context.CardPrefixes.AnyAsync(cp => cp.Prefix == prefix);

                if (exists)
                {
                    // If the prefix exists, return a success response
                    return Ok(new CheckCardPrefixResponseDto
                    {
                        IsValid = true,
                        Message = "Valid card prefix."
                    });
                }
                else
                {
                    // If the prefix does not exist, log the warning and return NotFound
                    _logger.LogWarning("Invalid card prefix: {Prefix}", prefix);
                    return NotFound(new CheckCardPrefixResponseDto
                    {
                        IsValid = false,
                        Message = "Invalid card prefix."
                    });
                }
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors by logging and returning a 500 status code
                _logger.LogError(ex, "Error occurred while checking card prefix.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }
    }
}
