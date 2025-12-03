using System;

namespace MyDemoProj.Mapping
{
    
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CsvIgnoreAttribute : Attribute { }
}