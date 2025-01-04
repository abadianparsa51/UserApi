using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.Models.DTOs;

namespace UserApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public CardController(ApiDbContext context)
        {
            _context = context;
        }

        // متد بررسی پیش‌شماره کارت
        [HttpPost("CheckPrefix")]
        public async Task<IActionResult> CheckCardPrefix([FromBody] CheckCardPrefixRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new CheckCardPrefixResponseDto
                {
                    IsValid = false,
                    Message = "Invalid request. Card number must be at least 6 digits."
                });
            }

            var prefix = request.CardNumber.Substring(0, 6);

            var exists = await _context.CardPrefixes.AnyAsync(cp => cp.Prefix == prefix);

            if (exists)
            {
                return Ok(new CheckCardPrefixResponseDto
                {
                    IsValid = true,
                    Message = "Valid card prefix."
                });
            }
            else
            {
                _logger.LogWarning("Invalid card prefix: {Prefix}", prefix);
                return NotFound(new CheckCardPrefixResponseDto
                {
                    IsValid = false,
                    Message = "Invalid card prefix."
                });
            }
        }

    }

    internal class _logger
    {
        internal static void LogWarning(string v, string prefix)
        {
            throw new NotImplementedException();
        }
    }
}