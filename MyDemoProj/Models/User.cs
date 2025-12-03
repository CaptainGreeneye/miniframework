using System;
using MyDemoProj.Mapping;
using MyDemoProj.Validation;

namespace MyDemoProj.Models
{
    public class User
    {

        [CsvColumn("Ім'я Користувача")]
        [Required]
        [StringLength(3, 50)]
        public string Username { get; set; } = string.Empty;


        [CsvColumn("Єл.Пошта")]
        [Required]
        [StringLength(5, 100)]
        public string Email { get; set; } = string.Empty;

        // make Age nullable and require it so conversion failures are reported by the validator
        [CsvColumn("Вік")]
        [Required]
        [Range(0, 150)]
        public int? Age { get; set; }

        [CsvIgnore]
        public bool IsAdult => Age.HasValue && Age.Value >= 18;

        public override string ToString() => $"{Username} ({Email}), Вік={Age}, Чи дорослий={IsAdult}";
    }
}