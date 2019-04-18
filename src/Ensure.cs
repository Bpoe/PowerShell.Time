namespace PowerShell.Time
{
    using System;
    using System.Diagnostics;

    public static class Ensure
    {
        [DebuggerStepThrough]
        public static void IsNotNull([ValidatedNotNull] object value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        [DebuggerStepThrough]
        public static void DateTimeKindIsUtc(DateTime dateTime, string paramName)
        {
            IsTrue(
                dateTime.Kind == DateTimeKind.Utc,
                Resources.TimeMustBeUtc,
                paramName);
        }

        [DebuggerStepThrough]
        public static void DateTimeKindIsUnspecified(DateTime dateTime, string paramName)
        {
            IsTrue(
                dateTime.Kind == DateTimeKind.Unspecified,
                Resources.TimeMustBeUnspecified,
                paramName);
        }

        [DebuggerStepThrough]
        public static void IsTrue(bool condition)
        {
            IsTrue(condition, null);
        }

        [DebuggerStepThrough]
        public static void IsTrue(bool condition, string message)
        {
            IsTrue(condition, message, null);
        }

        [DebuggerStepThrough]
        public static void IsTrue(bool condition, string message, string paramName)
        {
            if (!condition)
            {
                throw new ArgumentException(message, paramName);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class ValidatedNotNullAttribute : Attribute
    {
    }
}
