using Labor.Models.Entities.User;

namespace Labor.DataAccess.IRepositories
{
    public interface IAddressRepository
    {
        Task<IEnumerable<Address>> GetUserAddressesAsync(int userId);
        Task<Address?> GetAddressByIdAsync(int addressId, int userId);
        Task<int> CreateAddressAsync(Address address);
        Task<bool> UpdateAddressAsync(Address address);
        Task<bool> DeleteAddressAsync(int addressId, int userId);
        Task<bool> SetDefaultAddressAsync(int addressId, int userId);
        Task<Address?> GetDefaultAddressAsync(int userId);
    }
} 