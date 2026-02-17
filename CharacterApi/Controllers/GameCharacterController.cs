using CharacterApi.Dtos;
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
        public async Task<ActionResult<List<CharacterResponse>>> GetCharacters() => Ok(await service.GetAllCharactersAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<CharacterResponse>> GetCharacter(int id)
        {
            var character = await service.GetCharacterByIdAsync(id);
            return character is null ? NotFound("Personagem com o Id mencionado não foi encontrado") : Ok(character);
        }
        //[HttpPost]
        //public ActionResult<Character> AddCharacter(Character character)
        //{
        //    if(character == null)
        //    {
        //        return BadRequest("O personagem não pode ser nulo");
        //    }
        //    character.Id = 
        //}
    }
}
