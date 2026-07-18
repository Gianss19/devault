using System.ComponentModel.DataAnnotations;
using devault.DTO;
using devault.DTO.Users;
using Microsoft.AspNetCore.Mvc;

namespace devault.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
            //TODO: INYECTAR DEPENDENCIAS.


public AuthController()
{
    
}

[HttpPost]
public ActionResult Register([FromBody] [Required] UserRegisterDto user)
    {

        return Created();    
    }


[HttpPost]
    public IActionResult Login([FromBody] UserAuthRequestDto user)
    {
        string Token = "";
        
        return Ok(Token);
    }


    
}
