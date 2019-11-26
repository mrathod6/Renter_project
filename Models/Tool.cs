using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Rntr.Models
{
    public class Tool
    {
        [Key]
        public int ToolId { get; set; }
// -----------------------------------------------------------------------------------------------------------
        [Required]
        [MinLength(3, ErrorMessage = "Tool name needs to be at least 3 characters!")]
        [Display(Name = "Tool Name")]
        public string ToolName { get; set; }
// -----------------------------------------------------------------------------------------------------------
        [Required]
        [MinLength(5, ErrorMessage = "Tool description must be more than 5 characters")]
        [MaxLength(50, ErrorMessage = "Tool description must be less than 50 characters")]
        public string Description {get;set;}

// -----------------------------------------------------------------------------------------------------------
        [Required]
        public byte[] ToolImage1 {get;set;}
// -----------------------------------------------------------------------------------------------------------
        public DateTime Created_at { get; set; } = DateTime.Now;
// -----------------------------------------------------------------------------------------------------------
        public DateTime Updated_at { get; set; } = DateTime.Now;
// -----------------------------------------------------------------------------------------------------------
        public bool ToolAvailability { get; set; } = true;
// -----------------------------------------------------------------------------------------------------------
        public User Owner {get;set;}
// -----------------------------------------------------------------------------------------------------------
        public List<Rental> ToolsRentals {get;set;}
// -----------------------------------------------------------------------------------------------------------
    }
}
