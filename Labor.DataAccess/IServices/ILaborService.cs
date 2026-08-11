using Labor.Models.DTOs.Labor;
using Labor.Models.Entities.Labor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.DataAccess.IServices
{
    public interface ILaborService
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
        Task<bool> UpsertLaborAvailabilitiesAsync(int laborId, List<LaborAvailabilityItemDto> items);
        Task<IEnumerable<LaborAvailabilityItemDto>> GetLaborAvailabilitiesByMonthAsync(int laborId, int year, int month);
    }
}
