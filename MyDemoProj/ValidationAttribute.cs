using System;

namespace MyDemoProj.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public abstract class ValidationAttribute : Attribute
    {
        public abstract string? Validate(object? value, string propertyName);
    }
}