using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace BRS.Models
{
    public class ReportParams
    {
        public string RptFileName { get; set; }
        public string ReportTitle { get; set; }
        public DataTable DataSource { get; set; }
        public bool IsHasParams { get; set; }
        public string DataSetName { get; internal set; }
        public string prmLocation { get; set; }
        public string period { get; set; }
        public string filter { get; set; }
    }
}