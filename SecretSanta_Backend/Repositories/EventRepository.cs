using Microsoft.EntityFrameworkCore;
using System.Linq;
using SecretSanta_Backend.Models;
using SecretSanta_Backend.Interfaces;

namespace SecretSanta_Backend.Repositories
{
    public class EventRepository : RepositoryBase<Event>, IEventRepository
    {
        public EventRepository(ApplicationContext context) : base(context)
        {

        }

        public List<Event> GetEventsByMemberId(Guid memberId)
        {
            return context.Set<MemberEvent>()
                .Where(x => x.MemberId == memberId)
                .Select(x => x.Event)
                .ToList();
        }
        public void CreateEvent(Event @event) => Create(@event);
        public void UpdateEvent(Event @event) => Update(@event);
        public void DeleteEvent(Event @event) => Delete(@event);

    }
}
