namespace PowerShell.Time
{
    using System;
    using NodaTime;
    using NodaTime.TimeZones;

    public static class DateTimeExtensions
    {
        /// <summary>
        /// A ZoneLocalMappingResolver which never throws an exception due to ambiguity or skipped time.
        /// Ambiguity is handled by returning the earlier occurrence, and skipped times are shifted forward by the duration of the gap. This resolver combines ReturnEarlier and ReturnForwardShifted. 
        /// </summary>
        private static readonly ZoneLocalMappingResolver DefaultResolver = Resolvers.CreateMappingResolver(Resolvers.ReturnLater, Resolvers.ReturnForwardShifted);

        /// <summary>
        /// Converts a UTC time to a time on the specified Time Zone.
        /// </summary>
        /// <param name="time">The UTC DateTime with DateTimeKind.UTC to convert.</param>
        /// <param name="timeZone">The Time Zone ID of the target Time Zone.</param>
        /// <returns>A DateTime with DateTimeKind.Unspecified in the target Time Zone.</returns>
        public static DateTime InTimeZone(this DateTime time, string timeZone)
        {
            Ensure.IsNotNull(time, nameof(time));
            Ensure.DateTimeKindIsUtc(time, nameof(time));

            var utcDateTimeOffset = new DateTimeOffset(time);
            return utcDateTimeOffset.InTimeZone(timeZone).DateTime;
        }

        /// <summary>
        /// Converts a DateTimeOffset to a DateTimeOffset in the specified Time Zone.
        /// </summary>
        /// <param name="time">The DateTimeOffset to convert.</param>
        /// <param name="timeZone">The Time Zone ID of the target Time Zone.</param>
        /// <returns>A DateTimeOffset with an offset of the target Time Zone.</returns>
        public static DateTimeOffset InTimeZone(this DateTimeOffset time, string timeZone)
        {
            Ensure.IsNotNull(time, "time");

            var instant = Instant.FromDateTimeOffset(time);
            var dateTimeZone = DateTimeZoneProviders.Tzdb[timeZone];
            var inZone = instant.InZone(dateTimeZone);
            return inZone.ToDateTimeOffsetSafe();
        }

        public static DateTimeOffset ToDateTimeOffsetInTimeZone(
            this DateTime time,
            string timeZone)
        {
            Ensure.IsNotNull(time, "time");
            Ensure.DateTimeKindIsUnspecified(time, "time");

            var localTime = LocalDateTime.FromDateTime(time);
            var dateTimeZone = DateTimeZoneProviders.Tzdb[timeZone];
            var inZone = localTime.InZone(dateTimeZone, DefaultResolver);
            return inZone.ToDateTimeOffsetSafe();
        }

        /// <summary>
        /// Returns a TimeSpan rounded down to the nearest significance
        /// </summary>
        /// <param name="timeSpan">The TimeSpan that you wish to round down.</param>
        /// <param name="significance">The multiple of significance that you wish to round a TimeSpan to.</param>
        /// <returns>A TimeSpan rounded down to the nearest significance</returns>
        public static TimeSpan Floor(this TimeSpan timeSpan, TimeSpan significance)
        {
            return new TimeSpan(timeSpan.Ticks.Floor(significance.Ticks));
        }

        /// <summary>
        /// Returns a number rounded down to the nearest significance
        /// </summary>
        /// <param name="number">The number that you wish to round down.</param>
        /// <param name="significance">The multiple of significance that you wish to round a number to.</param>
        /// <returns>A number rounded down to the nearest significance</returns>
        public static long Floor(this long number, long significance)
        {
            var divisions = number / significance;
            return significance * divisions;
        }

        /// <summary>
        /// Converts a NodeTime ZonedDateTime into a BCL DateTimeOffset. If the DateTimeOffset would surpass
        /// either DateTimeOffset.MinValue or DateTimeOffset.MaxValue, the Min or Max value will be returned
        /// instead. If the ZonedDateTime's offset includes seconds, these will be removed since the BCL 
        /// DateTimeOffset class does not support this.
        /// </summary>
        /// <param name="zonedDateTime">A ZonedDateTime object</param>
        /// <returns>A DateTimeOffset that corresponds as closely as possible to the ZonedDateTime</returns>
        private static DateTimeOffset ToDateTimeOffsetSafe(this ZonedDateTime zonedDateTime)
        {
            var timeInstant = zonedDateTime.ToInstant();
            var maxValueInstant = Instant.FromDateTimeOffset(DateTimeOffset.MaxValue);
            var minValueInstant = Instant.FromDateTimeOffset(DateTimeOffset.MinValue);

            // Ensure the Instant (UTC Time) is not greater than MaxValue
            if (timeInstant > maxValueInstant)
            {
                return DateTimeOffset.MaxValue;
            }

            // Ensure the Instant (UTC Time) is not less than MinValue
            if (timeInstant < minValueInstant)
            {
                return DateTimeOffset.MinValue;
            }

            // Ensure the Local Time is not greater than MaxValue
            if (zonedDateTime.LocalDateTime.Year > DateTimeOffset.MaxValue.Year)
            {
                return DateTimeOffset.MaxValue;
            }

            // Ensure the Local Time is not less than MinValue
            if (zonedDateTime.LocalDateTime.Year < DateTimeOffset.MinValue.Year)
            {
                return DateTimeOffset.MinValue;
            }

            // To address bug in NodaTime
            // https://github.com/nodatime/nodatime/issues/395
            var offsetTimeSpan = zonedDateTime.Offset.ToTimeSpan();
            var offsetFlooredToNearestMinute = offsetTimeSpan.Floor(TimeSpan.FromMinutes(1));

            return new DateTimeOffset(zonedDateTime.LocalDateTime.ToDateTimeUnspecified(), offsetFlooredToNearestMinute);
        }
    }
}
