namespace PowerShell.Time
{
    using System;
    using System.Management.Automation;
    using NodaTime;

    [Cmdlet(VerbsData.ConvertTo, "TimeZone")]
    public class ConvertToTimeZoneCommand : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 1)]
        public DateTime DateTime { get; set; }

        [Parameter(Mandatory = true, Position = 2)]
        public string OutputTimeZone { get; set; }

        [Parameter(Mandatory = false, Position = 3)]
        public string SourceTimeZone { get; set; }

        protected override void ProcessRecord()
        {
            EnsureTimeZoneIdIsValid(this.OutputTimeZone);

            if (!string.IsNullOrWhiteSpace(this.SourceTimeZone))
            {
                EnsureTimeZoneIdIsValid(this.SourceTimeZone);

                var sourceTime = this.DateTime.ToDateTimeOffsetInTimeZone(this.SourceTimeZone);
                WriteObject(sourceTime.InTimeZone(this.OutputTimeZone));
                return;
            }

            var dateTimeOffset = new DateTimeOffset(this.DateTime);
            WriteObject(dateTimeOffset.InTimeZone(this.OutputTimeZone));
        }

        private static void EnsureTimeZoneIdIsValid([ValidatedNotNull] string timeZoneId)
        {
            var dateTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
            Ensure.IsTrue(dateTimeZone != null, Resources.TimeZoneIdIsInvalid);
        }
    }
}
