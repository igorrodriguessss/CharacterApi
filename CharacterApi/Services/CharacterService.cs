using CharacterApi.Data;
using CharacterApi.Models;
using Microsoft.EntityFrameworkCore;
using CharacterApi.Dtos;
namespace CharacterApi.Services
{
    public class CharacterService(AppDbContext context) : ICharacterService
    {


        
        public async Task<CharacterResponse> AddCharacterAsync(CreateCharacterRequest character)
        {
            var newCharacter = new Character
            {
                Name = character.Name,
                Game = character.Game,
                Role = character.Role
            };
            context.Characters.Add(newCharacter);
            await context.SaveChangesAsync();

            return new CharacterResponse
            {
                Id = newCharacter.Id,
                Name = newCharacter.Name,
                Game = newCharacter.Game,
                Role = newCharacter.Role
            };

        }

        public async Task<bool> DeleteCharacterAsync(int id)
        {
            var usuarioParaDeletar = await context.Characters.FindAsync(id);
            if (usuarioParaDeletar is null)
            {
                return false;
            }
            context.Characters.Remove(usuarioParaDeletar);
            await context.SaveChangesAsync();

            return true;

        }

        public async Task<List<CharacterResponse>> GetAllCharactersAsync() => await context.Characters.Select(c => new CharacterResponse
        {
            Id = c.Id,
            Name = c.Name,
            Game = c.Game,
            Role = c.Role
        }).ToListAsync();

        public async Task<CharacterResponse?> GetCharacterByIdAsync(int id)
        {
            var result = await context.Characters.Where(c => c.Id == id).Select(c => new CharacterResponse
            {
                Id = c.Id,
                Name = c.Name,
                Game = c.Game,
                Role = c.Role
            }).FirstOrDefaultAsync();

            return result; 
        }

        public async Task<bool> UpdatedCharacterAsync(int id, UpdateCharacterRequest character)
        {
            var existente = await context.Characters.FindAsync(id);
            if(existente is null)
            {
                return false;
            }
            existente.Name = character.Name;
            existente.Game = character.Game;
            existente.Role= character.Role;

            await context.SaveChangesAsync();

            return true;
        }
    }
}

