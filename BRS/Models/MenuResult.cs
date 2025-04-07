using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BRS.Models
{
    public class MenuResult
    {
        public string brandName { get; set; } = "";
        public List<UserLibrary.UserModuleCategory> moduleCategories { get; set; }
        public string errorMessage { get; set; } = "";
    }
}