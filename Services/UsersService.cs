using Microsoft.EntityFrameworkCore;
using System.Drawing;
using WebApi.Repositories;

namespace WebApi.Services
{
    public class UsersService
    {
        private readonly UsersRepository _usersRepository;
        public UsersService(UsersRepository usersRepository)
        {  _usersRepository = usersRepository; }

        public async Task<Models.User> InsertAsync(long telegramId, String name, string? username)
        {
            var user = new Models.User
            { 
                Name = name,
                TelegramId = telegramId,
                TelegramUsername = username,
            };
            return await _usersRepository.InsertAsync(user);
        }   

        public async Task<IEnumerable<Models.User>> GetAllAsync()
        {
            return await _usersRepository.GetAllAsync();
        }

        public async Task<Models.User?> GetByTelegramIdAsync(long telegramId)
        {
            return await _usersRepository.GetByTelegramIdAsync(telegramId);
        }

    }
}
