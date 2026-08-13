using VerificarApi.Domain.Entities;
using VerificarApi.Dtos.RegistrationLinks;

namespace VerificarApi.Services.Interfaces
{
    public interface IRegistrationLinkService
    {
        Task<List<RegistrationLinkResponseDto>> ListarAsync();
        Task<RegistrationLinkResponseDto> CriarAsync(CreateRegistrationLinkDto dto);
        Task<RegistrationLinkResponseDto?> AtualizarAsync(Guid id, UpdateRegistrationLinkDto dto);
        Task<bool> ExcluirAsync(Guid id);
        Task<ValidarRegistrationLinkDto> ValidarAsync(string token);
        Task<RegistrationLink?> ClaimUsoAsync(string token);
    }
}
