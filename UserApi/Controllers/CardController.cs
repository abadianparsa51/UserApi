using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using UserApi.Data;
using UserApi.Models;
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

        // POST: api/Card/CheckPrefix
        [HttpPost("CheckPrefix")]
        public async Task<IActionResult> CheckCardPrefix([FromBody] CheckCardPrefixRequest request)
        {
            if (string.IsNullOrEmpty(request?.CardNumber))
            {
                return BadRequest("Card number is required.");
            }

            // گرفتن پیش‌شماره از شماره کارت
            var prefix = request.CardNumber.Substring(0, 4);  // فرض کنیم پیش‌شماره ۴ رقم اول باشد

            // بررسی اینکه آیا پیش‌شماره موجود در دیتابیس هست یا خیر
            var exists = await _context.CardPrefixes
                .AnyAsync(cp => cp.Prefix == prefix);

            if (exists)
            {
                return Ok("Valid card prefix.");
            }
            else
            {
                return BadRequest("Invalid card prefix.");
            }
        }
    }
}
