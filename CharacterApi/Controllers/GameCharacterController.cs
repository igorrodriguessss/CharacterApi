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
        [HttpPost]
        public async Task<ActionResult<CharacterResponse>> AddCharacter(CreateCharacterRequest character)
        {
            var createdCharacter = await service.AddCharacterAsync(character);
            return CreatedAtAction(nameof(GetCharacter), new { id = createdCharacter.Id }, createdCharacter);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCharacter(int id, UpdateCharacterRequest character)
        {
            var isUpdated = await service.UpdatedCharacterAsync(id, character);
            return isUpdated ? NoContent() : NotFound("Personagem com o Id mencionado não foi encontrado");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCharacter(int id)
        {
            var isDeleted = await service.DeleteCharacterAsync(id);
            return isDeleted ? NoContent() : NotFound("Personagem com o Id mencionado não foi encontrado");
        }
    }
}
