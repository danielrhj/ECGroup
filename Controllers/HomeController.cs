using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECGroup.Models;
//using BLL;
using Zephyr.Core;
using Zephyr.Utils;
using System.Configuration;

namespace ECGroup.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/F
        //BLL_Home bh = new BLL_Home();
        //BLL_User bu = new BLL_User();
        SYS_UserInfo ui = new SYS_UserInfo();

        public ActionResult Index()
        {
            var defaults = new Dictionary<string, object>();
            defaults.Add("theme", "default");
            defaults.Add("navigation", "tree");//accordion
            defaults.Add("gridrows", "10");
            ViewBag.Title = ConfigurationManager.AppSettings["LoginTitleChinese"].ToString();
            ViewBag.Settings = defaults; //

            ViewBag.UserName = FormsAuth.GetUserData().UserCode;
            return View();
        }

        public ActionResult main()
        {
            return View();
        }

        //[AllowAnonymous]
        //public ActionResult Default()
        //{
        //    ViewBag.List = bh.GetLinkList();
        //    ViewBag.News = bh.GetNewsList();
        //    ViewBag.contact = bh.GetcontactList();
        //    ViewBag.Help = bh.GetHelpList();
        //    return View();
        //}
        //public ActionResult Default1()
        //{
        //    ViewBag.List = bh.GetLinkList();
        //    ViewBag.News = bh.GetNewsList();
        //    ViewBag.contact = bh.GetcontactList();
        //    ViewBag.Help = bh.GetHelpList();
        //    ui.UserAccount = Request["userid"].ToString();
        //    ui.PWd = Request["pwd"].ToString();
        //    string signno = Request["SignCode"].ToString();
        //    bu.UserInfo_Model = ui;
        //    if (bu.Login(ui))
        //    {
        //        Session["Userid"] = ui.UserAccount.ToString();
        //        return Redirect("/Sign/SignItem?SignCode="+signno);
        //    }
        //    else
        //    {
        //        return View();
        //    }
        //}

        [HttpPost]
        [AllowAnonymous]
        public  string  Login()
        {            
            ui.UserAccount = Request["Userid"].ToString();
            ui.Password = Request["Password"].ToString();
            
            if (SYS_UserInfoService.Login(ui))
            {
                ui = new SYS_UserInfoService().GetUserModel(ui.UserAccount);
                Session["Userid"] = ui.UserAccount.ToString();
                //寫入cookie備用
                var loginer = new { UserCode = ui.UserAccount, UserName = ui.UserName };                
                //new BLL_DLL().SignIn(ui.UserAccount, loginer, 30);
                //Zephyr.Utils.ZCookies.WriteCookies("UserCode", 3600, ui.UserAccount);
                //Zephyr.Utils.ZCookies.WriteCookies("PWD", 3600, ui.PWd);

                return "1";
            }
            else
            {
                return "0";
            }
        }

        //[HttpPost]
        //public string Login1()
        //{
        //    ui.UserAccount = Request["userid"].ToString();
        //    ui.PWd = Request["pwd"].ToString();
        //    string signno = Request["Form_ID"].ToString();
        //    bu.UserInfo_Model = ui;
        //    if (bu.Login(ui))
        //    {
        //        Session["Userid"] = ui.UserAccount.ToString();
        //        Response.Redirect("/Sign/SignItem");
        //        return "1";
        //    }
        //    else
        //    {
        //        return "0";
        //    }
        //}

        #region 註冊
        //public ActionResult Regist()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult Regist(RegistInfo model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //            if (bu.CreatUser(model))
        //            {
        //                ModelState.AddModelError("", "註冊成功！請關閉本頁面進行系統登錄！");
        //            }
        //            else
        //            {
        //                ModelState.AddModelError("", "工號已存在！");
        //            }
        //    }
        //    return View();
        //}
        #endregion

        #region 按登入賬號獲取對應菜單
        public string GetUserMenu()
        {
            string UserID = this.User.Identity.Name;
            var MenuList = SYS_UserInfoService.GetUserMenu(UserID);

            return MenuList;
        }
        #endregion

        public void Download()
        {
            //BLL.Exporter.Instance().Download();
            //Zephyr.Core.Exporter方法由於寫死了Zephyr的命名空間，所以只好改寫了這一段放在BLL裡面。目前最大只能導出1000行。
        }
       
        public string changePWD()
        {
            string UserCode = Request["key"].ToString();
            string oldPWD = Request["oldPWD"].ToString();
            string newPWD = Request["newPWD"].ToString();
            string newCFMPWD = Request["newCFMPWD"].ToString();

            string AA = SYS_UserInfoService.ChangePWD(UserCode, oldPWD, newPWD);

            return AA;
        }
    }
}
