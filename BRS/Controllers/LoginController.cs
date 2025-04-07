using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Mvc;
using BRS.Models;
using BRS.ViewModels;
using System.Web.Security;

namespace BRS.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            LOGIN_DA loginDA = new LOGIN_DA();
            UserLibrary.UserData userdat = new UserLibrary.UserData();
            return View(userdat);
        }

        [HttpPost]
        public ActionResult Index(UserLibrary.UserData userdat)
        {
            LOGIN_DA loginDA = new LOGIN_DA();
            
            if (userdat.UserId != null && userdat.Password != null)
            {
                bool result = loginCheck(userdat);

                if (result)
                {
                    FormsAuthentication.SetAuthCookie(userdat.UserId, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View(userdat);
                }
            }
            else
            {
                return View(userdat);
            }
        }

        protected bool loginCheck(UserLibrary.UserData userdat)
        {
            bool returnValue = true;

            try
            {
                string userId = userdat.UserId;
                string password = LOGIN_DA.Encrypt(userdat.Password);
                
                LOGIN_DA loginDA = new LOGIN_DA();
                MenuResult mr = loginDA.getUserModule(userId, password);
                if (mr.errorMessage.Length > 0)
                    throw new Exception(mr.errorMessage);

                if (mr.moduleCategories.Count() == 0)
                {
                    userdat.visibleErrorLogin = "visible";
                    returnValue = false;
                }
                else
                {
                    LoginData.userId = userId;
                    LoginData.brandName = mr.brandName;
                    LoginData.umc = mr.moduleCategories;
                    loginDA.UpdateLastLogin(userId);
                }
            }
            catch (Exception ex)
            {
                TempData["AlertErrorMessage"] = ex.Message;
                returnValue = false;
            }

            return returnValue;
        }
    }
}