using Microsoft.EntityFrameworkCore;
using VerificarApi.Dtos.Vehicles;
using VerificarApi.Domain.Entities;
using VerificarApi.Infrastructure.Data;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly AppDbContext _context;

        public VehicleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VehicleResponseDto>> GetAllAsync()
        {
            return await _context.Vehicles
                .OrderBy(v => v.Marca)
                .ThenBy(v => v.Modelo)
                .ThenBy(v => v.Versao)
                .Select(v => new VehicleResponseDto
                {
                    Id = v.Id,
                    Marca = v.Marca,
                    Modelo = v.Modelo,
                    Versao = v.Versao,
                    Motorizacao = v.Motorizacao,
                    Aspiracao = v.Aspiracao,
                    Combustivel = v.Combustivel,
                    Cambio = v.Cambio,
                    Categoria = v.Categoria,
                    PaisOrigem = v.PaisOrigem,
                    Importado = v.Importado,
                    AnoFabricacao = v.AnoFabricacao,
                    AnoModelo = v.AnoModelo,
                    FipeValor = v.FipeValor,
                    FipeData = v.FipeData,
                    Descricao = v.Descricao,
                    PontosPositivos = v.PontosPositivos,
                    PontosNegativos = v.PontosNegativos,
                    ProblemasCronicos = v.ProblemasCronicos,
                    NotaConsumo = v.NotaConsumo,
                    NotaConforto = v.NotaConforto,
                    NotaDesempenho = v.NotaDesempenho,
                    NotaConfiabilidade = v.NotaConfiabilidade,
                    NotaCustoManutencao = v.NotaCustoManutencao,
                    NotaEspacoInterno = v.NotaEspacoInterno

                })
                .ToListAsync();
        }

        public async Task<VehicleResponseDto?> GetByIdAsync(Guid id)
        {
            return await _context.Vehicles
                .Where(v => v.Id == id)
                .Select(v => new VehicleResponseDto
                {
                    Id = v.Id,
                    Marca = v.Marca,
                    Modelo = v.Modelo,
                    Versao = v.Versao,
                    Motorizacao = v.Motorizacao,
                    Aspiracao = v.Aspiracao,
                    Combustivel = v.Combustivel,
                    Cambio = v.Cambio,
                    Categoria = v.Categoria,
                    PaisOrigem = v.PaisOrigem,
                    Importado = v.Importado,
                    AnoFabricacao = v.AnoFabricacao,
                    AnoModelo = v.AnoModelo,
                    FipeValor = v.FipeValor,
                    FipeData = v.FipeData,
                    Descricao = v.Descricao,
                    PontosPositivos = v.PontosPositivos,
                    PontosNegativos = v.PontosNegativos,
                    ProblemasCronicos = v.ProblemasCronicos,
                    NotaConsumo = v.NotaConsumo,
                    NotaConforto = v.NotaConforto,
                    NotaDesempenho = v.NotaDesempenho,
                    NotaConfiabilidade = v.NotaConfiabilidade,
                    NotaCustoManutencao = v.NotaCustoManutencao,
                    NotaEspacoInterno = v.NotaEspacoInterno
                })
                .FirstOrDefaultAsync();
        }

        public async Task<VehicleResponseDto> CreateAsync(CreateVehicleDto dto)
        {
            var vehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                Marca = dto.Marca,
                Modelo = dto.Modelo,
                Versao = dto.Versao,
                Motorizacao = dto.Motorizacao,
                Aspiracao = dto.Aspiracao,
                Combustivel = dto.Combustivel,
                Cambio = dto.Cambio,
                Categoria = dto.Categoria,
                PaisOrigem = dto.PaisOrigem,
                Importado = dto.Importado,
                AnoFabricacao = dto.AnoFabricacao,
                AnoModelo = dto.AnoModelo,
                FipeValor = dto.FipeValor,
                FipeData = dto.FipeData,
                Descricao = dto.Descricao,
                PontosPositivos = dto.PontosPositivos,
                PontosNegativos = dto.PontosNegativos,
                ProblemasCronicos = dto.ProblemasCronicos,
                NotaConsumo = dto.NotaConsumo,
                NotaConforto = dto.NotaConforto,
                NotaDesempenho = dto.NotaDesempenho,
                NotaConfiabilidade = dto.NotaConfiabilidade,
                NotaCustoManutencao = dto.NotaCustoManutencao,
                NotaEspacoInterno = dto.NotaEspacoInterno
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return new VehicleResponseDto
            {
                Id = vehicle.Id,
                Marca = vehicle.Marca,
                Modelo = vehicle.Modelo,
                Versao = vehicle.Versao,
                Motorizacao = vehicle.Motorizacao,
                Aspiracao = vehicle.Aspiracao,
                Combustivel = vehicle.Combustivel,
                Cambio = vehicle.Cambio,
                Categoria = vehicle.Categoria,
                PaisOrigem = vehicle.PaisOrigem,
                Importado = vehicle.Importado,
                AnoFabricacao = vehicle.AnoFabricacao,
                AnoModelo = vehicle.AnoModelo,
                FipeValor = vehicle.FipeValor,
                FipeData = vehicle.FipeData,
                Descricao = vehicle.Descricao,
                PontosPositivos = vehicle.PontosPositivos,
                PontosNegativos = vehicle.PontosNegativos,
                ProblemasCronicos = vehicle.ProblemasCronicos,
                NotaConsumo = vehicle.NotaConsumo,
                NotaConforto = vehicle.NotaConforto,
                NotaDesempenho = vehicle.NotaDesempenho,
                NotaConfiabilidade = vehicle.NotaConfiabilidade,
                NotaCustoManutencao = vehicle.NotaCustoManutencao,
                NotaEspacoInterno = vehicle.NotaEspacoInterno
            };
        }

        public async Task<VehicleResponseDto?> UpdateAsync(Guid id, UpdateVehicleDto dto)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
                return null;

            vehicle.Marca = dto.Marca;
            vehicle.Modelo = dto.Modelo;
            vehicle.Versao = dto.Versao;
            vehicle.Motorizacao = dto.Motorizacao;
            vehicle.Aspiracao = dto.Aspiracao;
            vehicle.Combustivel = dto.Combustivel;
            vehicle.Cambio = dto.Cambio;
            vehicle.Categoria = dto.Categoria;
            vehicle.PaisOrigem = dto.PaisOrigem;
            vehicle.Importado = dto.Importado;
            vehicle.AnoFabricacao = dto.AnoFabricacao;
            vehicle.AnoModelo = dto.AnoModelo;
            vehicle.FipeValor = dto.FipeValor;
            vehicle.FipeData = dto.FipeData;
            vehicle.Descricao = dto.Descricao;
            vehicle.PontosPositivos = dto.PontosPositivos;
            vehicle.PontosNegativos = dto.PontosNegativos;
            vehicle.ProblemasCronicos = dto.ProblemasCronicos;
            vehicle.NotaConsumo = dto.NotaConsumo;
            vehicle.NotaConforto = dto.NotaConforto;
            vehicle.NotaDesempenho = dto.NotaDesempenho;
            vehicle.NotaConfiabilidade = dto.NotaConfiabilidade;
            vehicle.NotaCustoManutencao = dto.NotaCustoManutencao;
            vehicle.NotaEspacoInterno = dto.NotaEspacoInterno;

            await _context.SaveChangesAsync();

            return new VehicleResponseDto
            {
                Id = vehicle.Id,
                Marca = vehicle.Marca,
                Modelo = vehicle.Modelo,
                Versao = vehicle.Versao,
                Motorizacao = vehicle.Motorizacao,
                Aspiracao = vehicle.Aspiracao,
                Combustivel = vehicle.Combustivel,
                Cambio = vehicle.Cambio,
                Categoria = vehicle.Categoria,
                PaisOrigem = vehicle.PaisOrigem,
                Importado = vehicle.Importado,
                AnoFabricacao = vehicle.AnoFabricacao,
                AnoModelo = vehicle.AnoModelo,
                FipeValor = vehicle.FipeValor,
                FipeData = vehicle.FipeData,
                Descricao = vehicle.Descricao,
                PontosPositivos = vehicle.PontosPositivos,
                PontosNegativos = vehicle.PontosNegativos,
                ProblemasCronicos = vehicle.ProblemasCronicos,
                NotaConsumo = vehicle.NotaConsumo,
                NotaConforto = vehicle.NotaConforto,
                NotaDesempenho = vehicle.NotaDesempenho,
                NotaConfiabilidade = vehicle.NotaConfiabilidade,
                NotaCustoManutencao = vehicle.NotaCustoManutencao,
                NotaEspacoInterno = vehicle.NotaEspacoInterno
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
                return false;

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}