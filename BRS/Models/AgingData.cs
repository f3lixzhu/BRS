using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel.DataAnnotations;

namespace BRS.Models
{
    public class AgingData
    {
        public string searchField { get; set; }
        public string searchValue { get; set; }
        public DataTable dtAgingList { get; set; }
        public Pager pager { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy.MM}", ApplyFormatInEditMode = true)]
        public DateTime YMDate { get; set; }
        public HttpPostedFileBase file { get; set; }
        public int action { get; set; }
    }
}