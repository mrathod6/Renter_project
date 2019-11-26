using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Rntr.Models
{
       public class User
   {
       [Key]
       public int UserId {get;set;}
// -----------------------------------------------------------------------------------------------------------
       [Required]
       [MinLength(2, ErrorMessage = "Your First Name must contain at least 2 characters!")]
       [Display(Name = "First Name", Prompt = "First Name")]
       public string FirstName {get;set;}
// -----------------------------------------------------------------------------------------------------------
       [Required]
       [MinLength(2, ErrorMessage = "Your Last Name must contain at least 2 characters!")]
       [Display(Name = "Last Name", Prompt = "Last Name")]
       public string LastName {get;set;}
// -----------------------------------------------------------------------------------------------------------
       [Required(ErrorMessage = "Please enter a vaid email address!")]
       [Display(Name = "Email", Prompt = "Email")]
       [EmailAddress]
       public string Email {get;set;}
// -----------------------------------------------------------------------------------------------------------
       [Required]
       [DataType(DataType.Password)]
       [Display(Name = "Password", Prompt = "Password")]
       [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long", MinimumLength = 8)]
       [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
       public string Password { get; set; }
// -----------------------------------------------------------------------------------------------------------
       [NotMapped]
       [DataType(DataType.Password)]
       [Display(Name = "Confirm password")]
       [Compare("Password", ErrorMessage = "The passwords do not match.")]
       public string ConfirmPassword { get; set; }
// -----------------------------------------------------------------------------------------------------------
       public byte[] UserPhoto {get;set;}
// -----------------------------------------------------------------------------------------------------------
       public DateTime Created_at {get;set;} = DateTime.Now;
// -----------------------------------------------------------------------------------------------------------
       public DateTime Updated_at {get;set;} = DateTime.Now;
// -----------------------------------------------------------------------------------------------------------
       public List<Tool> UsersTools {get;set;}
// -----------------------------------------------------------------------------------------------------------
       public List<Rental> UsersRentals {get;set;}
   }
}
