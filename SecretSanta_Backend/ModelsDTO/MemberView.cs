using System;
using System.Collections.Generic;

namespace SecretSanta_Backend.ModelsDTO
{
    public partial class MemberView
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Patronymic { get; set; }
        public string? Email { get; set; }
    }
}
