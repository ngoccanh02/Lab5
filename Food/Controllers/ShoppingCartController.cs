using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Food.Models;
using Food.common;

namespace Food.Controllers
{
    public class ShoppingCartController : Controller
    {
        // GET: ShoppingCart
        FoodStoreEntities db = new FoodStoreEntities();
         public List<CartDetail> LayGioHang()
        {
            List<CartDetail> lstGioHang = Session["GioHang"] as List<CartDetail>;
            if (lstGioHang == null)
            {
                lstGioHang = new List<CartDetail>();
                Session["GioHang"] = lstGioHang;
            }
            return lstGioHang;
        }
        public ActionResult ThemGioHang(int msp, string url)
        {
            if (Session["TaiKhoan"] == null)
            {
                List<CartDetail> lstGioHang = LayGioHang();
                CartDetail sp = lstGioHang.Find(n => n.ProductID == msp);
                if (sp == null)
                {
                        sp = new CartDetail(msp);
                        lstGioHang.Add(sp);
                }
                else
                {
                    sp.Quantity++;
                }
            }
            else
            {
                int mgh = int.Parse(Session["CartID"].ToString());
                CartDetail sp = db.CartDetails.SingleOrDefault(n =>n.ProductID == msp && n.ShoppingCartID==mgh );
                if (sp == null)
                {
                    CartDetail spi = new CartDetail(msp);
                    spi.ShoppingCartID = Convert.ToInt32(Session["CartID"]);
                    db.CartDetails.Add(spi);
                    db.SaveChanges();
                }
                else
                {
                    sp.Quantity++;
                    db.SaveChanges();
                }
            }
            return Redirect(url);
        }
        private double TongTien()
        {
            double dTongTien = 0;
            //
            if(Session["TaiKhoan"] == null)
            {
                List<CartDetail> lstGioHang = Session["GioHang"] as List<CartDetail>;
                if (lstGioHang != null)
                {
                    dTongTien = Convert.ToInt32(lstGioHang.Sum(n => n.TotalPrice));
                }
            }
            else
            {
                int mspc = Convert.ToInt32(Session["CartID"]);
                List<CartDetail> lstGioHang = (from s in db.CartDetails
                                             where s.ShoppingCartID== mspc
                                             select s).ToList();
                
                if (lstGioHang != null)
                {
                    dTongTien = Convert.ToInt32(lstGioHang.Sum(n=>n.Quantity*n.Price));
                }
            }
            return dTongTien;
        }
        private double NeedPay()
        {
            double dTongTien = 0;
            int mspc = Convert.ToInt32(Session["CartID"]);
            List<CartDetail> lstGioHang = (from s in db.CartDetails
                                           where s.ShoppingCartID == mspc && s.PayNeed == true
                                               select s).ToList();

                if (lstGioHang != null)
                {
                    dTongTien = Convert.ToInt32(lstGioHang.Sum(n => n.Quantity * n.Price));
                }
            
            return dTongTien;
        }
        private int TongSoLuong()
        {
            int iTongSoLuong = 0;
            if (Session["TaiKhoan"] == null)
            {
                List<CartDetail> lstGioHang = Session["GioHang"] as List<CartDetail>;
                if (lstGioHang != null)
                {
                    iTongSoLuong = Convert.ToInt32(lstGioHang.Count);
                }
            }
            else
            {
                int mspc = Convert.ToInt32(Session["CartID"]);
                List<CartDetail> lstGioHang = db.CartDetails.Where(n=>n.ShoppingCartID==mspc).ToList();
                if (lstGioHang!= null)
                {
                    iTongSoLuong = Convert.ToInt32(lstGioHang.Count);
                }
            }
            return iTongSoLuong;
        }
        public ActionResult GioHang()
        {
            if(Session["TaiKhoan"] == null)
            {
                List<CartDetail> lstGioHang = LayGioHang();
                if (lstGioHang.Count == 0)
                {
                    return RedirectToAction("Index", "Food");
                }
                ViewBag.TongSoLuong = TongSoLuong();
                ViewBag.TongTien = TongTien();
                return View(lstGioHang);
            }
            else 
            {
                int mspc = Convert.ToInt32(Session["CartID"]);
                List<CartDetail> lstGioHang = (from s in db.CartDetails
                                               where s.ShoppingCartID == mspc
                                               select s).ToList();
                if (lstGioHang.Count== 0)
                {
                    return RedirectToAction("Index", "Food");
                }

                ViewBag.TongSoLuong = TongSoLuong();
                ViewBag.TongTien = TongTien();
                return View(lstGioHang);
            }
        }
        public ActionResult Delete(int idProduct)
        {
            if (Session["TaiKhoan"] == null)
            {
                List<CartDetail> lstGioHang = LayGioHang();
                CartDetail sp = lstGioHang.SingleOrDefault(n => n.ProductID == idProduct);
                if (sp != null)
                {
                    lstGioHang.RemoveAll(n => n.ProductID == idProduct);
                    if (lstGioHang.Count == 0)
                    {
                        return RedirectToAction("Index", "Food");
                    }
                }
                return RedirectToAction("GioHang");
            }
            else
            {
                int mspc = Convert.ToInt32(Session["CartID"]);
                CartDetail rm = db.CartDetails.SingleOrDefault(m => m.ShoppingCartID == mspc && m.ProductID == idProduct);
                db.CartDetails.Remove(rm);
                db.SaveChanges();
                return RedirectToAction("GioHang");
            }
        }
        public ActionResult GioHangPartial()
        {
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            return PartialView();
        }
        public ActionResult CapNhatGioHang(int ProductID, FormCollection f)
        {
            if (Session["TaiKhoan"] == null)
            {
                List<CartDetail> lstGioHang = LayGioHang();
                CartDetail sp = lstGioHang.SingleOrDefault(n => n.ProductID == ProductID);
                if (sp != null)
                {
                    sp.Quantity = Int32.Parse(f["Quantity"].ToString());
                }
                return RedirectToAction("GioHang");
            }
            else
            {
                int mspc = Convert.ToInt32(Session["CartID"]);
                CartDetail lstGioHang = db.CartDetails.SingleOrDefault(n => n.ShoppingCartID == mspc && n.ProductID==ProductID);
                if(lstGioHang !=null)
                {
                    lstGioHang.Quantity= Int32.Parse(f["Quantity"].ToString());
                    db.SaveChanges();
                }
                return RedirectToAction("GioHang");
            }
        }
        [HttpGet]
        public ActionResult Payment()
        {
            if(Session["TaiKhoan"]==null)
            {
                return Redirect("~/User/Login?id=2");
            }
            
            else
            {
                int mspc = Convert.ToInt32(Session["CartID"]);
                var pay = from s in db.CartDetails
                                        where s.ShoppingCartID == mspc && s.PayNeed==true 
                                        select s;
                Session["pay"] = (from s in db.CartDetails
                                  where s.ShoppingCartID == mspc && s.PayNeed == true
                                  select s).ToList();
                ViewBag.TongTien = NeedPay();
                if(pay==null)
                {
                    return this.GioHang();
                }
                return View(pay);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(FormCollection f)
        {
            List<CartDetail> em = Session["pay"] as List<CartDetail>;
            Account ac = (Account)Session["TaiKhoan"];
            if (f["address"] == null)
            {
                ViewData["err1"] = "Địa chỉ không được để trống";
            }
            else if (f["sdt"] == null)
            {
                ViewData["err2"] = "Số điện thoại không được để trống";
            }
            else if (f["password"] != ac.Password)
            {
                ViewData["err3"] = "Vui lòng kiểm tra lại  mật khẩu";
            }
            else
            {
                if (Convert.ToInt32(f["pay"]) == 0)
                {
                    Order ctac = db.Orders.FirstOrDefault(n => n.AccountID == ac.AccountID && n.Date == DateTime.Now);
                    if (ctac == null)
                    {
                        Order od = new Order();
                        od.Date = DateTime.Now;
                        od.AccountID = ac.AccountID;
                        db.Orders.Add(od);
                        db.SaveChanges();
                    }
                    var noidung = "";
                    
                    
                    Order ct = db.Orders.OrderByDescending(m => m.Date).FirstOrDefault(n => n.AccountID == ac.AccountID);
                    foreach (var item in em)
                    {
                        OrderDetail ctdh = new OrderDetail();
                        ctdh.OrderID = ct.OrderID;
                        ctdh.ProductID = item.ProductID;
                        ctdh.Quantity = Convert.ToInt32(item.Quantity);
                        Product pr = db.Products.SingleOrDefault(m => m.ProductID == item.ProductID);
                        noidung += "  <tr>";
                        noidung +="<td style ='color:#636363;border:1px solid #e5e5e5;padding:12px;text-align:left;vertical-align:middle;font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif;word-wrap:break-word'>";
         		        noidung += pr.Name + "</td>";
                        noidung += "<td style ='color:#636363;border:1px solid #e5e5e5;padding:12px;text-align:left;vertical-align:middle;font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif'>";
                        noidung += item.Quantity + "</td>";
                        noidung += "<td style ='color:#636363;border:1px solid #e5e5e5;padding:12px;text-align:left;vertical-align:middle;font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif'><span>";
                        noidung += pr.Price+ "&nbsp;<span> VND </span></span></td>" ;
                        ctdh.CustomerName = f["name"];
                        ctdh.Address = f["address"];
                        ctdh.PhoneNumber = f["sdt"];
                        ctdh.Email = f["email"];
                        ctdh.PayStatus = 0;
                        db.OrderDetails.Add(ctdh);
                        db.SaveChanges();
                        CartDetail cd = db.CartDetails.SingleOrDefault(n => n.ProductID == ctdh.ProductID);
                        db.CartDetails.Remove(cd);
                        db.SaveChanges();
                    }
                    string content = System.IO.File.ReadAllText(Server.MapPath("~/Content/template/send1.html"));
                    int Total = Convert.ToInt32(em.Sum(n => n.Quantity * n.Price)); ;
                    content = content.Replace("{{CustomerName}}", f["name"]);
                    content = content.Replace("{{Phone}}", f["sdt"]);
                    content = content.Replace("{{Address}}", f["address"]);
                    content = content.Replace("{{Email}}", f["email"]);
                    content = content.Replace("{{Date}}",DateTime.Now.ToString("MM/dd/yyyy"));
                    content = content.Replace("{{Total}}", Total.ToString("N0"));
                    content = content.Replace("{{mes}}",noidung.ToString());

                    var toEmail = ConfigurationManager.AppSettings["ToEmailAddress"].ToString();

                    // Để Gmail cho phép SmtpClient kết nối đến server SMTP của nó với xác thực 
                    //là tài khoản gmail của bạn, bạn cần thiết lập tài khoản email của bạn như sau:
                    //Vào địa chỉ https://myaccount.google.com/security  Ở menu trái chọn mục Bảo mật, sau đó tại mục Quyền truy cập 
                    //của ứng dụng kém an toàn phải ở chế độ bật
                    //  Đồng thời tài khoản Gmail cũng cần bật IMAP
                    //Truy cập địa chỉ https://mail.google.com/mail/#settings/fwdandpop

                    new MailHelper().SendMail(f["email"], "Đơn hàng mới từ Hưng Vũ Vegetable", content);
                    new MailHelper().SendMail(toEmail, "Đơn hàng mới từ Hưng Vũ Vegetable", content);
                    Session["pay"] = null;
                    ViewBag.ThongBao = "Đặt hàng thành công.Vào mục đơn hàng để xem chi tiết ";
                    return this.GioHang();
                }
                else
                {
                    double TT;
                    if (em != null)
                    {
                        TT = Convert.ToDouble(em.Sum(n => n.Quantity * n.Price)) * 100;
                        return RedirectToAction("Payment", "Food", new { value = TT.ToString() });
                    }
                    else
                    {
                        return RedirectToAction("GioHang", "ShoppingCart");
                    }
                }
            }
            if (em.Count > 1)
            {
                return this.Payment();
            }
            else
            {
                CartDetail cnew = em.Single();
                return this.Payment();
            }
        }
        public JsonResult PayNeed(int id)
        {
            try
            {
                int mspc = Convert.ToInt32(Session["CartID"]);
                CartDetail cd = db.CartDetails.SingleOrDefault(m => m.ShoppingCartID == mspc && m.ProductID == id);
                if (cd.PayNeed == true)
                {
                    cd.PayNeed = false;
                }
                else
                {
                    cd.PayNeed = true;
                }
                db.SaveChanges();
                return Json(new { code = 200 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { code = 500}, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult PayNeedNo(int id)
        {
            try
            {
                int mspc = Convert.ToInt32(Session["CartID"]);
                CartDetail cd = db.CartDetails.SingleOrDefault(m => m.ShoppingCartID == mspc && m.ProductID == id);
                cd.PayNeed = false;
                db.SaveChanges();
                return Json(new { code = 200 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { code = 500 }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}