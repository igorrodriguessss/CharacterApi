using CharacterApi.Dtos;
using CharacterApi.Models;
namespace CharacterApi.Services
{
    public interface ICharacterService
    {
        Task<List<CharacterResponse>> GetAllCharactersAsync();
        Task<CharacterResponse?> GetCharacterByIdAsync(int id);
        Task<CharacterResponse> AddCharacterAsync(CreateCharacterRequest character);
        Task<bool> UpdatedCharacterAsync(int id, UpdateCharacterRequest character);
        Task<bool> DeleteCharacterAsync(int id);
    }
}
