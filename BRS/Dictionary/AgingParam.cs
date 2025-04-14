using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BRS.Dictionary
{
    public class AgingParam
    {
        public static Dictionary<string, string> DimensionParamDictionary = new Dictionary<string, string>
        {
            { "Description", "Items" },
            { "Category", "Category" },
            { "Fit", "Fit" },
            { "SeasonYear", "Season Year" },
            { "Gender", "Gender" },
            { "ItemType", "Item Type" },
            { "Color", "Color" },
            { "Size", "Size" },
            { "BoardWH", "Board WH" }
        };

        public static Dictionary<string, string> DataParamDictionary = new Dictionary<string, string>
        {
            { "Quantity", "Quantity" },
            { "TagPrice", "Tag Price" },
            { "RetailPrice", "Retail Price" },
            { "CostPrice", "Cost Price" }
        };

        public static Dictionary<string, string> DimensionFilterDictionary = new Dictionary<string, string>
        {
            { "Category", "Category" },
            { "Fit", "Fit" },
            { "SeasonYear", "Season Year" },
            { "Gender", "Gender" },
            { "ItemType", "Item Type" },
            { "Color", "Color" },
            { "Size", "Size" }
        };
    }
}