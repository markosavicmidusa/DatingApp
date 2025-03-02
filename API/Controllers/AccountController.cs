using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{


    public class AccountController(DataContext context, ITokenService tokenService) : BaseApiController
    {


        [HttpPost("register")] // account/register
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {

            if (await UserExists(registerDto.Username)) return Conflict(new { message = "Username already taken" }); // conflict 409

            var user = CreateUser(registerDto.Username, registerDto.Password);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            //return Ok(user);
            return new UserDto{
                Username = user.UserName,
                Token = tokenService.CreateToken(user)
            };

        }

        [HttpPost("login")] // http://localhost:5000/api/account/login
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {

            if (string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            var user = await context.Users.FirstOrDefaultAsync(u => string.Equals(u.UserName, loginDto.Username.ToLower()));

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            if(!VerifyPassword(loginDto.Password,user.PasswordHash,user.PasswordSalt))
            {
                return Unauthorized(new {message = "Invalid password"});
            }

            return new UserDto{
                Username = user.UserName,
                Token = tokenService.CreateToken(user)
            };
        }

        private static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            
            using var hmac = new HMACSHA512(storedSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            return computedHash.SequenceEqual(storedHash);
        }

        private async Task<bool> UserExists(string username)
        {

            return await context.Users.AnyAsync(u => u.UserName == username.ToLower());
        }

        private static AppUser CreateUser(string username, string password)
        {

            using var hmac = new HMACSHA512(); // Generates a new random salt automatically

            return new AppUser
            { 
                UserName = username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)), // Hashing the password
                PasswordSalt = hmac.Key // Storing the salt
            };
        }



    }
}
