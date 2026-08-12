using VerificarApi.Dtos.Users;

namespace VerificarApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserResponseDto>> GetAllAsync();
        Task<UserResponseDto?> GetByIdAsync(Guid id);
        Task<UserResponseDto> CreateAsync(CreateUserDto dto);
        Task<UserResponseDto?> UpdateAsync(Guid id, UpdateUserDto dto);
        Task<UserResponseDto?> AdicionarConsultasAsync(Guid id, AdicionarConsultasDto dto);
        Task<int> ConsultasRestantesAsync(Guid userId, TipoConsulta tipo);
        Task<int> ConsumirConsultaAsync(Guid userId, TipoConsulta tipo);
        Task<UserResponseDto?> ObterPerfilAsync(Guid userId);
        Task<bool> DeleteAsync(Guid id);
    }
}