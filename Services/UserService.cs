using Microsoft.EntityFrameworkCore;
using VerificarApi.Dtos.Users;
using VerificarApi.Domain.Entities;
using VerificarApi.Infrastructure.Data;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public UserService(AppDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<List<UserResponseDto>> GetAllAsync()
        {
            return await _context.Users
                .OrderBy(u => u.Name)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    TenantId = u.TenantId
                })
                .ToListAsync();
        }

        public async Task<UserResponseDto?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    TenantId = u.TenantId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
        {
            var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
                throw new Exception("Já existe um usuário com este e-mail.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role, // trata se vier vazio
                TenantId = _tenantProvider.GetTenantId()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                TenantId = user.TenantId
            };
        }

        public async Task<UserResponseDto?> UpdateAsync(Guid id, UpdateUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;

            var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id);
            if (emailExists)
                throw new Exception("Já existe um usuário com este e-mail.");

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.Role = dto.Role;

            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                TenantId = user.TenantId
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}