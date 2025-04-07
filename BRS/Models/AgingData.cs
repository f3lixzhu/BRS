using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace BRS.Models
{
    public class AgingData
    {
        public string searchField { get; set; }
        public string searchValue { get; set; }
        public DataTable dtAgingList { get; set; }
        public Pager pager { get; set; }
    }
}