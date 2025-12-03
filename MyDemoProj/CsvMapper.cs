using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MyDemoProj.Mapping
{
    public class CsvMapper<T> where T : new()
    {
        public static List<T> MapFromFile(string path, out List<string> errors)
        {
            errors = new List<string>();
            var results = new List<T>();

            if (!File.Exists(path))
            {
                errors.Add($"Файл не знайдено: {path}");
                return results;
            }

            string[] lines;
            try
            {
                lines = File.ReadAllLines(path);
            }
            catch (Exception ex)
            {
                errors.Add($"Помилка при чтанні файлі: {ex.Message}");
                return results;
            }

            if (lines.Length == 0)
            {
                errors.Add("CSV пустий.");
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
                    errors.Add($"Немая заголовку '{p.Column.Name}' обов'язкового для '{p.Prop.Name}'.");
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
                        continue;

                    string fieldValue = colIdx < fields.Count ? fields[colIdx] : string.Empty;
                    var propType = mp.Prop.PropertyType;
                    try
                    {
                        object? converted = ConvertToType(fieldValue, propType);
                        mp.Prop.SetValue(instance, converted);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Строка {lineIdx + 1}: Не можу конвертувати значення '{fieldValue}' дo {propType.Name} для значення '{mp.Prop.Name}': {ex.Message}");
                        var underlying = Nullable.GetUnderlyingType(propType);
                        if (underlying != null)
                        {
                            mp.Prop.SetValue(instance, null);
                        }
                        else if (propType == typeof(string))
                        {
                            mp.Prop.SetValue(instance, string.Empty);
                        }

                        rowHadError = true;
                    }
                }

               results.Add(instance);
            }

            return results;
        }

        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(line)) return result;
            int i = 0;
            while (i < line.Length)
            {
                if (line[i] == '"')
                {
                    i++;
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
                                i++;
                                break;
                            }
                        }
                        i++;
                    }
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

            if (line.EndsWith(",")) result.Add(string.Empty);
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
                throw new InvalidCastException("Неправильний integer");
            }
            if (underlying == typeof(double))
            {
                if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
                if (string.IsNullOrWhiteSpace(text)) return default(double);
                throw new InvalidCastException("Неправильний double");
            }
            if (underlying == typeof(bool))
            {
                if (bool.TryParse(text, out var v)) return v;
                if (int.TryParse(text, out var iv)) return iv != 0;
                if (string.IsNullOrWhiteSpace(text)) return default(bool);
                throw new InvalidCastException("Неправильний boolean");
            }
            if (underlying.IsEnum)
            {
                try { return Enum.Parse(underlying, text, true); } catch { throw new InvalidCastException("Неправильний enum"); }
            }
            if (underlying == typeof(DateTime))
            {
                if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var v)) return v;
                throw new InvalidCastException("Неправильний DateTime");
            }

            try
            {
                return Convert.ChangeType(text, underlying, CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new InvalidCastException($"Не можу конвертувати дo {underlying.Name}");
            }
        }
    }
}