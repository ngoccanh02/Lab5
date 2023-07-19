using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Food.Models;
namespace AdminFood.Areas.Admin.Controllers
{
    public class UserAController : Controller
    {
        FoodStoreEntities db = new FoodStoreEntities();
        // GET: Admin/User
        public ActionResult Index()
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "UserA");
            }
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection f)
        {
            var sTenDN = f["UserName"];
            var sMatKhau = f["PassWord"];
            ADMIN ad = db.ADMINs.SingleOrDefault(n => n.UserName == sTenDN && n.PassWord == sMatKhau);
            if (ad != null)
            {
                Session["Account"] = ad;
                return RedirectToAction("Index","UserA");
            }
            else
            {
                ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
            }
            return View();
        }
        public ActionResult loginlogout()
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "UserA");
            }
            return PartialView("loginlogout");
        }
    }
}