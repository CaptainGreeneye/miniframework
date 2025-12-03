using System;
using System.Globalization;

namespace StudentMetaFramework.Core.Validation
{
    public sealed class RangeAttribute : ValidationAttribute
    {
        public double Min { get; }
        public double Max { get; }
        public RangeAttribute(double min, double max) { Min = min; Max = max; }

        public override string? Validate(object? value, string propertyName)
        {
            if (value == null) return null;
            try
            {
                var d = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                if (d < Min || d > Max) return $"{propertyName} must be in range [{Min}..{Max}].";
            }
            catch
            {
                return $"{propertyName} has incompatible type for Range validation.";
            }
            return null;
        }
    }
}