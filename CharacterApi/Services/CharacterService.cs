using CharacterApi.Models;

namespace CharacterApi.Services
{
    public class CharacterService : ICharacterService
    {

        static List<Character> characters = new List<Character>
        {
            new Character { Id = 1, Name = "Mario", Game = "Super Mario", Role = "Plumber" },
            new Character { Id = 2, Name = "Link", Game = "The Legend of Zelda", Role = "Hero" },
            new Character { Id = 3, Name = "Master Chief", Game = "Halo", Role = "Spartan" }
        };
        public Task<Character> AddCharacterAsync(Character character)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteCharacterAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Character>> GetAllCharactersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Character> GetCharacterByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdatedCharacterAsync(int id, Character character)
        {
            throw new NotImplementedException();
        }
    }
}

