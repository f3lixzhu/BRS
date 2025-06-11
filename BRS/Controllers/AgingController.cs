using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Configuration;
using BRS.Models;
using BRS.ViewModels;
using BRS.Dictionary;
using System.Reflection;
using System.IO;
using ClosedXML.Excel;

namespace BRS.Controllers
{
    public class AgingController : Controller
    {
        public class ButtonNameActionAttribute : ActionNameSelectorAttribute
        {
            public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
            {
                if (actionName.Equals(methodInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                    return true;

                var request = controllerContext.RequestContext.HttpContext.Request;
                return request[methodInfo.Name] != null;
            }
        }

        // GET: Aging
        public ActionResult Index(string searchField = "", string searchValue = "", string sortBy = "", int page = 1, string actions = "")
        {
            ViewBag.SortPeriodParameter = sortBy == "PERIOD" ? "PERIOD ASC" : "PERIOD";
            ViewBag.SortReleaseDateParameter = sortBy == "RELEASEDATE" ? "RELEASEDATE DESC" : "RELEASEDATE";
            ViewBag.SortLocationParameter = sortBy == "LOCATION" ? "LOCATION DESC" : "LOCATION";
            ViewBag.SortBarcodeParameter = sortBy == "BARCODE" ? "BARCODE DESC" : "BARCODE";
            ViewBag.SortQuantityParameter = sortBy == "QUANTITY" ? "QUANTITY DESC" : "QUANTITY";

            if (LoginData.userId is null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                TRANS_DA TransDA = new TRANS_DA();
                AgingData agingData;

                if (TempData.Peek("_aging") is null)
                {
                    string where = "";
                    sortBy = "Period DESC, Barcode ASC";
                    DataSet ds = TransDA.GetAging(page, where, sortBy);
                    agingData = new AgingData()
                    {
                        dtAgingList = ds.Tables[0],
                        pager = new Pager((ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0) ? Convert.ToInt32(ds.Tables[1].Rows[0]["TotalRecords"]) : 0, 1, Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"])),
                        YMDate = DateTime.Now,
                        action = 1
                    };

                    TempData["_aging"] = agingData;
                }
                else
                {
                    agingData = (AgingData)TempData.Peek("_aging");

                    if (page > 1)
                        sortBy = (string)TempData.Peek("_sort");
                    else
                        TempData["_sort"] = sortBy;

                    if (searchField.Length == 0 && actions == "ItemSearch")
                    {
                        TempData["err"] = "Silakan pilih filter terlebih dahulu!";
                    }
                    
                    agingData = bindGrid(page, agingData, searchField, searchValue, sortBy);
                    if (actions != "UploadFile" && agingData.dtAgingList.Rows.Count == 0)
                    {
                        if (actions == "ItemSearch")
                            TempData["err"] = "Data tidak ditemukan!";
                    }
                }

                ViewBag.searchField = searchField;
                ViewBag.searchValue = searchValue;
                ViewBag.ItemSearchSelectList = new SelectList(AgingSearch.AgingSearchDictionary, "Key", "Value");
                if (agingData.file != null)
                    ViewBag.FileName = agingData.YMDate.ToString("yyyy.MM") + " - " + agingData.file.FileName;
                return View(agingData);
            }
        }

        private AgingData bindGrid(int page, AgingData agingData, string searchField, string searchValue, string sortBy)
        {
            string where = "";

            if (searchField.Length > 0 && searchValue.Length > 0)
            {
                where = where + string.Format("{0} like '%{1}%' and ", searchField, searchValue);
            }

            if (where != "")
            {
                where = " where " + where.Remove(where.Length - 5);
            }

            agingData.searchField = searchField;
            agingData.searchValue = searchValue;
            TRANS_DA TransDA = new TRANS_DA();
            DataSet ds = new DataSet();

            string sort = string.Empty;
            switch (sortBy)
            {
                case "PERIOD ASC":
                    sort = "Period ASC, Barcode ASC";
                    break;
                case "RELEASEDATE DESC":
                    sort = "ReleaseDate DESC, Barcode ASC";
                    break;
                case "RELEASEDATE":
                    sort = "ReleaseDate ASC, Barcode ASC";
                    break;
                case "LOCATION DESC":
                    sort = "Locations DESC, Barcode ASC";
                    break;
                case "LOCATION":
                    sort = "Locations ASC, Barcode ASC";
                    break;
                case "BARCODE DESC":
                    sort = "Barcode DESC";
                    break;
                case "BARCODE":
                    sort = "Barcode ASC";
                    break;
                case "QUANTITY DESC":
                    sort = "Quantity DESC";
                    break;
                case "QUANTITY":
                    sort = "Quantity ASC";
                    break;
                default:
                    sort = "Period DESC, Barcode ASC";
                    break;
            }

            ds = TransDA.GetAging(page, where, sort);

            agingData.dtAgingList = ds.Tables[0];
            var pager = new Pager((ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0) ? Convert.ToInt32(ds.Tables[1].Rows[0]["TotalRecords"]) : 0, page, Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]));
            agingData.pager = pager;
            return agingData;
        }

