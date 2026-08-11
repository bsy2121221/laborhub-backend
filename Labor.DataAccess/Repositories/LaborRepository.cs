using Dapper;
using Labor.DataAccess.Context;
using Labor.DataAccess.IRepositories;
using Labor.Models.DTOs.Labor;
using Labor.Models.Entities.Labor;
using System.Data;

namespace Labor.DataAccess.Repositories
{
    public class LaborRepository : ILaborRepository
    {
        private readonly IDbContext _context;

        public LaborRepository(IDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LaborResponseDto>> SearchLaborsAsync(LaborSearchRequestDto request)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@availabilityDate", request.availabilityDate);
            parameters.Add("@LaborTypeId", request.LaborTypeId);
            parameters.Add("@SearchText", request.SearchText);
            parameters.Add("@AvailabilityStatus", request.AvailabilityStatus);
            parameters.Add("@MinRating", request.MinRating);
            parameters.Add("@MaxDailyRate", request.MaxDailyRate);
            parameters.Add("@PageNumber", request.PageNumber);
            parameters.Add("@PageSize", request.PageSize);

            var results = await connection.QueryAsync<LaborResponseDto>(
                "[dbo].[sp_SearchLabors]",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            
            return results;
        }

        public async Task<IEnumerable<LaborResponseDto>> GetAvailableLaborNearByTomorrowAsync(
            decimal? latitude,
            decimal? longitude,
            int radiusKm = 10,
            string? availabilityStatus = "Available",
            int pageNumber = 1,
            int pageSize = 20)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Latitude", latitude?.ToString());
            parameters.Add("@Longitude", longitude?.ToString());
            parameters.Add("@RadiusKm", radiusKm.ToString());
            parameters.Add("@AvailabilityStatus", availabilityStatus);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);

            return await connection.QueryAsync<LaborResponseDto>(
                "[dbo].[sp_AvailableLaborNearByTomorrow]",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<LaborResponseDto?> GetLaborDetailsAsync(int laborId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@LaborId", laborId);

            using var multi = await connection.QueryMultipleAsync(
                "[dbo].[sp_GetLaborDetails]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var laborInfo = await multi.ReadFirstOrDefaultAsync<LaborResponseDto>();
            if (laborInfo == null) return null;

            var skills = await multi.ReadAsync<LaborSkillDto>();
            laborInfo.Skills = skills.ToList();

            var reviews = await multi.ReadAsync<LaborReviewDto>();
            laborInfo.RecentReviews = reviews.ToList();

            return laborInfo;
        }

        public async Task<IEnumerable<LaborType>> GetLaborTypesAsync()
        {
            using var connection = _context.CreateConnection();
            
            return await connection.QueryAsync<LaborType>(
                "[dbo].[sp_GetLaborTypes]",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> CreateLaborProfileAsync(Laborer labor)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", labor.UserId);
            parameters.Add("@LaborTypeId", labor.LaborTypeId);
            parameters.Add("@Specialization", labor.Specialization);
            parameters.Add("@ExperienceYears", labor.ExperienceYears);
            parameters.Add("@DailyRate", labor.DailyRate);
            parameters.Add("@MinimumHours", labor.MinimumHours);
            parameters.Add("@MaximumHours", labor.MaximumHours);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_CreateLaborProfile]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.LaborId;
        }

        public async Task<bool> UpdateLaborProfileAsync(Laborer labor)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", labor.Id);
            parameters.Add("@LaborTypeId", labor.LaborTypeId);
            parameters.Add("@Specialization", labor.Specialization);
            parameters.Add("@ExperienceYears", labor.ExperienceYears);
            parameters.Add("@DailyRate", labor.DailyRate);
            parameters.Add("@MinimumHours", labor.MinimumHours);
            parameters.Add("@MaximumHours", labor.MaximumHours);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_UpdateLaborProfile]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.RowsAffected > 0;
        }

        public async Task<bool> UpdateLaborAvailabilityStatusAsync(int laborId, string availabilityStatus)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@LaborId", laborId);
            parameters.Add("@AvailabilityStatus", availabilityStatus);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_UpdateLaborAvailability]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.RowsAffected > 0;
        }

        public async Task<IEnumerable<LaborSkill>> GetLaborSkillsAsync(int laborId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@LaborId", laborId);

            return await connection.QueryAsync<LaborSkill>(
                "[dbo].[sp_GetLaborSkills]",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> AddLaborSkillAsync(LaborSkill skill)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@LaborId", skill.LaborId);
            parameters.Add("@SkillName", skill.SkillName);
            parameters.Add("@ProficiencyLevel", skill.ProficiencyLevel);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_AddLaborSkill]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.RowsAffected > 0;
        }

        public async Task<bool> RemoveLaborSkillAsync(int skillId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@SkillId", skillId);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_RemoveLaborSkill]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.RowsAffected > 0;
        }

        public async Task<bool> UpdateLaborAvailablityAsync(int laborId,string AvailabilityJson)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@LaborId", laborId);
            parameters.Add("@AvailabilityJson", AvailabilityJson);
            string procName = "[dbo].[sp_UpsertLaborAvailabilities]";
            var result = await connection.QuerySingleAsync<dynamic>(
                procName,
                parameters,
                commandType: CommandType.StoredProcedure
                );
            return result.RowsAffected > 0;
            
        }

        public async Task<IEnumerable<LaborAvailabilityItemDto>> GetLaborAvailabilitiesByMonthAsync(int laborId, int year, int month)
        {
            using var connection=_context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@LaborId", laborId);
            parameters.Add("@Year", year);
            parameters.Add("@Month", month);
            string procName = "[dbo].[sp_GetLaborAvailabilitiesByMonth]";
            return await connection.QueryAsync<LaborAvailabilityItemDto>(
                procName,
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
} 