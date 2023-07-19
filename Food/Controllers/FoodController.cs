using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Food.common;
using Food.Models;
using Food.Other;
using PagedList;
using PagedList.Mvc;

namespace Food.Controllers
{
    public class FoodController : Controller
    {
        // GET: Food
        FoodStoreEntities db = new FoodStoreEntities();
        public ActionResult Index()
        {
            return View(db.Products);
        }
        public ActionResult Shop(int? page)
        {
            int size = 6;
            int iPageNum = (page ?? 1);
            return View(db.Products.OrderBy(s=>s.ProductID).ToPagedList(iPageNum, size));
        }
        public ActionResult Sale()
        {   
            return PartialView();
        }
        public ActionResult Category()
        {
            return PartialView(db.Categories);
        }
        public ActionResult SectionTitle()
        {
            return PartialView(db.Categories);
        }
        public ActionResult Trend()
        {
            var tr = from s in db.OrderDetails
                     select s;
            return PartialView(tr);
        }
        public ActionResult LoginLogout()
        {
            return PartialView();
        }
        public ActionResult Detail (int id)
        {
            var pro = from s in db.Products
                       where s.ProductID == id
                       select s;
            return View(pro.Single());
        }
        public ActionResult CategoryDetail(int id, int ? page)
        {
            ViewBag.IDCate = id;
            int size = 6;
            int iPageNum = (page ?? 1);
            var ca = from s in db.Products
                     where s.CategoryID == id
                     orderby s.ProductID
                     select s;
            return View(ca.ToPagedList(iPageNum,size));
        }
        public ActionResult PostReview()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Review(int id)
        {
            var rv = from s in db.Rate_
                     where s.ProductID == id
                     select s;
            return PartialView(rv);
        }
        [HttpGet]
        public ActionResult Promotion(int id)
        {
            var rv = from s in db.PromoDetails
                     where s.ProductID == id
                     select s;
            return PartialView(rv);
        }
        public ActionResult Star(int id)
        {
            List<Rate_> rv = (from s in db.Rate_
                             where s.ProductID == id
                             select s).ToList();
            ViewBag.count = rv.Count;
            ViewBag.Avg =Convert.ToInt32(rv.Average(m => m.NumberStar));
            return PartialView(rv);
        }
        [HttpPost]
        public ActionResult InsertReview(int id, FormCollection f)
        {
            Rate_ rv = new Rate_();
            Account ac = (Account)Session["TaiKhoan"];
            if (id!= 0)
            {
                rv.AccountID = ac.AccountID;
                rv.Comment = f["comment"];
                rv.NumberStar = Convert.ToInt32(f["star"]);
                rv.Date = DateTime.Now;
                rv.ProductID = id;
                db.Rate_.Add(rv);
                db.SaveChanges();
            }
            return RedirectToAction("Detail", new {id = id});
        }
        public ActionResult Find(string searchString)
        {
            var links = from l in db.Products // lấy toàn bộ liên kết
                       select l;
            if (!String.IsNullOrEmpty(searchString)) // kiểm tra chuỗi tìm kiếm có rỗng/null hay không
            {
                searchString = searchString.ToLower();
                links = links.Where(s => s.Decription.ToLower().Contains(searchString)); //lọc theo chuỗi tìm kiếm
            }
            return View(links.ToList());
        }
        public ActionResult Payment(double value)
        {
            string url = ConfigurationManager.AppSettings["Url"];
            string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
            string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["HashSecret"];
            Session["value"] = value;
            PayLib pay = new PayLib();

            pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
            pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
            pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
            pay.AddRequestData("vnp_Amount", value.ToString()); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
            pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
            pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
            pay.AddRequestData("vnp_IpAddr", Ulti.GetIpAddress()); //Địa chỉ IP của khách hàng thực hiện giao dịch
            pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
            pay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang"); //Thông tin mô tả nội dung thanh toán
            pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
            pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString()); //mã hóa đơn

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

            return Redirect(paymentUrl);
        }

        public ActionResult PaymentConfirm()
        {
            if (Request.QueryString.Count > 0)
            {
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"]; //Chuỗi bí mật
                var vnpayData = Request.QueryString;
                PayLib pay = new PayLib();
                int money =Convert.ToInt16(Session["value"]);
                //lấy toàn bộ dữ liệu được trả về
                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(s, vnpayData[s]);
                    }
                }

