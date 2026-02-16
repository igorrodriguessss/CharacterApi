using CharacterApi.Models;

namespace CharacterApi.Services
{
    public class CharacterService : ICharacterService
    {

        static List<Character> characters = new List<Character>
        {
            new Character { Id = 1, Name = "Mario", Game = "Super Mario", Role = "Plumber" },
            new Character { Id = 2, Name = "Link", Game = "The Legend of Zelda", Role = "Hero" },
            new Character { Id = 3, Name = "Master Chief", Game = "Halo", Role = "Spartan" },
            new Character { Id = 4, Name = "Pica pau", Game = "carioca simulator", Role = "tecladista de churrascaria" }
        };
        public Task<Character> AddCharacterAsync(Character character)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteCharacterAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Character>> GetAllCharactersAsync() => await Task.FromResult(characters);

        public async  Task<Character?> GetCharacterByIdAsync(int id)
        {
            var result = characters.FirstOrDefault(c => c.Id == id);
            return await Task.FromResult(result);
        }

        public Task<bool> UpdatedCharacterAsync(int id, Character character)
        {
            throw new NotImplementedException();
        }
    }
}

