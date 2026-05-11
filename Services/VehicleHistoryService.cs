using Microsoft.EntityFrameworkCore;
using VerificarApi.Domain.Entities;
using VerificarApi.Dtos.Analise;
using VerificarApi.Infrastructure.Data;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Services
{
    public class VehicleHistoryService : IVehicleHistoryService
    {
        private readonly AppDbContext _context;

        public VehicleHistoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VehicleHistoryDto>> ListarHistoricoAsync(string marca, string modelo, int ano, string? placa = null)
        {
            var query = _context.VehicleHistories
                .Where(h =>
                    EF.Functions.Like(h.Marca, $"%{marca}%") &&
                    EF.Functions.Like(h.Modelo, $"%{modelo}%") &&
                    h.AnoModelo == ano);

            if (!string.IsNullOrWhiteSpace(placa))
                query = query.Where(h => h.Placa == placa.ToUpper());

            return await query
                .OrderByDescending(h => h.DataEvento)
                .Select(h => new VehicleHistoryDto
                {
                    Id = h.Id,
                    TipoEvento = h.TipoEvento,
                    Descricao = h.Descricao,
                    DataEvento = h.DataEvento,
                    Fonte = h.Fonte
                })
                .ToListAsync();
        }

        public async Task<VehicleHistoryDto> AdicionarEventoAsync(CreateHistoryDto dto)
        {
            var evento = new VehicleHistory
            {
                Id = Guid.NewGuid(),
                Marca = dto.Marca.Trim(),
                Modelo = dto.Modelo.Trim(),
                AnoModelo = dto.AnoModelo,
                Placa = dto.Placa?.Trim().ToUpper(),
                TipoEvento = dto.TipoEvento,
                Descricao = dto.Descricao,
                DataEvento = dto.DataEvento,
                Fonte = "Manual",
                CriadoEm = DateTime.UtcNow
            };

            await _context.VehicleHistories.AddAsync(evento);
            await _context.SaveChangesAsync();

            return new VehicleHistoryDto
            {
                Id = evento.Id,
                TipoEvento = evento.TipoEvento,
                Descricao = evento.Descricao,
                DataEvento = evento.DataEvento,
                Fonte = evento.Fonte
            };
        }
    }
}
