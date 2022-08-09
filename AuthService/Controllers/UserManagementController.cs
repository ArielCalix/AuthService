using AuthService.Context;
using AuthService.Models;
using AuthService.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly userDbContext _context;
        public UserManagementController(userDbContext context)
        {
            _context = context;
        }
        [HttpPost("Registrar")]
        public object RegistrarUsuario(Employed employed)
        {
            try
            {
                if (_context.Employeds == null)
                {
                    return NotFound();
                }
                employed.CreatedAt = DateTime.Now;
                employed.UpdateAt = DateTime.Now;
                employed.State = 1;
                employed.EncryptedPass = Encryption.Encrypt(employed.EncryptedPass);
                var currentIp = HttpContext.Connection.RemoteIpAddress;
                employed.CurrentIp = (currentIp != null) ? currentIp.ToString() : "";

                _context.Add(employed);
                _context.SaveChanges();
                return Ok(new { msj = "Usuario creado", employed = employed });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mjs = $"Ocurrio un error! {ex.Message}"
                });
            }
        }
        [HttpPost("AssignRole")]
        public object AssignarRol(UserRole userRole)
        {
            try
            {
                if (_context.UserRoles == null) return NotFound();

                if (!_context.Employeds.Any(c => c.Identification.Equals(userRole.UserIdentity))) return NotFound();
                if (!_context.Roles.Any(c => c.Id.Equals(userRole.Id))) return NotFound();

                userRole.CreatedAt = DateTime.Now;
                userRole.UpdatedAt = DateTime.Now;

                _context.UserRoles.Add(userRole);
                _context.SaveChanges();
                return Ok(new { employed = userRole });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    msj = ex.Message,
                });
            }
        }
        [HttpPost("Login")]
        public object Login([Required][FromHeader] string Authorization)
        {
            if (_context.Employeds == null)
            { return NotFound(new { msj = "No hay usuarios registrados" }); }
            else
            {
                string base64 = Authorization.Replace("Bearer ", "");
                var encodedTextBytes = Convert.FromBase64String(base64);
                string textoPlano = Encoding.UTF8.GetString(encodedTextBytes);
                string[] authUser = textoPlano.Split(",");
                string email = authUser[0];
                string pass = (authUser[1] != "") ? authUser[1] : "";

                Employed employed = new();
                if (!_context.Employeds.Any(c => c.Email.Equals(email))) return BadRequest();
                else employed = _context.Employeds.Where(c => c.Email.Equals(email)).FirstOrDefault();

                if (employed == null) return BadRequest();
                else if (employed.State != 1) return Unauthorized();

                bool isMatch = (employed != null) ? Encryption.Decrypt(employed.EncryptedPass).Equals(pass) : false;

                if (!isMatch) return Unauthorized();

                if (_context.UserTokens != null)
                {
                    UserToken userTokenDb = _context.UserTokens.Where(c => c.UserIdentity.Equals(employed.Identification)).FirstOrDefault();
                    if (userTokenDb == null)
                    {
                        UserToken userToken = new UserToken() { CurrentToken = DateTime.Now, LastToken = DateTime.Now, UserIdentity = employed.Identification, HashToken = Authorization };
                        _context.UserTokens.Add(userToken);
                        _context.SaveChanges();
                    }
                    else
                    {
                        userTokenDb.LastToken = userTokenDb.CurrentToken;
                        userTokenDb.CurrentToken = DateTime.Now;
                        _context.Entry(userTokenDb).State = EntityState.Detached;
                        _context.Entry(userTokenDb).State = EntityState.Modified;
                        _context.SaveChanges();
                    }
                }


                return Ok(new
                {
                    employed,
                    token = Authorization
                });
            }
        }
        [HttpPost("TokenGenerate")]
        public object TokenGenerate(Employed employed)
        {
            if (_context.Employeds == null)
            {
                return NotFound(new { msj = "No hay usuarios registrados" });
            }
            else
            {
                if (employed != null)
                {
                    string email = employed.Email;
                    Employed registryEmployed = new();
                    string token = "";
                    if (!_context.Employeds.Any(c => c.Email.Equals(employed.Email))) return NotFound();
                    else registryEmployed = _context.Employeds.Where(c => c.Email.Equals(email)).FirstOrDefault();

                    bool isUser = employed.EncryptedPass.Equals(Encryption.Decrypt(registryEmployed.EncryptedPass));
                    if (isUser)
                    {

                        string data = $"{employed.Email},{employed.EncryptedPass}";
                        Byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                        token = Convert.ToBase64String(dataBytes);
                        token = Regex.Replace(token, ' '.ToString(), string.Empty).Trim();
                        return Ok(new { token = token });
                    }
                    else { return BadRequest(); }
                }
                else { return BadRequest(); }
            }
        }
    }
}
