using System;
using System.Collections.Generic;
using System.IO;
using StudentMetaFramework.Core.Models;
using StudentMetaFramework.Core.Mapping;
using StudentMetaFramework.Core.Validation;

namespace StudentMetaFramework.App
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("StudentMetaFramework demo starting...");

            var fileName = "users.csv";
            if (!File.Exists(fileName))
            {
                CreateSampleCsv(fileName);
                Console.WriteLine($"Sample CSV created: {fileName}");
            }

            var users = CsvMapper<User>.MapFromFile(fileName, out var mapErrors);

            Console.WriteLine($"Imported {users.Count} records. Mapper reported {mapErrors.Count} errors.");
            foreach (var e in mapErrors) Console.WriteLine("Mapper error: " + e);

            int validCount = 0, invalidCount = 0;
            for (int i = 0; i < users.Count; i++)
            {
                var u = users[i];
                var vErrors = ModelValidator.Validate(u);
                if (vErrors.Count == 0)
                {
                    validCount++;
                    Console.WriteLine($"Record {i + 1}: VALID -> {u}");
                }
                else
                {
                    invalidCount++;
                    Console.WriteLine($"Record {i + 1}: INVALID -> {u}");
                    foreach (var ve in vErrors) Console.WriteLine("  Validation: " + ve);
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Summary: Total={users.Count}, Valid={validCount}, Invalid={invalidCount}");
        }

        private static void CreateSampleCsv(string path)
        {
            var lines = new List<string>
            {
                "Username,Email,Age",
                "alice,alice@example.com,30",
                "bob,bob_at_example.com,25",
                "c,short@example.com,17",
                "dave,dave@example.com,notanumber",
                "\"eva,smith\",eva.smith@example.com,22",
                "frank, ,40",
            };
            File.WriteAllLines(path, lines);
        }
    }
}