using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Zephyr.Core;
using ECGroup.Models;

namespace ECGroup.Areas.Sys.Controllers
{
    public class MenuController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RoleMenu()
        {
            var model = new
            {
                dataSource = new
                {
                    MenuTreeData = new MenuApiController().GetAllForCombotree()
                }
            };

            return View(model);
        }
    }

    public class MenuApiController : ApiController
    {
        // GET api/menu
        public IEnumerable<dynamic> Get()
        {
            var UserCode = this.User.Identity.Name;
            return new sys_menuService().GetUserMenu(UserCode);
        }

        // GET api/menu
        public dynamic GetEnabled(string id)
        {
            var result = new sys_menuService().GetEnabledMenusAndButtons(id);
            return result;
        }

        // GET api/menu
        public IEnumerable<dynamic> GetAll()
        {
            var MenuService = new sys_menuService();
           
            ParamSP ps = new ParamSP().Name("usp_SYS_Base");
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetAllMenuList");

            var result=MenuService.GetDynamicList(ps);

            return result;
        }

        public IEnumerable<dynamic> GetAllForCombotree()
        {
            var MenuService = new sys_menuService();
            ParamSP ps = new ParamSP().Name("usp_SYS_Base");
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetAllMenuListForCombotree");

            var result = MenuService.GetDynamicList(ps);

            return result;
        }
        
        [System.Web.Http.HttpPost]
        public dynamic Edit(dynamic data)
        {   
            var service = new sys_menuService();            

            JObject gridData = JObject.Parse(data["list"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name("usp_SYS_Base");

            if (gridChange)
            {
                ps.Parameter("ActionType", "SaveMenu");
                ps.Parameter("UserId", User.Identity.Name);

                //MenuName', 'MenuCode', 'ParentCode', 'IconClass', 'URL', 'IsVisible', 'IsEnable', 'MenuSeq
                foreach (var item in gridData)
                {
                    string itemKey = item.Key;//deleted,updated,inserted
                    if (item.Value.HasValues)
                    {
                        ps.Parameter("ActionItem", itemKey);

                        JArray ActionData = item.Value as JArray; //注意這個數組的每個元素對應表格的一行編輯的記錄
                        foreach (var itemDetail in ActionData)
                        {
                            JObject dr = itemDetail as JObject;
                            foreach (var drField in dr)
                            {
                                ps.Parameter(drField.Key, drField.Value.ToString().Trim());
                            }
                            service.StoredProcedureNoneQuery(ps);
                        }
                    }
                }
            }
            return "OK";
        }

        public IEnumerable<dynamic> GetMenuButtons(string id)
        {
            return new sys_menuService().GetMenuButtons(id);
        }

        public IEnumerable<dynamic> GetButtons()
        {
            return new sys_menuService().GetButtons();
        }


        [System.Web.Http.HttpPost]
        public void EditMenuButtons(string id, dynamic data)
        {
            var service = new sys_menuService();
            service.SaveMenuButtons(id, data as JToken);
        }

        [System.Web.Http.HttpPost]
        public void EditButton(dynamic data)
        {
            var service = new sys_menuService();
            var result = service.SaveButtons(data);
        }      

        //页面加载树形,显示所有Role(第一层为RoleID,没有第二层)
        public dynamic GetRoleTree(RequestWrapper request)
        {
            var service = new sys_menuService();
            ParamSP ps = new ParamSP().Name("usp_SYS_Base");
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetRoleTree");

            var RoleList = service.GetDynamicList(ps);
            return RoleList;

        }

        //單擊树節點,显示此Role对应的所有Menu
        public dynamic GetRoleMenuList(RequestWrapper request)
        {
            //首次加载树时request["RoleID"]=null;
            string RoleID = request["RoleID"] == null ? "" : request["RoleID"].ToString();
            var service = new sys_menuService();
            ParamSP ps = new ParamSP().Name("usp_SYS_Base");
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(request["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(request["rows"].ToString());
            ps.Parameter("ActionType", "GetRoleMenu");
            ps.Parameter("Role", RoleID);

            var RoleMenuList = service.GetDynamicListWithPaging(ps);
            return RoleMenuList;
        }

        [System.Web.Http.HttpPost]
        public string EditRoleMenu(dynamic data)  
        {
            var service = new sys_menuService();

            JObject TabData = JObject.Parse(data["list"].ToString());
            bool tabChange = (bool)TabData.GetValue("_changed");
            //string RoleID = data["RoleID"].ToString();

            //更新單身表格           
            if (tabChange)
            {
                foreach (var item in TabData)
                {
                    string itemKey = item.Key;//deleted,updated,inserted
                    if (item.Value.HasValues)
                    {
                        ParamSP ps1 = new ParamSP().Name("usp_SYS_Base");
                        ps1.Parameter("ActionType", "SaveRoleMenu");
                        ps1.Parameter("ActionItem", itemKey);

                        JArray ActionData = item.Value as JArray; 
                        foreach (var itemDetail in ActionData)
                        {
                            JObject dr = itemDetail as JObject;
                            foreach (var drField in dr)
                            {
                                ps1.Parameter(drField.Key, drField.Value.ToString());
                            }
                            service.StoredProcedureNoneQuery(ps1);
                        }
                    }
                }
            }

            return "OK";
        }

        public dynamic Getrolelist(RequestWrapper request)
        {
            var service = new sys_menuService();
            ParamSP ps = new ParamSP().Name("usp_SYS_Base");
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetRoleListForBox");
            psd.PagingCurrentPage = int.Parse(request["page"].ToString()); 
            psd.PagingItemsPerPage = int.Parse(request["rows"].ToString()); ;

            var RoleList = service.GetDynamicListWithPaging(ps);
            return RoleList;
        }

        [System.Web.Http.HttpPost]
        public string editroletype(dynamic data) 
        {
            string strUser = User.Identity.Name;
            var service = new sys_menuService();


            JObject TabData = JObject.Parse(data["list"].ToString());
            bool tabChange = (bool)TabData.GetValue("_changed");

            //更新單身表格           
            if (tabChange)
            {
                foreach (var item in TabData)
                {
                    string itemKey = item.Key;//deleted,updated,inserted
                    if (item.Value.HasValues)
                    {
                        ParamSP ps1 = new ParamSP().Name("usp_SYS_Base");
                        ps1.Parameter("ActionType", "SaveRole");
                        ps1.Parameter("ActionItem", itemKey);

                        JArray ActionData = item.Value as JArray;                 
                        foreach (var itemDetail in ActionData)
                        {
                            JObject dr = itemDetail as JObject;
                            
                            foreach (var drField in dr)
                            {
                                ps1.Parameter(drField.Key, drField.Value.ToString());
                                if (itemKey == "inserted") { ps1.Parameter("creator", strUser); }
                            }
                            service.StoredProcedureNoneQuery(ps1);
                        }
                    }
                }
            }

            return "OK";
        }
    }
}
