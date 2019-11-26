using System.ComponentModel.DataAnnotations;


namespace Rntr.Models
{
    public class LogUser
    {
        [Required(ErrorMessage="Email is required to login")]
        [EmailAddress]
        [Display(Name = "E-mail", Prompt = "E-mail")]
        public string LogEmail{get;set;}
// -----------------------------------------------------------------------------------------------------------

        [Required(ErrorMessage="Password is required to login")]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Prompt = "Password")]
        public string LogPassword{get;set;}
    }
}