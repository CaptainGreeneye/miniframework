using System;

namespace MyDemoProj.Mapping
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CsvColumnAttribute : Attribute
    {
        public string Name { get; }
        public CsvColumnAttribute(string name) => Name = name;
    }
}