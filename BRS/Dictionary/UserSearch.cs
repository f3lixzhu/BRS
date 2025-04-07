using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BRS.Dictionary
{
    public class UserSearch
    {
        public static Dictionary<string, string> UserSearchDictionary = new Dictionary<string, string>
        {
            { "US.UserId", "USER ID" },
            { "US.UserName", "USERNAME" },
            { "BR.BrandName", "BRAND NAME" }
        };
    }
}