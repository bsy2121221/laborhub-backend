using Labor.Models.DTOs.Labor;
using Labor.Models.Entities.Labor;

namespace Labor.DataAccess.IRepositories
{
    public interface ILaborRepository
    {
        Task<IEnumerable<LaborResponseDto>> SearchLaborsAsync(LaborSearchRequestDto request);
        Task<IEnumerable<LaborResponseDto>> GetAvailableLaborNearByTomorrowAsync(
            decimal? latitude,
            decimal? longitude,
            int radiusKm = 10,
            string? availabilityStatus = "Available",
            int pageNumber = 1,
            int pageSize = 20);
        Task<LaborResponseDto?> GetLaborDetailsAsync(int laborId);
        Task<IEnumerable<LaborType>> GetLaborTypesAsync();
        Task<int> CreateLaborProfileAsync(Laborer labor);
        Task<bool> UpdateLaborProfileAsync(Laborer labor);
        Task<bool> UpdateLaborAvailabilityStatusAsync(int laborId, string availabilityStatus);
        Task<IEnumerable<LaborSkill>> GetLaborSkillsAsync(int laborId);
        Task<bool> AddLaborSkillAsync(LaborSkill skill);
        Task<bool> RemoveLaborSkillAsync(int skillId);
        Task<bool> UpdateLaborAvailablityAsync(int laborId,string AvailabilityJson);
        Task<IEnumerable<LaborAvailabilityItemDto>> GetLaborAvailabilitiesByMonthAsync(int laborId,int year,int month);
    }
} 