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
                AgingData agingData = new AgingData()
                {
                    YMDate = DateTime.Now
                };

                TRANS_DA TransDA = new TRANS_DA();
                ViewBag.LocationsSelectList = new SelectList(TransDA.GetLocationsList(), "Key", "Value");
                ViewBag.DimensionParamSelectList = new SelectList(AgingParam.DimensionParamDictionary, "Key", "Value");
                ViewBag.DataParamSelectList = new SelectList(AgingParam.DataParamDictionary, "Key", "Value");
                return View(agingData);
            }
        }

        public void GetAgingReport(string period, string locparam, string searchfield, string searchvalue)
        {
            ReportParams objReportParams = new ReportParams();
            TRANS_DA TransDA = new TRANS_DA();

            string condition = string.Empty;
            string filter = "-";
            if (searchfield.Length > 0 && searchvalue.Length > 0)
            {
                condition = condition + $"and {searchfield} like '%{searchvalue.ToUpper()}%'";
                filter = $"{searchfield} is {searchvalue.ToUpper()}";
            }

            var data = TransDA.GetAgingReportData(period.Replace(".",""), locparam, LoginData.brandName, condition);
            objReportParams.DataSource = data.Tables[0];
            objReportParams.ReportTitle = "Aging Report";
            objReportParams.RptFileName = "rptAgingReport.rdlc";
            objReportParams.DataSetName = "dsAgingReport";
            objReportParams.prmLocation = locparam;
            string _period = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt16(period.Split('.')[1]))} {period.Split('.')[0]}";
            objReportParams.period = _period;
            objReportParams.filter = filter;
            this.HttpContext.Session["ReportParam"] = objReportParams;
        }


    }
}