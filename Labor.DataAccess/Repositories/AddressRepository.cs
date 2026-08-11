using Dapper;
using Labor.DataAccess.Context;
using Labor.DataAccess.IRepositories;
using Labor.Models.Entities.User;
using System.Data;

namespace Labor.DataAccess.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly IDbContext _context;

        public AddressRepository(IDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Address>> GetUserAddressesAsync(int userId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            return await connection.QueryAsync<Address>(
                "[dbo].[sp_GetUserAddresses]",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<Address?> GetAddressByIdAsync(int addressId, int userId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@AddressId", addressId);
            parameters.Add("@UserId", userId);

            return await connection.QueryFirstOrDefaultAsync<Address>(
                "[dbo].[sp_GetAddressById]",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> CreateAddressAsync(Address address)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", address.UserId);
            parameters.Add("@AddressType", address.AddressType);
            parameters.Add("@Street", address.Street);
            parameters.Add("@City", address.City);
            parameters.Add("@State", address.State);
            parameters.Add("@Country", address.Country);
            parameters.Add("@ZipCode", address.ZipCode);
            parameters.Add("@Latitude", address.Latitude);
            parameters.Add("@Longitude", address.Longitude);
            parameters.Add("@IsDefault", address.IsDefault);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_CreateAddress]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.AddressId;
        }

        public async Task<bool> UpdateAddressAsync(Address address)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@AddressId", address.Id);
            parameters.Add("@UserId", address.UserId);
            parameters.Add("@AddressType", address.AddressType);
            parameters.Add("@Street", address.Street);
            parameters.Add("@City", address.City);
            parameters.Add("@State", address.State);
            parameters.Add("@Country", address.Country);
            parameters.Add("@ZipCode", address.ZipCode);
            parameters.Add("@Latitude", address.Latitude);
            parameters.Add("@Longitude", address.Longitude);
            parameters.Add("@IsDefault", address.IsDefault);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_UpdateAddress]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.RowsAffected > 0;
        }

        public async Task<bool> DeleteAddressAsync(int addressId, int userId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@AddressId", addressId);
            parameters.Add("@UserId", userId);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_DeleteAddress]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.RowsAffected > 0;
        }

        public async Task<bool> SetDefaultAddressAsync(int addressId, int userId)
        {
            using var connection = _context.CreateConnection();
            
            // Use the update address stored procedure with IsDefault = true
            var address = await GetAddressByIdAsync(addressId, userId);
            if (address == null) return false;

            address.IsDefault = true;
            return await UpdateAddressAsync(address);
        }

        public async Task<Address?> GetDefaultAddressAsync(int userId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            return await connection.QueryFirstOrDefaultAsync<Address>(
                "[dbo].[sp_GetDefaultAddress]",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
} 