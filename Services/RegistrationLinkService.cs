using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using VerificarApi.Domain.Entities;
using VerificarApi.Dtos.RegistrationLinks;
using VerificarApi.Infrastructure.Data;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Services
{
    public class RegistrationLinkService : IRegistrationLinkService
    {
        private readonly AppDbContext _context;

        public RegistrationLinkService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RegistrationLinkResponseDto>> ListarAsync()
        {
            var links = await _context.RegistrationLinks
                .OrderByDescending(l => l.CriadoEm)
                .ToListAsync();

            return links.Select(Mapear).ToList();
        }

        public async Task<RegistrationLinkResponseDto> CriarAsync(CreateRegistrationLinkDto dto)
        {
            var link = new RegistrationLink
            {
                Id = Guid.NewGuid(),
                Token = GerarToken(),
                Nome = dto.Nome,
                QtdConsultaModelo = dto.QtdConsultaModelo,
                QtdConsultaPlaca = dto.QtdConsultaPlaca,
                Ativo = true,
                LimiteUsos = dto.LimiteUsos,
                UsosAtuais = 0,
                ExpiraEm = Normalizar(dto.ExpiraEm),
                CriadoEm = DateTime.UtcNow
            };

            _context.RegistrationLinks.Add(link);
            await _context.SaveChangesAsync();

            return Mapear(link);
        }

        public async Task<RegistrationLinkResponseDto?> AtualizarAsync(Guid id, UpdateRegistrationLinkDto dto)
        {
            var link = await _context.RegistrationLinks.FirstOrDefaultAsync(l => l.Id == id);

            if (link == null)
                return null;

            link.Nome = dto.Nome;
            link.QtdConsultaModelo = dto.QtdConsultaModelo;
            link.QtdConsultaPlaca = dto.QtdConsultaPlaca;
            link.Ativo = dto.Ativo;
            link.LimiteUsos = dto.LimiteUsos;
            link.ExpiraEm = Normalizar(dto.ExpiraEm);

            await _context.SaveChangesAsync();

            return Mapear(link);
        }

        public async Task<bool> ExcluirAsync(Guid id)
        {
            var link = await _context.RegistrationLinks.FirstOrDefaultAsync(l => l.Id == id);

            if (link == null)
                return false;

            _context.RegistrationLinks.Remove(link);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ValidarRegistrationLinkDto> ValidarAsync(string token)
        {
            var tokenNorm = token?.Trim().ToUpperInvariant() ?? string.Empty;
            var link = await _context.RegistrationLinks.FirstOrDefaultAsync(l => l.Token == tokenNorm);

            if (link == null)
                return new ValidarRegistrationLinkDto { Token = tokenNorm, Valido = false, Status = "nao_encontrado" };

            if (!link.Ativo)
                return new ValidarRegistrationLinkDto
                {
                    Token = tokenNorm,
                    Nome = link.Nome,
                    QtdConsultaModelo = link.QtdConsultaModelo,
                    QtdConsultaPlaca = link.QtdConsultaPlaca,
                    LimiteUsos = link.LimiteUsos,
                    UsosAtuais = link.UsosAtuais,
                    Valido = false,
                    Status = "inativo"
                };

            var expira = link.ExpiraEm.HasValue
                ? DateTime.SpecifyKind(link.ExpiraEm.Value, DateTimeKind.Utc)
                : (DateTime?)null;

            if (expira.HasValue && expira.Value <= DateTime.UtcNow)
                return new ValidarRegistrationLinkDto
                {
                    Token = tokenNorm,
                    Nome = link.Nome,
                    QtdConsultaModelo = link.QtdConsultaModelo,
                    QtdConsultaPlaca = link.QtdConsultaPlaca,
                    LimiteUsos = link.LimiteUsos,
                    UsosAtuais = link.UsosAtuais,
                    Valido = false,
                    Status = "expirado"
                };

            if (link.LimiteUsos > 0 && link.UsosAtuais >= link.LimiteUsos)
                return new ValidarRegistrationLinkDto
                {
                    Token = tokenNorm,
                    Nome = link.Nome,
                    QtdConsultaModelo = link.QtdConsultaModelo,
                    QtdConsultaPlaca = link.QtdConsultaPlaca,
                    LimiteUsos = link.LimiteUsos,
                    UsosAtuais = link.UsosAtuais,
                    Valido = false,
                    Status = "esgotado"
                };

            return new ValidarRegistrationLinkDto
            {
                Token = tokenNorm,
                Nome = link.Nome,
                QtdConsultaModelo = link.QtdConsultaModelo,
                QtdConsultaPlaca = link.QtdConsultaPlaca,
                LimiteUsos = link.LimiteUsos,
                UsosAtuais = link.UsosAtuais,
                Valido = true,
                Status = "valido"
            };
        }

        /// <summary>
        /// Reserva atomicamente uma vaga de cadastro no link, incrementando
        /// UsosAtuais. Retorna o link se houver vaga, ou null caso contrário.
        /// Deve ser chamado dentro de uma transação para reverter em falha.
        /// </summary>
        public async Task<RegistrationLink?> ClaimUsoAsync(string token)
        {
            var tokenNorm = token?.Trim().ToUpperInvariant() ?? string.Empty;

            var atualizados = await _context.RegistrationLinks
                .Where(l => l.Token == tokenNorm
                    && l.Ativo
                    && (l.ExpiraEm == null || l.ExpiraEm > DateTime.UtcNow)
                    && (l.LimiteUsos == 0 || l.UsosAtuais < l.LimiteUsos))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(l => l.UsosAtuais, l => l.UsosAtuais + 1));

            if (atualizados == 0)
                return null;

            return await _context.RegistrationLinks.FirstOrDefaultAsync(l => l.Token == tokenNorm);
        }

        private static DateTime? Normalizar(DateTime? data)
        {
            if (!data.HasValue)
                return null;

            return data.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(data.Value, DateTimeKind.Utc)
                : data.Value.ToUniversalTime();
        }

        private static DateTime? ParaUtc(DateTime? data) =>
            data.HasValue ? DateTime.SpecifyKind(data.Value, DateTimeKind.Utc) : null;

        private static string GerarToken()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var bytes = RandomNumberGenerator.GetBytes(8);
            var sb = new StringBuilder(8);
            foreach (var b in bytes)
                sb.Append(chars[b % chars.Length]);
            return sb.ToString();
        }

        private static RegistrationLinkResponseDto Mapear(RegistrationLink l) => new()
        {
            Id = l.Id,
            Token = l.Token,
            Nome = l.Nome,
            QtdConsultaModelo = l.QtdConsultaModelo,
            QtdConsultaPlaca = l.QtdConsultaPlaca,
            Ativo = l.Ativo,
            LimiteUsos = l.LimiteUsos,
            UsosAtuais = l.UsosAtuais,
            ExpiraEm = ParaUtc(l.ExpiraEm),
            CriadoEm = l.CriadoEm
        };
    }
}
