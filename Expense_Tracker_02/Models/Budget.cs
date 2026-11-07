using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker_02.Models
{
    public class Budget
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; } = Category.All[0];

        [Required]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
        public int Month { get; set; } = DateTime.Today.Month;

        [Required]
        [Range(2000, 2100)]
        public int Year { get; set; } = DateTime.Today.Year;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Limit must be greater than 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LimitAmount { get; set; }
    }
}
