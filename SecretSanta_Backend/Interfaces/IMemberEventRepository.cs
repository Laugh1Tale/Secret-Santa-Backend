using SecretSanta_Backend.Models;

namespace SecretSanta_Backend.Interfaces
{
    public interface IMemberEventRepository : IRepositoryBase<MemberEvent>
    {
        void CreateMemberEvent(MemberEvent memberEvent);
        void UpdateMemberEvent(MemberEvent memberEvent);
        void DeleteMemberEvent(MemberEvent memberEvent);
    }
}
