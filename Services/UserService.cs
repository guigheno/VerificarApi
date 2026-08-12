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

        public UserService(AppDbContext context)
        {
            _context = context;
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
                    QtdConsultaModelo = u.QtdConsultaModelo,
                    QtdConsultaPlaca = u.QtdConsultaPlaca
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
                    QtdConsultaModelo = u.QtdConsultaModelo,
                    QtdConsultaPlaca = u.QtdConsultaPlaca
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
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                QtdConsultaModelo = user.QtdConsultaModelo,
                QtdConsultaPlaca = user.QtdConsultaPlaca
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
                QtdConsultaModelo = user.QtdConsultaModelo,
                QtdConsultaPlaca = user.QtdConsultaPlaca
            };
        }

        public async Task<UserResponseDto?> AdicionarConsultasAsync(Guid id, AdicionarConsultasDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;

            if (dto.QtdConsultaModelo < 0 || dto.QtdConsultaPlaca < 0)
                throw new Exception("As quantidades não podem ser negativas.");

            user.QtdConsultaModelo += dto.QtdConsultaModelo;
            user.QtdConsultaPlaca += dto.QtdConsultaPlaca;

            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                QtdConsultaModelo = user.QtdConsultaModelo,
                QtdConsultaPlaca = user.QtdConsultaPlaca
            };
        }

        public async Task<int> ConsultasRestantesAsync(Guid userId, TipoConsulta tipo)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new Exception("Usuário não encontrado.");

            // Admin não tem limite de consultas.
            if (user.Role == "Admin")
                return int.MaxValue;

            return tipo == TipoConsulta.Modelo ? user.QtdConsultaModelo : user.QtdConsultaPlaca;
        }

        public async Task<int> ConsumirConsultaAsync(Guid userId, TipoConsulta tipo)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new Exception("Usuário não encontrado.");

            // Admin não consome créditos de consulta.
            if (user.Role == "Admin")
                return int.MaxValue;

            var limiteAtingido = tipo == TipoConsulta.Modelo
                ? user.QtdConsultaModelo <= 0
                : user.QtdConsultaPlaca <= 0;

            if (limiteAtingido)
                throw new InvalidOperationException("Limite de consultas atingido.");

            if (tipo == TipoConsulta.Modelo)
                user.QtdConsultaModelo--;
            else
                user.QtdConsultaPlaca--;

            await _context.SaveChangesAsync();

            return tipo == TipoConsulta.Modelo ? user.QtdConsultaModelo : user.QtdConsultaPlaca;
        }

        public async Task<UserResponseDto?> ObterPerfilAsync(Guid userId)
        {
            return await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    QtdConsultaModelo = u.QtdConsultaModelo,
                    QtdConsultaPlaca = u.QtdConsultaPlaca
                })
                .FirstOrDefaultAsync();
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