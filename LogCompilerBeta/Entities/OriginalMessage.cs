using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogCompilerBeta.Entities
{
    namespace YourProjectName.Models
    {
        public class OriginalMessage
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            [Required]
            [Column(TypeName = "nvarchar(MAX)")]
            public string Message { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        }
    }
}
