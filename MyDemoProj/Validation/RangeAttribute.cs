using System;
using System.Globalization;

namespace MyDemoProj.Validation
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
                if (d < Min || d > Max) return $"{propertyName} повинен бути в межах [{Min}..{Max}].";
            }
            catch
            {
                return $"{propertyName} має несумісний тип для перевірки діапазону.";
            }
            return null;
        }
    }
}