        [HttpPost]
        [ButtonNameAction]
        public ActionResult AgingsSearch(AgingData aging, FormCollection fc)
        {
            TempData["_aging"] = aging;
            return RedirectToAction("Index", "Aging", new { searchField = fc["searchField"], searchValue = fc["searchValue"], actions = "ItemSearch" });
        }

        [HttpPost]
        [ButtonNameAction]
        public ActionResult ClearFilter(AgingData aging)
        {
            return RedirectToAction("Index", "Aging", new { actions = "ClearFilter" });
        }

        [HttpPost]
        [ButtonNameAction]
        public ActionResult UploadFile(AgingData aging)
        {
            try
            {
                if (aging.file == null)
                    throw new Exception("Please select file to upload first!");

                if (aging.file.ContentLength > 0)
                {
                    string period = aging.YMDate.ToString("yyyyMM");
                    string _fileName = Path.GetFileName(aging.file.FileName);
                    string _path = Path.Combine(Server.MapPath("~/UploadedFiles"), _fileName);
                    aging.file.SaveAs(_path);
                    string _fileExt = Path.GetExtension(_path);
                    TRANS_DA TransDA = new TRANS_DA();
                    int uploadRows = 0;
                    DataTable itemResult = new DataTable();
                    string errMessage = TransDA.uploadAging(aging.action, period, _fileExt, _path, LoginData.userId, true, out uploadRows, out itemResult);
                    if (errMessage.Length > 0)
                    {
                        TempData["_aging"] = aging;
                        TempData["ItemResult"] = itemResult;
                        throw new Exception(errMessage);
                    }

                    TempData["_aging"] = aging;
                    TempData["suc"] = $"Aging Uploaded Successfully, {uploadRows} processed";
                }

                return RedirectToAction("Index", "Aging", new { actions = "UploadFile" });
            }
            catch (Exception ex)
            {
                if (ex.Message != "CONFIRM")
                    TempData["ErrMessage"] = $"File upload failed!! {ex.Message}";
                else
                    TempData["ErrMessage"] = ex.Message;

                TempData["show"] = 1;

                if (ex.Message.Substring(0, 5) == "Found")
                    TempData["showButton"] = 1;

                return RedirectToAction("Index", "Aging", new { actions = "UploadFile" });
            }
        }

        [HttpPost]
        [ButtonNameAction]
        public ActionResult DeleteAging(FormCollection fc)
        {
            try
            {
                string period = fc["Period"];
                string location = fc["Location"].Trim();
                string barcode = fc["Barcode"].Trim();

                TRANS_DA transDA = new TRANS_DA();
                string errMessage = transDA.deleteAging(period, location, barcode, LoginData.userId);
                if (errMessage.Length > 0)
                    throw new Exception(errMessage);

                TempData["Suc"] = "Aging deleted successfully";
            }
            catch (Exception ex)
            {
                int maxLength = 120;
                if (ex.Message.Length > maxLength)
                    TempData["err"] = ex.Message.Substring(0, maxLength);
                else
                    TempData["err"] = ex.Message;
            }

            return RedirectToAction("Index", "Aging", new { actions = "Delete" });
        }

        [HttpPost]
        [ButtonNameAction]
        public ActionResult ExportItems()
        {
            if (TempData.Peek("ItemResult") != null)
            {
                DataTable dt = (DataTable)TempData["ItemResult"];
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = "ItemsResult_" + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx";

                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dt);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), contentType, fileName);
                    }
                }
            }

            return RedirectToAction("Index", "Aging", new { actions = "Export" });
        }

        public ActionResult Download()
        {
            string _path = @"../UploadedFiles/FileUploadFormat/Template Aging.xlsx";
            if (System.IO.File.Exists(Server.MapPath(_path)))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(Server.MapPath(_path));
                return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, "TemplateAging.xlsx");
            }

            return RedirectToAction("Index", "Aging", new { actions = "Download" });
        }
    }
}