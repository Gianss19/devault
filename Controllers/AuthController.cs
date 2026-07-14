using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace devalut.Controllers;
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
    public IActionResult<TokenDto> Login([FromBody] UserRequestDto user)
    {

        return Ok(Token);
    }


    
}