                long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef")); //mã hóa đơn
                long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo")); //mã giao dịch tại hệ thống VNPAY
                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode"); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"]; //hash của dữ liệu trả về

                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?

                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00")
                    {
                        //Thanh toán thành công
                        ViewBag.Message = "Thanh toán thành công hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId;
                        Account ac = (Account)Session["TaiKhoan"];
                        Order ctac = db.Orders.SingleOrDefault(n => n.AccountID == ac.AccountID);
                        if (ctac == null)
                        {
                            Order od = new Order();
                            od.Date = DateTime.Now;
                            od.AccountID = ac.AccountID;
                            db.Orders.Add(od);
                            db.SaveChanges();
                        }
                        var noidung = "";
                        List<CartDetail> em = Session["pay"] as List<CartDetail>;
                        Order ct = db.Orders.Single(n => n.AccountID == ac.AccountID);
                        foreach (var item in em)
                        {
                            OrderDetail ctdh = new OrderDetail();
                            ctdh.OrderID = ct.OrderID;
                            ctdh.ProductID = item.ProductID;
                            Product pr = db.Products.SingleOrDefault(m => m.ProductID == item.ProductID);
                            ctdh.Quantity = Convert.ToInt32(item.Quantity);
                            noidung += "  <tr>";
                            noidung += "<td style ='color:#636363;border:1px solid #e5e5e5;padding:12px;text-align:left;vertical-align:middle;font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif;word-wrap:break-word'>";
                            noidung += pr.Name + "</td>";
                            noidung += "<td style ='color:#636363;border:1px solid #e5e5e5;padding:12px;text-align:left;vertical-align:middle;font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif'>";
                            noidung += item.Quantity + "</td>";
                            noidung += "<td style ='color:#636363;border:1px solid #e5e5e5;padding:12px;text-align:left;vertical-align:middle;font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif'><span>";
                            noidung += pr.Price + "&nbsp;<span> VND </span></span></td>";
                            ctdh.PayStatus = 1;
                            db.OrderDetails.Add(ctdh);
                            db.SaveChanges();
                            CartDetail cd = db.CartDetails.SingleOrDefault(n => n.ProductID == ctdh.ProductID);
                            db.CartDetails.Remove(cd);
                            db.SaveChanges();
                        }
                        OrderDetail dl = db.OrderDetails.FirstOrDefault(m => m.OrderID == ct.OrderID);
                        string content = System.IO.File.ReadAllText(Server.MapPath("~/Content/template/neworder.html"));
                        int Total = Convert.ToInt32(em.Sum(n => n.Quantity * n.Price)); ;
                        content = content.Replace("{{CustomerName}}", dl.CustomerName);
                        content = content.Replace("{{Phone}}", dl.PhoneNumber);
                        content = content.Replace("{{Email}}",dl.Email);
                        content = content.Replace("{{Address}}", dl.Address);
                        content = content.Replace("{{Date}}", DateTime.Now.ToString("MM/dd/yyyy"));
                        content = content.Replace("{{Total}}", money.ToString("N0"));
                        content = content.Replace("{{mes}}", noidung.ToString());
                        var toEmail = ConfigurationManager.AppSettings["ToEmailAddress"].ToString();

                        // Để Gmail cho phép SmtpClient kết nối đến server SMTP của nó với xác thực 
                        //là tài khoản gmail của bạn, bạn cần thiết lập tài khoản email của bạn như sau:
                        //Vào địa chỉ https://myaccount.google.com/security  Ở menu trái chọn mục Bảo mật, sau đó tại mục Quyền truy cập 
                        //của ứng dụng kém an toàn phải ở chế độ bật
                        //  Đồng thời tài khoản Gmail cũng cần bật IMAP
                        //Truy cập địa chỉ https://mail.google.com/mail/#settings/fwdandpop

                        new MailHelper().SendMail(dl.Address, "Đơn hàng mới từ Hưng Vũ Vegetable", content);
                        new MailHelper().SendMail(toEmail, "Đơn hàng mới từ Hưng Vũ Vegetable", content);
                        Session["pay"] = null;
                    }
                    else
                    {
                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
                    }
                }
                else
                {
                    ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý";
                }
            }

            return this.Index();
        }
        public JsonResult Collect(int id)
        {
            try
            {
                Account ac = (Account)Session["TaiKhoan"];
                Collect cl = db.Collects.SingleOrDefault(m=>m.PromotionID==id && m.AccountID==ac.AccountID);
                 if (cl==null)
                {
                    Collect npr = new Collect();
                    npr.AccountID = ac.AccountID;
                    npr.PromotionID = id;
                    db.Collects.Add(npr);
                    db.SaveChanges();
                    return Json(new { code = 200, msg = "Thêm thành công" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { code = 200, msg = "Bạn đã sưu tầm mã giảm giá này rồi" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = "Thêm thất bại" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Voucher()
        {
            Account ac = (Account)Session["TaiKhoan"];
            var cl = from s in db.Collects
                     where s.AccountID == ac.AccountID
                     select s;
            return View(cl);
        }
    }
}