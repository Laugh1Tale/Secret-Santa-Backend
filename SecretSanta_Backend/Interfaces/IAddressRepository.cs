using SecretSanta_Backend.Models;

namespace SecretSanta_Backend.Interfaces
{
    public interface IAddressRepository : IRepositoryBase<Address>
    {
        void CreateAddress(Address address);
        void DeleteAddress(Address address);
        void UpdateAddress(Address address);

    }
}
