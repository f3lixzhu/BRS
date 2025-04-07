using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BRS.Models;
using BRS.ViewModels;
using BRS.Dictionary;

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
                TRANS_DA TransDA = new TRANS_DA();
                ViewBag.LocationsSelectList = new SelectList(TransDA.GetLocationsList(), "Key", "Value");
                ViewBag.DimensionParamSelectList = new SelectList(AgingParam.DimensionParamDictionary, "Key", "Value");
                ViewBag.DataParamSelectList = new SelectList(AgingParam.DataParamDictionary, "Key", "Value");
                return View();
            }
        }

        public void GetAgingReport(string locparam)
        {
            ReportParams objReportParams = new ReportParams();
            TRANS_DA TransDA = new TRANS_DA();
            var data = TransDA.GetAgingReportData(LoginData.brandName, locparam);
            objReportParams.DataSource = data.Tables[0];
            objReportParams.ReportTitle = "Aging Report";
            objReportParams.RptFileName = "rptAgingReport.rdlc";
            objReportParams.DataSetName = "dsAgingReport";
            objReportParams.prmLocation = locparam;
            this.HttpContext.Session["ReportParam"] = objReportParams;
        }


    }
}