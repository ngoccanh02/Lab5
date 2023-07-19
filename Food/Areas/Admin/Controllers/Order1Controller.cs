using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Food.Models;
using System.IO;
using System.Data.Entity;

namespace AdminFood.Areas.Admin.Controllers
{
    public class Order1Controller : Controller
    {
        // GET: Admin/Order1
        FoodStoreEntities db = new FoodStoreEntities();
        public ActionResult Index(int? size, int? page, string sortProperty, FormCollection f, string searchString, string sortOrder = "")
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "UserA");
            }
            ViewBag.Keyword = searchString;
            ViewBag.TT = PriceAll();
            IQueryable<OrderDetail> category = db.OrderDetails;
            if (!String.IsNullOrEmpty(searchString))
                category = category.Where(b => b.CustomerName.Contains(searchString));
            if (!String.IsNullOrEmpty(searchString))
                category = category.Where(b => b.CustomerName.Contains(searchString));
            



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
            int checkTotal = (int)(category.ToList().Count / pageSize) + 1;
            if (pageNumber > checkTotal) pageNumber = checkTotal;
            
            return View(category.OrderBy(n => n.OrderDetailID).ToPagedList(pageNumber, pageSize));
            
        }
        public ActionResult Details(int id)
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "UserA");
            }
            var account = db.OrderDetails.SingleOrDefault(n => n.OrderDetailID == id);
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
            var account = db.OrderDetails.SingleOrDefault(n => n.OrderDetailID == id);
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
            var account = db.OrderDetails.SingleOrDefault(n => n.OrderDetailID == id);
            if (account == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            db.OrderDetails.Remove(account);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Payment(int id)
        {
            var account = db.OrderDetails.SingleOrDefault(n => n.OrderDetailID == id);
            if (account == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            account.PayStatus = int.Parse("1");
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public double PriceAll()
        {
            List<OrderDetail> lst = (from s in db.OrderDetails select s).ToList();
            double TT = 0;
            if(lst != null)
            {
                TT = Convert.ToDouble(lst.Sum(n => n.Quantity * n.Product.Price));
            }
            return TT;
        }
    }
}