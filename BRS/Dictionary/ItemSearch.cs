using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BRS.Dictionary
{
    public class ItemSearch
    {
        public static Dictionary<string, string> ItemSearchDictionary = new Dictionary<string, string>
        {
            { "Category", "CATEGORY" },
            { "Description", "DESCRIPTION" },
            { "Barcode", "BARCODE" }
        };
    }
}