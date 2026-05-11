using VerificarApi.Dtos.Users;

namespace VerificarApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserResponseDto>> GetAllAsync();
        Task<UserResponseDto?> GetByIdAsync(Guid id);
        Task<UserResponseDto> CreateAsync(CreateUserDto dto);
        Task<UserResponseDto?> UpdateAsync(Guid id, UpdateUserDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}