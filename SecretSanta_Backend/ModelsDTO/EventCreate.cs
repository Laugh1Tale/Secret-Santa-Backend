using System;
using System.Collections.Generic;

namespace SecretSanta_Backend.ModelsDTO
{
    public partial class EventCreate
    {
        public string? Description { get; set; }
        public DateTime EndRegistration { get; set; }
        public DateTime? EndEvent { get; set; }
        public int? Sumprice { get; set; }
        public bool? Sendfriends { get; set; }
        public bool? Tracking { get; set; }
    }
}
