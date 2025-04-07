using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Configuration;
using System.Web;
using BRS.Models;
using BRS.ViewModels;
using BRS.Dictionary;
using System.Web.Mvc;
using System.Reflection;

namespace BRS.Controllers
{
    public class UsersController : Controller
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

        // GET: User
        public ActionResult Index(int page = 1, string searchFieldUser = "", string searchValueUser = "", string actions = "")
        {
            if (LoginData.userId is null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                USERS_DA UsersDA = new USERS_DA();
                UserLibrary.UserManagement usersData;
                if (TempData.Peek("_user") is null)
                {
                    DataSet dsUser = new DataSet();
                    dsUser = UsersDA.GetMsUser(page, "");
                    
                    usersData = new UserLibrary.UserManagement
                    {
                        dtUserList = dsUser.Tables[0],
                        pager = new Pager((dsUser.Tables[1] != null && dsUser.Tables[1].Rows.Count > 0) ? Convert.ToInt32(dsUser.Tables[1].Rows[0]["TotalRecords"]) : 0, 1, Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]))
                    };

                    TempData["action"] = "";
                    TempData["_user"] = usersData;
                }
                else
                {
                    usersData = (UserLibrary.UserManagement)TempData.Peek("_user");
                }

                if (searchFieldUser.Length == 0 && actions == "UsersSearch")
                {
                    TempData["err"] = "Silakan pilih filter terlebih dahulu!";
                }
                
                usersData = userBindGrid(page, searchFieldUser, searchValueUser, usersData);
                if (usersData.dtUserList.Rows.Count == 0)
                    TempData["err"] = "Data tidak ditemukan!";

                TRANS_DA TransDA = new TRANS_DA();
                ViewBag.UserSearchSelectList = new SelectList(UserSearch.UserSearchDictionary, "Key", "Value");
                ViewBag.UserBrandSelectList = new SelectList(TransDA.GetBrandList(), "Key", "Value");
                return View(usersData);
            }
        }

        [HttpPost]
        [ButtonNameAction]
        public ActionResult UsersSearch(UserLibrary.UserManagement users, FormCollection fc)
        {
            TempData["_users"] = users;
            return RedirectToAction("Index", "Users", new { searchFieldUser = fc["searchFieldUser"], searchValueUser = fc["searchValueUser"], actions = "UsersSearch" });
        }

        [HttpPost]
        [ButtonNameAction]
        public ActionResult ClearFilter(UserLibrary.UserManagement users)
        {
            TempData["action"] = "ClearFilter";
            return RedirectToAction("Index", "Users", new { actions = "ClearFilter" });
        }

        [HttpPost]
        [ButtonNameAction]
        public ActionResult Create(UserLibrary.UserManagement users, FormCollection fc)
        {
            try
            {
                if (users.userId == null || users.userId == string.Empty)
                    throw new Exception("Userid must be filled first!");
                if (users.userName == null || users.userName == string.Empty)
                    throw new Exception("Username must be filled first!");
                if (users.password == null || users.password == string.Empty)
                    throw new Exception("Password must be filled first!");
                else if (users.brandId == 0)
                    throw new Exception("Brand must be select first!");

                //Check ada username yang double gk?
                string where = string.Format("and US.UserId = '{0}'", users.userId);
                DataSet dsUserId = new DataSet();
                USERS_DA usersDA = new USERS_DA();

                dsUserId = usersDA.GetMsUser(1, where);
                if (dsUserId.Tables[0].Rows.Count > 0)
                {
                    //User already exists
                    TempData["err"] = "Username already exists!";
                }
                else
                {
                    //create master USER
                    string errMessage = usersDA.insertMsUser(users.userId, users.userName, LOGIN_DA.Encrypt(users.password), users.brandId, "USERS");
                    if (errMessage.Length > 0)
                        throw new Exception(errMessage);

                    TempData["suc"] = "User added successfully";
                }
            }
            catch (Exception ex)
            {
                TempData["show1"] = 1;
                int maxLength = 120;
                if (ex.Message.Length > maxLength)
                    TempData["err"] = ex.Message.Substring(0, maxLength);
                else
                    TempData["err"] = ex.Message;
            }

            TempData["_user"] = users;
            return RedirectToAction("Index", "Users", new { actions = "Create" });
        }

        [HttpPost]
        [ButtonNameAction]
        public ActionResult Update(UserLibrary.UserManagement users, FormCollection fc)
        {
            try
            {
                string userId = fc["userId"].ToString().Split(',')[1];
                string userName = fc["userName"].ToString().Split(',')[1];
                int brandId = Convert.ToInt32(fc["brandId"].ToString().Split(',')[1]);

                if (userId == "")
                    throw new Exception("Userid must be filled first!");
                if (userName == "")
                    throw new Exception("Username must be filled first!");
                if (brandId == 0)
                    throw new Exception("Brand must be filled first!");

                //update master USER
                USERS_DA usersDA = new USERS_DA();
                string errMessage = usersDA.updateMsUser(userId, userName, brandId, users.active);
                if (errMessage.Length > 0)
                    throw new Exception(errMessage);

                TempData["suc"] = "User updated successfully";
            }
            catch (Exception ex)
            {
                TempData["show2"] = 1;
                int maxLength = 120;
                if (ex.Message.Length > maxLength)
                    TempData["err"] = ex.Message.Substring(0, maxLength);
                else
                    TempData["err"] = ex.Message;
            }

            TempData["_user"] = users;
            return RedirectToAction("Index", "Users", new { actions = "Update" });
        }

        private UserLibrary.UserManagement userBindGrid(int page, string searchFieldUser, string searchValueUser, UserLibrary.UserManagement usersData)
        {
            string where = string.Empty;

            if (searchFieldUser.Length > 0)
            {
                where = where + $"{searchFieldUser} like '%{searchValueUser}%' and ";
            }
            

            if (where != "")
            {
                where = "and " + where.Remove(where.Length - 5);
            }

            usersData.searchFieldUser = searchFieldUser;
            usersData.searchValueUser = searchValueUser;
            USERS_DA UsersDA = new USERS_DA();
            DataSet ds = new DataSet();
            ds = UsersDA.GetMsUser(page, where);

            usersData.dtUserList = ds.Tables[0];
            var pager = new Pager((ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0) ? Convert.ToInt32(ds.Tables[1].Rows[0]["TotalRecords"]) : 0, page, Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]));
            usersData.pager = pager;
            return usersData;
        }

        [HttpPost]
        [ButtonNameAction]
        public ActionResult ResetPassword(UserLibrary.UserManagement users, FormCollection fc)
        {
            try
            {
                string userId = fc["UserId"].Split(',')[0].Trim();
                USERS_DA usersDA = new USERS_DA();
                string errMessage = usersDA.resetPassword(userId);
                if (errMessage.Length > 0)
                    throw new Exception(errMessage);

                TempData["Suc"] = "Password successfully reset";
            }
            catch (Exception ex)
            {
                int maxLength = 120;
                if (ex.Message.Length > maxLength)
                    TempData["err"] = ex.Message.Substring(0, maxLength);
                else
                    TempData["err"] = ex.Message;
            }

            return RedirectToAction("Index", "Users", new { actions = "Reset" });
        }
    }
}