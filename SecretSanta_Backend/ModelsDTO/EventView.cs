using SecretSanta_Backend.ModelsDTO;
using System;
using System.Collections.Generic;

namespace SecretSanta_Backend.Models
{
    public partial class EventView
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public DateTime EndRegistration { get; set; }
        public DateTime? EndEvent { get; set; }
        public int? SumPrice { get; set; }
        public bool? SendFriends { get; set; }
        public bool? Tracking { get; set; }
        public bool? Reshuffle { get; set; }
        public int? MembersCount { get; set; }
        public List<MemberViewAdmin>? MemberView { get; set; }
    }
    public class MemberViewAdmin
    {
        public MemberView? MemberView { get; set; }
        public MemberView? MemberRecipient { get; set; }
        public MemberView? MemberSender { get; set; }
    }
}