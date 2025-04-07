using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BRS.Models
{
    public class ItemResult
    {
        public ItemResult()
        {
            Barcodes = new List<string>();
        }
        public List<string> Barcodes { get; set; }
    }
}