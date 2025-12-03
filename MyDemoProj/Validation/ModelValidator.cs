using System;
using System.Collections.Generic;
using System.Reflection;

namespace MyDemoProj.Validation
{
    public static class ModelValidator
    {
        public static List<string> Validate(object model)
        {
            var errors = new List<string>();
            var props = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in props)
            {
                var attrs = p.GetCustomAttributes<ValidationAttribute>();
                foreach (var a in attrs)
                {
                    var value = p.GetValue(model);
                    var err = a.Validate(value, p.Name);
                    if (err != null) errors.Add(err);
                }
            }
            return errors;
        }
    }
}