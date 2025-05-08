using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace BRS.Models
{
    public class ItemData
    {
        public string searchField { get; set; }
        public string searchValue { get; set; }
        public DataTable dtItemList { get; set; }
        public Pager pager { get; set; }
        public int action { get; set; }
    }
}