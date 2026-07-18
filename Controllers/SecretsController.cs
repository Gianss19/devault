using devault.DTO.Secrets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace devault.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SecretsController : ControllerBase
{
    //TODO: INYECTAR DEPENDENCIAS DE SERVICIOS



    [HttpPost]  
    [Authorize]
    public IActionResult Generate ([FromBody] SecretRequestDto secret)
    {
        
        
        return Created();

    }

    [HttpGet]
    [Authorize]
    [Route("secrets")]
    public IActionResult Secrets()
    {

        object? ListaResponseDto = null;

        return Ok(ListaResponseDto);
    }


    [HttpGet]
    [Authorize]
    [Route("{id:guid}")]
    public IActionResult Secrets(Guid id)
    {

        return Ok();
    }

    [HttpDelete]
    [Authorize]
    [Route("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        return Ok();

    }

}

