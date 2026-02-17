using CharacterApi.Dtos;
using CharacterApi.Models;
namespace CharacterApi.Services
{
    public interface ICharacterService
    {
        Task<List<CharacterResponse>> GetAllCharactersAsync();
        Task<CharacterResponse?> GetCharacterByIdAsync(int id);
        Task<CharacterResponse> AddCharacterAsync(Character character);
        Task<bool> UpdatedCharacterAsync(int id, Character character);
        Task<bool> DeleteCharacterAsync(int id);
    }
}
