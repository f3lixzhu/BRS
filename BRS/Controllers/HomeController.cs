using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BRS.Models;
using BRS.ViewModels;
using System.Web.Security;

namespace BRS.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (LoginData.userId is null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                return View();
            }
        }

        public ActionResult ChangePassword()
        {
            if (LoginData.userId is null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                PasswordData pass = new PasswordData();
                return View(pass);
            }
        }

        [HttpPost]
        public ActionResult ChangePassword(PasswordData pass)
        {
            if (pass.password != null && pass.confirmpassword != null)
            {
                if (pass.password != pass.confirmpassword)
                {
                    TempData["err"] = "Confirm password doesn't equal with password!";
                    return View(pass);
                }
                else
                {
                    LOGIN_DA loginDA = new LOGIN_DA();
                    string result = loginDA.ChangePassword(LoginData.userId, LOGIN_DA.Encrypt(pass.password));

                    if (result.Length == 0)
                    {
                        TempData["suc"] = "Change password successfully";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["err"] = result;
                        return View(pass);
                    }
                }
            }
            else
            {
                return View(pass);
            }
        }

        public ActionResult Reset(PasswordData pass)
        {
            pass.password = string.Empty;
            pass.confirmpassword = string.Empty;
            return RedirectToAction("ChangePassword", "Home");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }

        [ChildActionOnly]
        public ActionResult NavMenu()
        {
            ViewBag.brandName = LoginData.brandName;
            ViewBag.UserModuleCategory = LoginData.umc;
            return PartialView("NavMenu");
        }
    }
}