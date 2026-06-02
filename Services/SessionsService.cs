using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Tasqana.Repositories;

namespace Tasqana.Services
{
    public class SessionsService
    {
        private readonly SessionsRepository _sessionRepository;
        public SessionsService(SessionsRepository sessionsRepository) {
            _sessionRepository = sessionsRepository;
        }

        public async Task<IEnumerable<Models.Session>> GetAllAsync()
        {
            var result = await _sessionRepository
                .Query(true)
                .Include(c => c.User)
                .OrderByDescending(c => c.ExpiredAt)
                .ToListAsync();
            return result;
        }

        public async Task<Models.http.AuthResponseDTO> CreateSessionAsync(Models.User user, HttpContext context, int minutesToLive)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "";
            string userAgent = context.Request.Headers["User-Agent"].ToString();

            var token = GenerateSecureToken();
            var session = new Models.Session
            {
                UserId = user.Id,
                TokenHash = GetTokenHash(token),
                ExpiredAt = DateTime.UtcNow.AddMinutes(minutesToLive),
                Device = userAgent,
                IP = ip,
            };
            await _sessionRepository.InsertAsync(session);
            return new Models.http.AuthResponseDTO(user, token, session.ExpiredAt);
        }

        public async Task<Models.User?> FindSessionAsync(string token)
        {
            var hash = GetTokenHash(token);
            var session = await _sessionRepository.FindByHashAsync(hash);
            if (session == null) { return null; }
            if (session.ExpiredAt < DateTime.UtcNow) { return null; }
            return session.User;
        }

        private string GenerateSecureToken(int byteLength = 32)
        {
            byte[] randomBytes = new byte[byteLength];
            RandomNumberGenerator.Fill(randomBytes);
            return Convert.ToHexString(randomBytes).ToLower();
        }

        private string GetTokenHash(string rawToken)
        {
            // Хэшировать токены через SHA-256 в .NET 8/9 можно в одну строку без создания лишних объектов в памяти
            byte[] inputBytes = Encoding.UTF8.GetBytes(rawToken);
            byte[] hashBytes = SHA256.HashData(inputBytes);

            return Convert.ToHexString(hashBytes).ToLower();
        }


    }
}
