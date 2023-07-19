using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Food.Models;

namespace Food.Controllers
{
    public class HistoryOrderController : Controller
    {
        // GET: HistoryOrder
        FoodStoreEntities db = new FoodStoreEntities();
        public ActionResult ManageOrder()
        {
            Account ac = (Account)Session["TaiKhoan"];
            var mn = from s in db.OrderDetails
                     where s.Order.AccountID== ac.AccountID
                     select s;
            return View(mn);
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var mn = from s in db.OrderDetails
                     where s.OrderDetailID ==id
                     select s;
            return View(mn.Single());
        }
        [HttpPost]
        public ActionResult Edit(int id,FormCollection f)
        {
            OrderDetail od = db.OrderDetails.SingleOrDefault(n => n.OrderDetailID == id);
            if(od!=null)
            {
                
                if(String.IsNullOrEmpty(f["name"]))
                  {
                    ViewData["err1"] = "Họ tên người nhận không được để trống";
                    return View(od);
                }
                else if (String.IsNullOrEmpty(f["address"]))
                {
                    ViewData["err1"] = "Địa chỉ nhận hàng không được để trống";
                    return View(od); 
                }
                else if (String.IsNullOrEmpty(f["sdt"]))
                {
                    ViewData["err1"] = "Số điện thoại không được để trống";
                    return View(od);
                }
                else if (!IsValidPhoneNumber(f["sdt"]))
                {
                    ViewData["err1"] = "Số điện thoại không hợp lệ";
                    return View(od);
                }
                else if (String.IsNullOrEmpty(f["email"]))
                {
                    ViewData["err1"] = "Email không được để trống";
                    return View(od);
                }
                else if(!IsValidEmail(f["email"]))
                {
                    ViewData["err1"] = "Email không hợp lệ";
                    return View(od);
                }
                else
                {
                    od.CustomerName = f["name"];
                    od.Address = f["address"];
                    od.PhoneNumber = f["sdt"];
                    od.Email = f["email"];
                    db.SaveChanges();
                }
                    
            }
            return RedirectToAction("ManageOrder", "HistoryOrder");
        }
        public bool IsValidEmail(string emailAddress)
        {
            // Biểu thức chính quy để kiểm tra tính hợp lệ của địa chỉ email
            string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";

            // Kiểm tra địa chỉ email với biểu thức chính quy
            Match match = Regex.Match(emailAddress, pattern);

            // Trả về true nếu khớp với biểu thức chính quy, ngược lại trả về false
            return match.Success;
        }
        public bool IsValidPhoneNumber(string phoneNumber)
        {
            // Biểu thức chính quy để kiểm tra tính hợp lệ của số điện thoại
            string pattern = @"^(0|\+84)(3[2-9]|5[2689]|7[06789]|8[0-9]|9[0-9])[0-9]{7}$";

            // Kiểm tra số điện thoại với biểu thức chính quy
            Match match = Regex.Match(phoneNumber, pattern);

            // Trả về true nếu khớp với biểu thức chính quy, ngược lại trả về false
            return match.Success;
        }
    }
}