using VerificarApi.Dtos.Vehicles;

namespace VerificarApi.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<List<VehicleResponseDto>> GetAllAsync();
        Task<VehicleResponseDto?> GetByIdAsync(Guid id);
        Task<VehicleResponseDto> CreateAsync(CreateVehicleDto dto);
        Task<VehicleResponseDto?> UpdateAsync(Guid id, UpdateVehicleDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}