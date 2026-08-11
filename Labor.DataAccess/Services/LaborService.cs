using Labor.DataAccess.IRepositories;
using Labor.DataAccess.IServices;
using Labor.Models.DTOs.Labor;
using Labor.Models.Entities.Labor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Labor.DataAccess.Services
{
    public class LaborService : ILaborService
    {
        private readonly ILaborRepository _laborRepository;

        public LaborService(ILaborRepository laborRepository)
        {
            _laborRepository = laborRepository;
        }
        public Task<bool> AddLaborSkillAsync(LaborSkill skill)
        {
            return _laborRepository.AddLaborSkillAsync(skill);
        }

        public Task<int> CreateLaborProfileAsync(Laborer labor)
        {
            return _laborRepository.CreateLaborProfileAsync(labor);
        }

        public Task<IEnumerable<LaborAvailabilityItemDto>> GetLaborAvailabilitiesByMonthAsync(int laborId, int year, int month)
        {
          return _laborRepository.GetLaborAvailabilitiesByMonthAsync(laborId, year, month);
        }

        public Task<LaborResponseDto?> GetLaborDetailsAsync(int laborId)
        {
            return _laborRepository.GetLaborDetailsAsync(laborId);
        }

        public Task<IEnumerable<LaborSkill>> GetLaborSkillsAsync(int laborId)
        {
           return _laborRepository.GetLaborSkillsAsync(laborId);
        }

        public Task<IEnumerable<LaborType>> GetLaborTypesAsync()
        {
            return _laborRepository.GetLaborTypesAsync();
        }

        public Task<bool> RemoveLaborSkillAsync(int skillId)
        {
            return _laborRepository.RemoveLaborSkillAsync(skillId);
        }

        public Task<IEnumerable<LaborResponseDto>> SearchLaborsAsync(LaborSearchRequestDto request)
        {
            return _laborRepository.SearchLaborsAsync(request); 
        }

        public Task<IEnumerable<LaborResponseDto>> GetAvailableLaborNearByTomorrowAsync(
            decimal? latitude,
            decimal? longitude,
            int radiusKm = 10,
            string? availabilityStatus = "Available",
            int pageNumber = 1,
            int pageSize = 20)
        {
            return _laborRepository.GetAvailableLaborNearByTomorrowAsync(
                latitude,
                longitude,
                radiusKm,
                availabilityStatus,
                pageNumber,
                pageSize);
        }

        public Task<bool> UpdateLaborAvailabilityStatusAsync(int laborId, string availabilityStatus)
        {
           return _laborRepository.UpdateLaborAvailabilityStatusAsync(laborId, availabilityStatus);
        }

        public Task<bool> UpdateLaborProfileAsync(Laborer labor)
        {
            return _laborRepository.UpdateLaborProfileAsync(labor);
        }

        public async Task<bool> UpsertLaborAvailabilitiesAsync(int laborId, List<LaborAvailabilityItemDto> items)
        {
            var payload = items.Select(x => new
            {
                AvailableDate = x.AvailableDate.Date.ToString("yyyy-MM-dd"),
                Status = x.Status?.Trim(),
                StartTime = x.StartTime?.ToString(@"hh\:mm\:ss"),
                EndTime = x.EndTime?.ToString(@"hh\:mm\:ss")
            });
            var availabilityJson=JsonSerializer.Serialize(payload);
            return await _laborRepository.UpdateLaborAvailablityAsync(laborId, availabilityJson);

        }
    }
}
