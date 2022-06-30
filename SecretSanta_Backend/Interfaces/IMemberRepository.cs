using SecretSanta_Backend.Models;

namespace SecretSanta_Backend.Interfaces
{
    public interface IMemberRepository : IRepositoryBase<Member>
    {
        Task<Member> GetMemberByIdAsync(Guid id);
        Task<Member> GetMemberByEmailAsync(string email);
        void CreateMember(Member member);
        void DeleteMember(Member member);
        void UpdateMember(Member member);
    }
}
