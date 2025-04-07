using System;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BRS.Models
{
    public class UserLibrary
    {
        public class UserData
        {
            [Required(ErrorMessage = "Please enter username first!")]
            public string UserId { get; set; }
            [Required(ErrorMessage = "Please enter password first!")]
            public string Password { get; set; }
            [Required(ErrorMessage = "Please enter company first!")]
            public string visibleErrorLogin { get; set; } = "hidden";
        }

        public class UserManagement
        {
            public string userId { get; set; }
            public string userName { get; set; }
            public string password { get; set; }
            public int brandId { get; set; }
            public string brandName { get; set; }
            public bool active { get; set; }
            public DateTime lastLogin { get; set; }
            public string searchFieldUser { get; set; }
            public string searchValueUser { get; set; }
            public DataTable dtUserList { get; set; }
            public Pager pager { get; set; }
        }

        public class UserModule
        {
            public int ModuleId { get; set; }
            public string ModuleName { get; set; }
            public string Url { get; set; }
        }

        public class UserModuleCategory
        {
            public UserModuleCategory()
            {
                UserModules = new List<UserModule>();
            }

            public string ModuleCategory { get; set; }
            public List<UserModule> UserModules { get; set; }
            public int MenuCategoryId { get; set; }
            public string MenuCategoryCss { get; set; }
        }
    }
}