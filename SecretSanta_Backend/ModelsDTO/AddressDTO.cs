using System;
using System.Collections.Generic;

namespace SecretSanta_Backend.ModelsDTO
{
    public partial class AddressDTO
    {
        public Guid MemberId { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string Zip { get; set; } = null!;
        public string Region { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string? Apartment { get; set; }

        public virtual MemberView Member { get; set; } = null!;
    }
}
