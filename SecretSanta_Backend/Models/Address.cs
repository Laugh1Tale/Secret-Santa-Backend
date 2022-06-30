using System;
using System.Collections.Generic;

namespace SecretSanta_Backend.Models
{
    public partial class Address
    {
        public Guid Id { get; set; }
        public Guid MemberId { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string Zip { get; set; } = null!;
        public string Region { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string? Apartment { get; set; }

        public virtual Member Member { get; set; } = null!;
    }
}
