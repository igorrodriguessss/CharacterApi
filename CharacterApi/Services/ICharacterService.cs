using CharacterApi.Models;
namespace CharacterApi.Services
{
    public interface ICharacterService
    {
        Task<List<Character>> GetAllCharactersAsync();
        Task<Character> GetCharacterByIdAsync(int id);
        Task<Character> AddCharacterAsync(Character character);
        Task<bool> UpdatedCharacterAsync(int id, Character character);
        Task<bool> DeleteCharacterAsync(int id);
    }
}
