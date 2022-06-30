namespace SecretSanta_Backend.ModelsDTO
{
    public class PreferencesPost
    {
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Zip { get; set; } = null!;
        public string Region { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string? Apartment { get; set; }
        public string? Preference { get; set; }
    }
}
