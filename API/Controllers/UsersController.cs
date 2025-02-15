using System;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(DataContext context) : ControllerBase
{   
   [HttpGet] // /api/users
   public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers(){

    var users = await context.Users.ToListAsync();

    if(users == null || users.Count == 0) return NotFound();

    return Ok(users);
   }

   [HttpGet("{id:int}")] // /api/users/1
   public async Task<ActionResult<AppUser>> GetUser(int id){

    var user = await context.Users.FindAsync(id);
    
    if(user == null) return NotFound();

    return Ok(user);
   }


}
