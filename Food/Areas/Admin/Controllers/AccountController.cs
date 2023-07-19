using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using Food.Models;
using System.IO;
namespace AdminFood.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
       FoodStoreEntities db = new FoodStoreEntities();
        // GET: Admin/Account
        public ActionResult Index(int? size, int? page, string sortProperty, string searchString, string sortOrder = "")
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "UserA");
            }
            ViewBag.Keyword = searchString;
            IQueryable<Account> accounts = db.Accounts;
            if (!String.IsNullOrEmpty(searchString))
              accounts  = accounts.Where(b => b.DisplayName.Contains(searchString));
            if (!String.IsNullOrEmpty(searchString))
                accounts = accounts.Where(b => b.DisplayName.Contains(searchString));

            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "5", Value = "5" });
            items.Add(new SelectListItem { Text = "10", Value = "10" });
            items.Add(new SelectListItem { Text = "20", Value = "20" });
            items.Add(new SelectListItem { Text = "25", Value = "25" });
            items.Add(new SelectListItem { Text = "50", Value = "50" });
            items.Add(new SelectListItem { Text = "100", Value = "100" });
            items.Add(new SelectListItem { Text = "200", Value = "200" });
            foreach (var item in items)
            {
                if (item.Value == size.ToString()) item.Selected = true;
            }
            ViewBag.Size = items;
            ViewBag.CurrentSize = size;
            page = page ?? 1;
            int pageSize = (size ?? 5);
            ViewBag.pageSize = pageSize;
            int pageNumber = (page ?? 1);
            int checkTotal = (int)(db.Accounts.ToList().Count / pageSize) + 1;
            if (pageNumber > checkTotal) pageNumber = checkTotal;
            return View(accounts.OrderBy(n => n.AccountID).ToPagedList(pageNumber, pageSize));
        }
        [HttpGet]
        public ActionResult Create()
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "UserA");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Account acount, FormCollection f)
        {
            
                ViewBag.UserName = f["sTenDN"];
                ViewBag.DisplayName = f["sTenND"];
                ViewBag.Password = f["sMK"];
                ViewBag.Email = f["sEM"];
                ViewBag.PhoneNumber = f["sPN"];
                ViewBag.Status = int.Parse(f["sTT"]);
                
            
            acount.UserName = f["sTenDN"];
                acount.DisplayName = f["sTenND"];
                acount.Password = f["sMK"];
                acount.Email = f["sEM"];
                acount.PhoneNumber = f["sPN"];
                acount.Status = int.Parse(f["sTT"]);
                db.Accounts.Add(acount);
                db.SaveChanges();
                return RedirectToAction("Index");
            
        }
        public ActionResult Details(int id)
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "UserA");
            }
            var account = db.Accounts.SingleOrDefault(n => n.AccountID == id);
            if (account == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(account);
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "UserA");
            }
            var account = db.Accounts.SingleOrDefault(n => n.AccountID == id);
            if (account == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(account);
        }
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirm(int id)
        {
            var account = db.Accounts.SingleOrDefault(n => n.AccountID == id);
            if (account == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            
            db.Accounts.Remove(account);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "UserA");
            }
            var account = db.Accounts.SingleOrDefault(n => n.AccountID == id);
            if (account == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(account);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection f ,Account acount,int id)
        {
           
            acount = db.Accounts.SingleOrDefault(n => n.AccountID == id);
            if (ModelState.IsValid)
            {
               
                acount.UserName = f["sTenDN"];
                acount.DisplayName = f["sTenND"];
                acount.Password = f["sMK"];
                acount.Email = f["sEM"];
                acount.PhoneNumber = f["sPN"];
                acount.Status = int.Parse(f["sTT"]);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(acount);
            
        }

    }
}
