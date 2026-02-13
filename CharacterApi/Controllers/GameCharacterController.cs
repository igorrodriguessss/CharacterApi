using CharacterApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CharacterApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameCharacterController : ControllerBase
    {
        
        [HttpGet]
        public async Task<ActionResult<List<Character>>> GetCharacters()=>await Task.FromResult(Ok(characters));
        
    }
}
