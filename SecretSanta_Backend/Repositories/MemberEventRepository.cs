using SecretSanta_Backend.Models;
using SecretSanta_Backend.Interfaces;

namespace SecretSanta_Backend.Repositories
{
    public class MemberEventRepository : RepositoryBase<MemberEvent>, IMemberEventRepository
    {
        public MemberEventRepository(ApplicationContext context) : base(context)
        {

        }

        public void CreateMemberEvent(MemberEvent memberEvent) => Create(memberEvent);
        public void UpdateMemberEvent(MemberEvent memberEvent) => Update(memberEvent);
        public void DeleteMemberEvent(MemberEvent memberEvent) => Delete(memberEvent);

    }
}
