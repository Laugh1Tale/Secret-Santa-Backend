using SecretSanta_Backend.Models;
using SecretSanta_Backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SecretSanta_Backend.Repositories
{
    public class MemberRepository : RepositoryBase<Member>, IMemberRepository
    {
        public MemberRepository(ApplicationContext context) : base(context)
        {

        }

        public async Task<Member> GetMemberByIdAsync(Guid id)
        {
            return await FindByCondition(member => member.Id.Equals(id)).FirstOrDefaultAsync();
        }
        public async Task<Member> GetMemberByEmailAsync(string email)
        {
            return await FindByCondition(member => member.Email.Equals(email)).FirstOrDefaultAsync();
        }
        public void CreateMember(Member member) => Create(member);
        public void UpdateMember(Member member) => Update(member);
        public void DeleteMember(Member member) => Delete(member);
    }
}
