using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserApi.Data;
using UserApi.Models;

namespace UserApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardDetailController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public CardDetailController(ApiDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CardDetail>>> GetcardDetails()
        {
            return await _context.CardDetails.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CardDetail>> GetCardDetail(int id)
        {
            var cardDetail = await _context.CardDetails.FindAsync(id);

            if (cardDetail == null)
            {
                return NotFound();
            }

            return cardDetail;
        }
        [HttpPost]
        public async Task<ActionResult<CardDetail>> PostCardDetail(CardDetail cardDetail)
        {
            // Get the user ID from the logged-in user (assuming you have authentication set up)
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if the user ID is available
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found");
            }

            // Set the UserId property of the cardDetail
            cardDetail.UserId = userId;

            _context.CardDetails.Add(cardDetail);
            await _context.SaveChangesAsync();

            return Ok(await _context.CardDetails.ToListAsync());
        }
    }
}
