using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Mvc;

namespace BRS.Models
{
    public class RagingData
    {
        public RagingData()
        {
            filterValueList = new List<SelectListItem>();
        }

        public DateTime YMDate { get; set; }
        public LocationsModel locationsModel { get; set; }
        public string locationsFilter { get; set; }
        public string filterField { get; set; }
        public List<SelectListItem> filterValueList { get; set; }
        public string filterValue { get; set; }
    }

    public class LocationsModel
    {
        public LocationsModel()
        {
            locationsList = new List<SelectListItem>();
        }

        public List<SelectListItem> locationsList { get; set; }
        public string[] locations { get; set; }
    }
}