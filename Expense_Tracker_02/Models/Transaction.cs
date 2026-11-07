using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker_02.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; } = Category.All[0];

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [StringLength(200)]
        public string? Note { get; set; }
    }
}
