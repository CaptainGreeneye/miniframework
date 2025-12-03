using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace StudentMetaFramework.Core.Mapping
{
    public static class CsvMapper<T> where T : new()
    {
        // Map CSV file into list of T; returns mapping errors via out parameter
        public static List<T> MapFromFile(string path, out List<string> errors)
        {
            errors = new List<string>();
            var results = new List<T>();

            if (!File.Exists(path))
            {
                errors.Add($"File not found: {path}");
                return results;
            }

            string[] lines;
            try
            {
                lines = File.ReadAllLines(path);
            }
            catch (Exception ex)
            {
                errors.Add($"Error reading file: {ex.Message}");
                return results;
            }

            if (lines.Length == 0)
            {
                errors.Add("CSV is empty.");
                return results;
            }

            var header = ParseCsvLine(lines[0]);
            var headerIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < header.Count; i++)
                headerIndex[header[i]] = i;

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.CanWrite)
                        .Select(p => new
                        {
                            Prop = p,
                            Column = p.GetCustomAttribute<CsvColumnAttribute>(),
                            Ignore = p.GetCustomAttribute<CsvIgnoreAttribute>() != null
                        })
                        .Where(x => !x.Ignore && x.Column != null)
                        .ToList();

            foreach (var p in props)
            {
                if (!headerIndex.ContainsKey(p.Column!.Name))
                    errors.Add($"Header missing column '{p.Column.Name}' required for property '{p.Prop.Name}'.");
            }

            for (int lineIdx = 1; lineIdx < lines.Length; lineIdx++)
            {
                var rawLine = lines[lineIdx];
                if (string.IsNullOrWhiteSpace(rawLine))
                    continue;

                var fields = ParseCsvLine(rawLine);
                var instance = new T();
                bool rowHadError = false;

                foreach (var mp in props)
                {
                    if (!headerIndex.TryGetValue(mp.Column!.Name, out int colIdx))
                        continue; // missing column already reported

                    string fieldValue = colIdx < fields.Count ? fields[colIdx] : string.Empty;
                    var propType = mp.Prop.PropertyType;
                    try
                    {
                        object? converted = ConvertToType(fieldValue, propType);
                        mp.Prop.SetValue(instance, converted);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Line {lineIdx + 1}: Cannot convert value '{fieldValue}' to {propType.Name} for property '{mp.Prop.Name}': {ex.Message}");
                        rowHadError = true;
                    }
                }

                results.Add(instance);
                // continue importing even if rowHadError, errors recorded
            }

            return results;
        }

        // Simple CSV parser with handling for quoted values and escaped quotes
        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(line)) return result;
            int i = 0;
            while (i < line.Length)
            {
                if (line[i] == '"')
                {
                    i++; // skip opening quote
                    var start = i;
                    var value = "";
                    while (i < line.Length)
                    {
                        if (line[i] == '"')
                        {
                            if (i + 1 < line.Length && line[i + 1] == '"')
                            {
                                value += line.Substring(start, i - start) + "\"";
                                i += 2;
                                start = i;
                                continue;
                            }
                            else
                            {
                                value += line.Substring(start, i - start);
                                i++; // skip closing quote
                                break;
                            }
                        }
                        i++;
                    }
                    // skip until comma or end
                    while (i < line.Length && line[i] != ',') i++;
                    if (i < line.Length && line[i] == ',') i++;
                    result.Add(value);
                }
                else
                {
                    var start = i;
                    while (i < line.Length && line[i] != ',') i++;
                    result.Add(line.Substring(start, i - start).Trim());
                    if (i < line.Length && line[i] == ',') i++;
                }
            }

            if (line.EndsWith(","))
                result.Add(string.Empty);

            return result;
        }

        private static object? ConvertToType(string text, Type targetType)
        {
            if (targetType == typeof(string)) return text;

            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlying == typeof(int))
            {
                if (int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
                if (string.IsNullOrWhiteSpace(text)) return default(int);
                throw new InvalidCastException("Invalid integer");
            }
            if (underlying == typeof(double))
            {
                if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
                if (string.IsNullOrWhiteSpace(text)) return default(double);
                throw new InvalidCastException("Invalid double");
            }
            if (underlying == typeof(bool))
            {
                if (bool.TryParse(text, out var v)) return v;
                if (int.TryParse(text, out var iv)) return iv != 0;
                if (string.IsNullOrWhiteSpace(text)) return default(bool);
                throw new InvalidCastException("Invalid boolean");
            }
            if (underlying.IsEnum)
            {
                try { return Enum.Parse(underlying, text, true); } catch { throw new InvalidCastException("Invalid enum"); }
            }
            if (underlying == typeof(DateTime))
            {
                if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var v)) return v;
                throw new InvalidCastException("Invalid DateTime");
            }

            try
            {
                return Convert.ChangeType(text, underlying, CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new InvalidCastException($"Cannot convert to {underlying.Name}");
            }
        }
    }
}