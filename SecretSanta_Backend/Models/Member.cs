using System;
using System.Collections.Generic;

namespace SecretSanta_Backend.Models
{
    public partial class Member
    {
        public Member()
        {
            Addresses = new HashSet<Address>();
            MemberEvents = new HashSet<MemberEvent>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string Patronymic { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Role { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<MemberEvent> MemberEvents { get; set; }
    }
}
