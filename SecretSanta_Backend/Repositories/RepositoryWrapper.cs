using Microsoft.EntityFrameworkCore;
using SecretSanta_Backend.Interfaces;
using SecretSanta_Backend.Models;

namespace SecretSanta_Backend.Repositories
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private ApplicationContext context;
        private IMemberRepository? member;
        private IEventRepository? @event;
        private IMemberEventRepository? memberEvent;
        private IAddressRepository? address;

        public IMemberRepository Member
        {
            get
            {
                if (member == null)
                {
                    member = new MemberRepository(context);
                }
                return member;
            }
        }

        public IEventRepository Event
        {
            get
            {
                if (@event == null)
                {
                    @event = new EventRepository(context);
                }
                return @event;
            }
        }

        public IMemberEventRepository MemberEvent
        {
            get
            {
                if (memberEvent == null)
                {
                    memberEvent = new MemberEventRepository(context);
                }
                return memberEvent;
            }
        }

        public IAddressRepository Address
        {
            get
            {
                if (address == null)
                {
                    address = new AddressRepository(context);
                }
                return address;
            }
        }

        public RepositoryWrapper(ApplicationContext context)
        {
            this.context = context;
        }

        public RepositoryWrapper()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            context = new ApplicationContext(optionsBuilder.Options);
        }

        public async Task Save()
        {
            await context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
