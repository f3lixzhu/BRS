using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BRS.Models;
using BRS.ViewModels;
using BRS.Dictionary;
using System.Globalization;

namespace BRS.Controllers
{
    public class RagingController : Controller
    {
        // GET: Raging
        public ActionResult Index()
        {
            if (LoginData.userId is null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                RagingData ragingData = new RagingData()
                {
                    YMDate = DateTime.Now,
                    locationsModel = new LocationsModel()
                };

                TRANS_DA TransDA = new TRANS_DA();
                ViewBag.LocationsSelectList = TransDA.GetLocationsList();
                ViewBag.DimensionParamSelectList = new SelectList(AgingParam.DimensionParamDictionary, "Key", "Value");
                ViewBag.DataParamSelectList = new SelectList(AgingParam.DataParamDictionary, "Key", "Value");
                ViewBag.DimensionFilterSelectList = new SelectList(AgingParam.DimensionFilterDictionary, "Key", "Value");
                return View(ragingData);
            }
        }

        [HttpPost]
        public JsonResult AjaxMethod(string value, string period, string locations)
        {
            RagingData model = new RagingData();
            TRANS_DA TransDA = new TRANS_DA();
            string condition = $" where period='{period.Replace(".","")}'";
            if (locations != "ALL")
                condition = condition + $" and locations = '{locations}'";

            model.filterField = string.Empty;
            model.filterValue = string.Empty;
            model.filterValueList = TransDA.GetPopulateFilterList(value, condition);
            return Json(model);
        }

        public void GetAgingReport(string period, string locparam, string filterfield, string filtervalue)
        {
            ReportParams objReportParams = new ReportParams();
            TRANS_DA TransDA = new TRANS_DA();

            string[] locarray = locparam.Split(',');
            string _locparam = string.Empty;
            bool _all = false;
            foreach (string loc in locarray)
            {
                if (loc == "ALL")
                    _all = true;

                if (_locparam.Length == 0)
                    _locparam = $"'{loc}'";
                else
                    _locparam = _locparam + $", '{loc}'";
            }

            if (_all)
                _locparam = "ALL";

            string condition = string.Empty;
            string filter = "-";
            if (filterfield.Length > 0 && filtervalue.Length > 0)
            {
                condition = condition + $"and {filterfield} = '{filtervalue}'";
                filter = $"{filterfield} is {filtervalue}";
            }

            var data = TransDA.GetAgingReportData(period.Replace(".",""), _locparam, LoginData.brandName, condition);
            objReportParams.DataSource = data.Tables[0];
            objReportParams.ReportTitle = "Aging Report";
            objReportParams.RptFileName = "rptAgingReport.rdlc";
            objReportParams.DataSetName = "dsAgingReport";
            objReportParams.prmLocation = _locparam.Replace("'","");
            string _period = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt16(period.Split('.')[1]))} {period.Split('.')[0]}";
            objReportParams.period = _period;
            objReportParams.filter = filter;
            this.HttpContext.Session["ReportParam"] = objReportParams;
        }


    }
}