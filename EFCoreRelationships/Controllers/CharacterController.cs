using Microsoft.AspNetCore.Mvc;

namespace EFCoreRelationships.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharacterController : ControllerBase
    {
        private readonly DataContext _context;

        public CharacterController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Character>>> Get(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found.");
            var characters = await _context.Characters
                .Where(c => c.UserId == userId)
                .Include(c => c.Weapon)
                .Include(c => c.Skills)
                .ToListAsync();

            return characters;
        }

        [HttpPost]
        public async Task<ActionResult<List<Character>>> Create(CharacterDTO characterDTO)
        {
            var user = await _context.Users.FindAsync(characterDTO.UserId);
            if (user == null) return NotFound("User not found.");

            var newCharacter = new Character
            {
                Name = characterDTO.Name,
                RpgClass = characterDTO.RpgClass,
                User = user
            };

            _context.Characters.Add(newCharacter);
            await _context.SaveChangesAsync();

            return await Get(newCharacter.UserId);
        }

        [HttpPost("weapon")]
        public async Task<ActionResult<Character>> AddWeapon(WeaponDTO weaponDTO)
        {
            var character = await _context.Characters.FindAsync(weaponDTO.CharacterId);
            if (character == null) return NotFound("Character not found.");

            var newWeapon = new Weapon
            {
                Name = weaponDTO.Name,
                Damage = weaponDTO.Damage,
                Character = character
            };

            _context.Weapons.Add(newWeapon);
            await _context.SaveChangesAsync();

            return character;
        }

        [HttpPost("skill")]
        public async Task<ActionResult<Character>> AddCharacterSkill(AddCharacterSkillDTO request)
        {
            var character = await _context.Characters.Where(c => c.Id == request.CharacterId).Include(c => c.Skills).FirstOrDefaultAsync();
            if (character == null) return NotFound();

            var skill = await _context.Skills.FindAsync(request.SkillId);
            if (skill == null) return NotFound();

            character.Skills.Add(skill);
            await _context.SaveChangesAsync();

            return character;
        }
    }
}