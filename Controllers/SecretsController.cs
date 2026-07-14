using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace devalut.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SecretsController : ControllerBase
{
    //TODO: INYECTAR DEPENDENCIAS DE SERVICIOS



    [HttpPost]  
    [Authorize]
    public IActionResult<SecretResponseDto> Generate ([FromBody] SecretRequestDto secret)
    {
        
        
        return Created(response);

    }

    [HttpGet]
    [Authorize]
    [Route("secrets")]
    public Task<IActionResult<IReadOnlyList<SecretResponseDto>>> Secrets()
    {
        return Ok(ListaResponseDto);
    }


    [HttpGet]
    [Authorize]
    [Route("{id:guid}")]
    public IActionResult<SecretResponseDto> Secrets(Guid id)
    {
        return Ok(ResponseDto);
    }

    [HttpDelete]
    [Authorize]
    [Route("{id:guid}")]
    public IActionResult Delete(Guid id)

}
