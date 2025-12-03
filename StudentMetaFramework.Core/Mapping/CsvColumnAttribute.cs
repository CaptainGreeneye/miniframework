using System;

namespace StudentMetaFramework.Core.Mapping
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CsvColumnAttribute : Attribute
    {
        public string Name { get; }
        public CsvColumnAttribute(string name) => Name = name;
    }
}