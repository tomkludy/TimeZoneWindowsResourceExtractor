namespace TZResScraper
{
    public class TimeZoneInfoEx
    {
        public int BaseUtcOffsetMinutes { get; set; }
        public string DaylightName { get; set; }
        public string DisplayName { get; set; }
        public string Id { get; set; }
        public string StandardName { get; set; }
        public bool SupportsDaylightSavingTime { get; set; }

        public bool Equals(TimeZoneInfoEx other)
        {
            return BaseUtcOffsetMinutes == other.BaseUtcOffsetMinutes &&
                DaylightName == other.DaylightName &&
                DisplayName == other.DisplayName &&
                Id == other.Id &&
                StandardName == other.StandardName &&
                SupportsDaylightSavingTime == other.SupportsDaylightSavingTime;
        }
    }
}
