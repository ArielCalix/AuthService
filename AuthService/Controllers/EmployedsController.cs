using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService.Context;
using AuthService.Models;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployedsController : ControllerBase
    {
        private readonly userDbContext _context;

        public EmployedsController(userDbContext context)
        {
            _context = context;
        }

        // GET: api/Employeds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employed>>> GetEmployeds()
        {
          if (_context.Employeds == null)
          {
              return NotFound();
          }
            return await _context.Employeds.ToListAsync();
        }

        // GET: api/Employeds/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employed>> GetEmployed(string id)
        {
          if (_context.Employeds == null)
          {
              return NotFound();
          }
            var employed = await _context.Employeds.FindAsync(id);

            if (employed == null)
            {
                return NotFound();
            }

            return employed;
        }

        // PUT: api/Employeds/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployed(string id, Employed employed)
        {
            if (id != employed.Identification)
            {
                return BadRequest();
            }

            _context.Entry(employed).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployedExists(id))
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

        // POST: api/Employeds
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Employed>> PostEmployed(Employed employed)
        {
          if (_context.Employeds == null)
          {
              return Problem("Entity set 'userDbContext.Employeds'  is null.");
          }
            _context.Employeds.Add(employed);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EmployedExists(employed.Identification))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEmployed", new { id = employed.Identification }, employed);
        }

        // DELETE: api/Employeds/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployed(string id)
        {
            if (_context.Employeds == null)
            {
                return NotFound();
            }
            var employed = await _context.Employeds.FindAsync(id);
            if (employed == null)
            {
                return NotFound();
            }

            _context.Employeds.Remove(employed);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployedExists(string id)
        {
            return (_context.Employeds?.Any(e => e.Identification == id)).GetValueOrDefault();
        }
    }
}
