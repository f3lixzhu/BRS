using System.Collections.Generic;

namespace BRS.Models
{
    public class LoginData
    {
        public static string userId { get; set; }
        public static string brandName { get; set; }
        public static List<UserLibrary.UserModuleCategory> umc { get; set; }
    }
}