using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zephyr.Core;
using ECGroup.Models;
using Zephyr.Utils;
//using Zephyr.Web;
using ECGroup.Areas.Mms.Common;
using System.Configuration;

namespace ECGroup.Controllers
{
    [AllowAnonymous]
    [MvcMenuFilter(false)]
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.CnName = ConfigurationManager.AppSettings["LoginTitleChinese"].ToString(); //""
            ViewBag.EnName = ConfigurationManager.AppSettings["LoginTitleEnglish"].ToString();//EC Group AR/AP
            ViewBag.EnNameStyle = "left:330px;height:50px;font-size:small";
            return View("Index");
        }        

        public JsonResult DoAction(JObject request)
        {
            var message = new SYS_UserInfoService().Login(request);
            return Json(message, JsonRequestBehavior.DenyGet);
        }

        public ActionResult Logout()
        {
            FormsAuth.SingOut();
            return Redirect("~/Login");
        }

        public FileResult VerificationCode()
        {
            var vc = new ECGroup.Models.VerificationCode();
            System.IO.MemoryStream ms = vc.CreateCheckCodeImage();
            byte[] bytes = ms.ToArray();
            return File(bytes, @"image/gif");
        }
    }
}
