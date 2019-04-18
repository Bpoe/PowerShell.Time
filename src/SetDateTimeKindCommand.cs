namespace PowerShell.Time
{
    using System;
    using System.Management.Automation;

    [Cmdlet(VerbsCommon.Set, "DateTimeKind")]
    public class SetDateTimeKindCommand : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 1)]
        public DateTime DateTime { get; set; }

        [Parameter(Mandatory = true, Position = 2)]
        public DateTimeKind Kind { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject(DateTime.SpecifyKind(this.DateTime, this.Kind));
        }
    }
}
