using Microsoft.EntityFrameworkCore;
using VerificarApi.Domain.Entities;
using VerificarApi.Dtos.SavedVehicles;
using VerificarApi.Infrastructure.Data;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Services
{
    public class SavedVehicleService : ISavedVehicleService
    {
        private readonly AppDbContext _context;

        public SavedVehicleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SavedVehicleResponseDto> SalvarAsync(Guid userId, CreateSavedVehicleDto dto)
        {
            // Se o usuário já salvou esse mesmo veículo (marca+modelo+ano+placa), atualiza o
            // snapshot em vez de duplicar — evita lixo na lista de veículos salvos.
            var existente = await _context.SavedVehicles.FirstOrDefaultAsync(v =>
                v.UserId == userId &&
                v.Marca == dto.Marca &&
                v.Modelo == dto.Modelo &&
                v.AnoModelo == dto.AnoModelo &&
                v.Placa == dto.Placa);

            SavedVehicle entidade;
            if (existente != null)
            {
                AplicarSnapshot(existente, dto);
                existente.SalvoEm = DateTime.UtcNow;
                entidade = existente;
            }
            else
            {
                entidade = new SavedVehicle
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Marca = dto.Marca,
                    Modelo = dto.Modelo,
                    AnoModelo = dto.AnoModelo,
                    Placa = dto.Placa,
                    SalvoEm = DateTime.UtcNow
                };
                AplicarSnapshot(entidade, dto);
                _context.SavedVehicles.Add(entidade);
            }

            await _context.SaveChangesAsync();
            return ParaDto(entidade);
        }

        public async Task<List<SavedVehicleResponseDto>> ListarAsync(Guid userId)
        {
            return await _context.SavedVehicles
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.SalvoEm)
                .Select(v => new SavedVehicleResponseDto
                {
                    Id = v.Id,
                    Marca = v.Marca,
                    Modelo = v.Modelo,
                    AnoModelo = v.AnoModelo,
                    Placa = v.Placa,
                    ValorFipe = v.ValorFipe,
                    ValorEstimado = v.ValorEstimado,
                    PercentualAjuste = v.PercentualAjuste,
                    NotaGeral = v.NotaGeral,
                    NotaConfiabilidade = v.NotaConfiabilidade,
                    NotaCustoManutencao = v.NotaCustoManutencao,
                    NotaConforto = v.NotaConforto,
                    NotaDesempenho = v.NotaDesempenho,
                    NotaConsumo = v.NotaConsumo,
                    NotaEspacoInterno = v.NotaEspacoInterno,
                    SalvoEm = v.SalvoEm
                })
                .ToListAsync();
        }

        public async Task<bool> RemoverAsync(Guid userId, Guid id)
        {
            var entidade = await _context.SavedVehicles.FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);
            if (entidade == null)
                return false;

            _context.SavedVehicles.Remove(entidade);
            await _context.SaveChangesAsync();
            return true;
        }

        private static void AplicarSnapshot(SavedVehicle entidade, CreateSavedVehicleDto dto)
        {
            entidade.ValorFipe = dto.ValorFipe;
            entidade.ValorEstimado = dto.ValorEstimado;
            entidade.PercentualAjuste = dto.PercentualAjuste;
            entidade.NotaGeral = dto.NotaGeral;
            entidade.NotaConfiabilidade = dto.NotaConfiabilidade;
            entidade.NotaCustoManutencao = dto.NotaCustoManutencao;
            entidade.NotaConforto = dto.NotaConforto;
            entidade.NotaDesempenho = dto.NotaDesempenho;
            entidade.NotaConsumo = dto.NotaConsumo;
            entidade.NotaEspacoInterno = dto.NotaEspacoInterno;
        }

        private static SavedVehicleResponseDto ParaDto(SavedVehicle v) => new()
        {
            Id = v.Id,
            Marca = v.Marca,
            Modelo = v.Modelo,
            AnoModelo = v.AnoModelo,
            Placa = v.Placa,
            ValorFipe = v.ValorFipe,
            ValorEstimado = v.ValorEstimado,
            PercentualAjuste = v.PercentualAjuste,
            NotaGeral = v.NotaGeral,
            NotaConfiabilidade = v.NotaConfiabilidade,
            NotaCustoManutencao = v.NotaCustoManutencao,
            NotaConforto = v.NotaConforto,
            NotaDesempenho = v.NotaDesempenho,
            NotaConsumo = v.NotaConsumo,
            NotaEspacoInterno = v.NotaEspacoInterno,
            SalvoEm = v.SalvoEm
        };
    }
}
