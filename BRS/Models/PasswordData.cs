using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BRS.Models
{
    public class PasswordData
    {
        [Required(ErrorMessage = "Please fill the password first!")]
        public string password { get; set; }
        [Required(ErrorMessage = "Please fill the confirm password first!")]
        public string confirmpassword { get; set; }
    }
}