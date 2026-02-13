using CharacterApi.Models;
using CharacterApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CharacterApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameCharacterController(ICharacterService service) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<List<Character>>> GetCharacters() => Ok(await service.GetAllCharactersAsync());
        
    }
}
