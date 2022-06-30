using SecretSanta_Backend.Models;
using SecretSanta_Backend.Interfaces;

namespace SecretSanta_Backend.Repositories
{
    public class AddressRepository : RepositoryBase<Address>, IAddressRepository
    {
        public AddressRepository(ApplicationContext context) : base(context)
        {

        }

        public void CreateAddress(Address address) => Create(address);
        public void UpdateAddress(Address address) => Update(address);
        public void DeleteAddress(Address address) => Delete(address);

    }
}
