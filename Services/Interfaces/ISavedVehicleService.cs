using VerificarApi.Dtos.SavedVehicles;

namespace VerificarApi.Services.Interfaces
{
    public interface ISavedVehicleService
    {
        Task<SavedVehicleResponseDto> SalvarAsync(Guid userId, CreateSavedVehicleDto dto);
        Task<List<SavedVehicleResponseDto>> ListarAsync(Guid userId);
        Task<bool> RemoverAsync(Guid userId, Guid id);
    }
}
