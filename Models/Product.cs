using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ProductWorkflow.API.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [StringLength(500)]
        public string Description { get; set; } = "";

        [Range(0, 9999, ErrorMessage = "Price must be non-negative")]
        [Precision(18, 2)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Product Category is required")]
        [StringLength(20)]
        public string Category { get; set; } = "";
    }
}
