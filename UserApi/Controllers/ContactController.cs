using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UserApi.Data;
using UserApi.Models;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApiDbContext _context;
    private readonly ILogger<ContactController> _logger;

    public ContactController(UserManager<ApplicationUser> userManager,
                             ApiDbContext context,
                             ILogger<ContactController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    // GET: api/Contact
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContactDTO>>> GetContacts()
    {
        var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        var contacts = await _context.Contacts
                                      .Where(c => c.UserId == userId)
                                      .ToListAsync();

        var contactDTOs = contacts.Select(c => new ContactDTO
        {
            Name = c.Name,
            Phone = c.Phone,
            Mail = c.Mail,
            DestinationCardNumber = c.DestinationCardNumber
        }).ToList();

        return Ok(contactDTOs);
    }

    // GET: api/Contact/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ContactDTO>> GetContact(int id)
    {
        var contact = await _context.Contacts.FindAsync(id);

        if (contact == null)
        {
            return NotFound();
        }

        var contactDTO = new ContactDTO
        {
            Name = contact.Name,
            Phone = contact.Phone,
            Mail = contact.Mail,
            DestinationCardNumber = contact.DestinationCardNumber
        };

        return Ok(contactDTO);
    }

    // POST: api/Contact
    [HttpPost]
    public async Task<ActionResult<ContactDTO>> PostContact(ContactDTO contactDTO)
    {
        if (contactDTO == null)
        {
            return BadRequest("Invalid contact data.");
        }

        var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        var contact = new Contact
        {
            Name = contactDTO.Name,
            Phone = contactDTO.Phone,
            Mail = contactDTO.Mail,
            DestinationCardNumber = contactDTO.DestinationCardNumber,
            UserId = userId
        };

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        var result = new ContactDTO
        {
            Name = contact.Name,
            Phone = contact.Phone,
            Mail = contact.Mail,
            DestinationCardNumber = contact.DestinationCardNumber
        };

        return CreatedAtAction("GetContact", new { id = contact.Id }, result);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> PutContact(int id, ContactDTO contactDTO)
    {
        // Fetch the existing contact from the database
        var contact = await _context.Contacts.FindAsync(id);

        if (contact == null)
        {
            return NotFound();
        }

        // Update the contact properties without checking the ID in the DTO
        contact.Name = contactDTO.Name;
        contact.Phone = contactDTO.Phone;
        contact.Mail = contactDTO.Mail;
        contact.DestinationCardNumber = contactDTO.DestinationCardNumber;

        // Mark the contact as modified
        _context.Entry(contact).State = EntityState.Modified;

        try
        {
            // Save the changes
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Handle concurrency exceptions
            if (!ContactExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // PUT: api/Contact/5
    //[HttpPut("{id}")]
    //public async Task<IActionResult> PutContact(int id, ContactDTO contactDTO)
    //{
    //    if (id != contactDTO.Id)
    //    {
    //        return BadRequest();
    //    }

    //    var contact = await _context.Contacts.FindAsync(id);
    //    if (contact == null)
    //    {
    //        return NotFound();
    //    }

    //    contact.Name = contactDTO.Name;
    //    contact.Phone = contactDTO.Phone;
    //    contact.Mail = contactDTO.Mail;
    //    contact.DestinationCardNumber = contactDTO.DestinationCardNumber;

    //    _context.Entry(contact).State = EntityState.Modified;

    //    try
    //    {
    //        await _context.SaveChangesAsync();
    //    }
    //    catch (DbUpdateConcurrencyException)
    //    {
    //        if (!ContactExists(id))
    //        {
    //            return NotFound();
    //        }
    //        else
    //        {
    //            throw;
    //        }
    //    }

    //    return NoContent();
    //}

    // DELETE: api/Contact/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContact(int id)
    {
        var contact = await _context.Contacts.FindAsync(id);
        if (contact == null)
        {
            return NotFound();
        }

        _context.Contacts.Remove(contact);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ContactExists(int id)
    {
        return _context.Contacts.Any(e => e.Id == id);
    }
}
