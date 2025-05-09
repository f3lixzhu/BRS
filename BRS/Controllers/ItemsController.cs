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
    public class ItemsController : Controller
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

        // GET: Items
        public ActionResult Index(string searchField = "", string searchValue = "", string sortBy = "", int page = 1, int _action = 1, string actions = "")
        {
            ViewBag.SortBarcodeParameter = sortBy == "BARCODE" ? "BARCODE DESC" : "BARCODE";
            ViewBag.SortGenderParameter = sortBy == "GENDER" ? "GENDER DESC" : "GENDER";
            ViewBag.SortItemTypeParameter = sortBy == "ITEMTYPE" ? "ITEMTYPE DESC" : "ITEMTYPE";
            ViewBag.SortCategoryParameter = sortBy == "CATEGORY" ? "CATEGORY DESC" : "CATEGORY";
            ViewBag.SortDescriptionParameter = sortBy == "DESCRIPTION" ? "DESCRIPTION DESC" : "DESCRIPTION";
            ViewBag.SortColorParameter = sortBy == "COLOR" ? "COLOR DESC" : "COLOR";
            ViewBag.SortSizeParameter = sortBy == "SIZE" ? "SIZE DESC" : "SIZE";
            ViewBag.SortFitParameter = sortBy == "FIT" ? "FIT DESC" : "FIT";
            ViewBag.SortSeasonYearParameter = sortBy == "SEASONYEAR" ? "SEASONYEAR DESC" : "SEASONYEAR";
            ViewBag.SortTagPriceParameter = sortBy == "TAGPRICE" ? "TAGPRICE DESC" : "TAGPRICE";
            ViewBag.SortRetailPriceParameter = sortBy == "RETAILPRICE" ? "RETAILPRICE DESC" : "RETAILPRICE";
            ViewBag.SortCOGSParameter = sortBy == "COGS" ? "COGS DESC" : "COGS";

            if (LoginData.userId is null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                TRANS_DA TransDA = new TRANS_DA();
                ItemData itemData;

                if (TempData.Peek("_item") is null)
                {
                    string where = $"where BRAND = '{LoginData.brandName}'";
                    sortBy = "Barcode ASC";
                    DataSet ds = TransDA.GetMsItem(page, where, sortBy);
                    itemData = new ItemData()
                    {
                        dtItemList = ds.Tables[0],
                        pager = new Pager((ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0) ? Convert.ToInt32(ds.Tables[1].Rows[0]["TotalRecords"]) : 0, 1, Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"])),
                        action = 1
                    };

                    TempData["_item"] = itemData;
                }
                else
                {
                    itemData = (ItemData)TempData.Peek("_item");

                    if (page > 1)
                        sortBy = (string)TempData.Peek("_sort");
                    else
                        TempData["_sort"] = sortBy;

                    if (searchField.Length == 0 && actions == "ItemSearch")
                    {
                        TempData["err"] = "Silakan pilih filter terlebih dahulu!";
                    }

                    itemData = bindGrid(page, itemData, searchField, searchValue, sortBy, _action);
                    if (actions != "UploadFile" && itemData.dtItemList.Rows.Count == 0)
                        TempData["err"] = "Data tidak ditemukan!";
                }

                ViewBag.searchField = searchField;
                ViewBag.searchValue = searchValue;
                ViewBag.ItemSearchSelectList = new SelectList(ItemSearch.ItemSearchDictionary, "Key", "Value");
                return View(itemData);
            }
        }

        private ItemData bindGrid(int page, ItemData itemData, string searchField, string searchValue, string sortBy, int action)
        {
            string where = $"BRAND = '{LoginData.brandName}' and ";

            if (searchField.Length > 0 && searchValue.Length > 0)
            {
                where = where + string.Format("{0} like '%{1}%' and ", searchField, searchValue);
            }

            if (where != "")
            {
                where = " where " + where.Remove(where.Length - 5);
            }

            itemData.searchField = searchField;
            itemData.searchValue = searchValue;
            
            TRANS_DA TransDA = new TRANS_DA();
            DataSet ds = new DataSet();

            string sort = string.Empty;
            switch (sortBy)
            {
                case "BARCODE DESC":
                    sort = "Barcode DESC";
                    break;
                case "GENDER DESC":
                    sort = "Gender DESC, Barcode ASC";
                    break;
                case "GENDER":
                    sort = "Gender ASC, Barcode ASC";
                    break;
                case "ITEMTYPE DESC":
                    sort = "ItemType DESC, Barcode ASC";
                    break;
                case "ITEMTYPE":
                    sort = "ItemType ASC, Barcode ASC";
                    break;
                case "CATEGORY DESC":
                    sort = "Category DESC, Barcode ASC";
                    break;
                case "CATEGORY":
                    sort = "Category ASC, Barcode ASC";
                    break;
                case "DESCRIPTION DESC":
                    sort = "Description DESC";
                    break;
                case "DESCRIPTION":
                    sort = "Description ASC";
                    break;
                case "COLOR DESC":
                    sort = "Color DESC, Barcode ASC";
                    break;
                case "COLOR":
                    sort = "Color ASC, Barcode ASC";
                    break;
                case "SIZE DESC":
                    sort = "Size DESC, Barcode ASC";
                    break;
                case "SIZE":
                    sort = "Size ASC, Barcode ASC";
                    break;
                case "FIT DESC":
                    sort = "Fit DESC, Barcode ASC";
                    break;
                case "FIT":
                    sort = "Fit ASC, Barcode ASC";
                    break;
                case "SEASONYEAR DESC":
                    sort = "SeasonYear DESC, Barcode ASC";
                    break;
                case "SEASONYEAR":
                    sort = "SeasonYear ASC, Barcode ASC";
                    break;
                case "TAGPRICE DESC":
                    sort = "TagPrice DESC";
                    break;
                case "TAGPRICE":
                    sort = "TagPrice ASC";
                    break;
                case "RETAILPRICE DESC":
                    sort = "RetailPrice DESC";
                    break;
                case "RETAILPRICE":
                    sort = "RetailPrice ASC";
                    break;
                case "COGS DESC":
                    sort = "COGS DESC";
                    break;
                case "COGS":
                    sort = "COGS ASC";
                    break;
                default:
                    sort = "Barcode ASC";
                    break;
            }

            ds = TransDA.GetMsItem(page, where, sort);

            itemData.dtItemList = ds.Tables[0];
            var pager = new Pager((ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0) ? Convert.ToInt32(ds.Tables[1].Rows[0]["TotalRecords"]) : 0, page, Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]));
            itemData.pager = pager;
            itemData.action = action;

            return itemData;
        }

        [HttpPost]
        [ButtonNameAction]
        public ActionResult ItemsSearch(ItemData items, FormCollection fc)
        { 
            TempData["_items"] = items;
            return RedirectToAction("Index", "Items", new { searchField = fc["searchField"], searchValue = fc["searchValue"], actions = "ItemSearch" });
        }

        [HttpPost]
        [ButtonNameAction]
        public ActionResult ClearFilter(ItemData items)
        {
            TempData["action"] = "ClearFilter";
            return RedirectToAction("Index", "Items", new { actions = "ClearFilter" });
        }

        [HttpPost]
        [ButtonNameAction]
        public ActionResult UploadFile(ItemData id, HttpPostedFileBase file)
        {
            try
            {
                if (file == null)
                    throw new Exception("Please select file to upload first!");

                if (file.ContentLength > 0)
                {
                    string _fileName = Path.GetFileName(file.FileName);
                    string _path = Path.Combine(Server.MapPath("~/UploadedFiles"), _fileName);
                    file.SaveAs(_path);
                    string _fileExt = Path.GetExtension(_path);
                    TRANS_DA TransDA = new TRANS_DA();
                    int uploadRows = 0;
                    DataTable itemResult = new DataTable();
                    string errMessage = TransDA.uploadMsItem(id.action, _fileExt, _path, LoginData.userId, out uploadRows, out itemResult);
                    if (errMessage.Length > 0)
                    {
                        TempData["ItemResult"] = itemResult;
                        throw new Exception(errMessage);
                    }

                    DataSet ds = TransDA.GetMsItem(1, "", "Barcode ASC");

                    TempData["suc"] = $"Items Uploaded Successfully, {uploadRows} processed";
                }

                return RedirectToAction("Index", "Items", new { _action = id.action, actions = "UploadFile" });
            }
            catch (Exception ex)
            {
                TempData["ErrMessage"] = $"File upload failed!! {ex.Message}";
                TempData["show"] = 1;

                if (ex.Message.Substring(0, 5) == "Found")
                    TempData["showButton"] = 1;

                return RedirectToAction("Index", "Items", new { _action = id.action, actions = "UploadFile" });
            }
        }

        [HttpPost]
        [ButtonNameAction]
        public ActionResult DeleteItem(FormCollection fc)
        {
            try
            {
                string barcode = fc["Barcode"];
                TRANS_DA transDA = new TRANS_DA();
                string errMessage = transDA.deleteItem(barcode, LoginData.userId);
                if (errMessage.Length > 0)
                    throw new Exception(errMessage);
                
                TempData["Suc"] = "Item deleted successfully";
            }
            catch (Exception ex)
            {
                int maxLength = 120;
                if (ex.Message.Length > maxLength)
                    TempData["err"] = ex.Message.Substring(0, maxLength);
                else
                    TempData["err"] = ex.Message;
            }

            return RedirectToAction("Index", "Items", new { actions = "Delete" });
        }

        public ActionResult Download()
        {
            string _path = @"../UploadedFiles/FileUploadFormat/Template Item Master.xlsx";
            if (System.IO.File.Exists(Server.MapPath(_path)))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(Server.MapPath(_path));
                return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, "TemplateItemMaster.xlsx");
            }

            return RedirectToAction("Index", "Items", new { actions = "Download" });
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

            return RedirectToAction("Index", "Items", new { actions = "Export" });
        }
    }
}