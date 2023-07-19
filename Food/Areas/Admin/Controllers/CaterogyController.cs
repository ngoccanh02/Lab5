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
    public class CaterogyController : Controller
    {
        FoodStoreEntities db = new FoodStoreEntities();
        // GET: Admin/Promotion
        public ActionResult Index(int? size, int? page, string sortProperty, string searchString, string sortOrder = "")
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "UserA");
            }
            ViewBag.Keyword = searchString;
           
            IQueryable<Category> category = db.Categories;
            if (!String.IsNullOrEmpty(searchString))
                category = category.Where(b => b.Name.Contains(searchString));
            if (!String.IsNullOrEmpty(searchString))
                category = category.Where(b => b.Name.Contains(searchString));




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
            return View(category.OrderBy(n => n.CategoryID).ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Details(int id)
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "UserA");
            }
            var caterogy = db.Categories.SingleOrDefault(n => n.CategoryID == id);
            if (caterogy == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(caterogy);
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
       
        public ActionResult Create(Category category, FormCollection f)
        {
            
                category.Name = f["sTenLSP"];
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
         
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "UserA");
            }
            var account = db.Categories.SingleOrDefault(n => n.CategoryID == id);
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
            var account = db.Categories.SingleOrDefault(n => n.CategoryID == id);
            if (account == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            db.Categories.Remove(account);
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
            var caterogy = db.Categories.SingleOrDefault(n => n.CategoryID == id);
            if (caterogy == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(caterogy);
        }
        [HttpPost]
        public ActionResult Edit(Category category, FormCollection f,int id)
        {
            category = db.Categories.SingleOrDefault(n => n.CategoryID == id);
            if (ModelState.IsValid)
            {
                category.Name = f["sTenLSP"];
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();

        }
    } 
}