using System;

namespace StudentMetaFramework.Core.Mapping
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CsvIgnoreAttribute : Attribute { }
}