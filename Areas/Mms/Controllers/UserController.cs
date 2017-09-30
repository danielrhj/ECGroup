using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zephyr.Core;
using ECGroup.Models;
using Zephyr.Utils;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ECGroup.Areas.Mms.Common;
using System.Web.Mvc;

namespace ECGroup.Areas.Mms.Controllers
{
    public class UserController : Controller
    {
        //BLL_User blluser = new BLL_User();
        //BLL_DLL bll = new BLL_DLL();

        #region UserList.cshtml
        public ActionResult Index()
        {
            var model = new
            {
                dataSource = new
                {
                    AreaNameList = SYS_UserInfoService.GetAreaNameList(),
                    UsedFlagList = MmsHelper.GetYN()
                },
                urls = new
                {
                    query = "/api/mms/user",
                    add = "/api/mms/user/GetH", 
                    edit = "/api/mms/user/edit"
                },
                resx = new
                {
                    noneSelect = "请先选择一条数据！",
                    editSuccess = "保存成功！",
                    auditSuccess = "单据已审核！"
                },
                form = new
                {
                    UserAccount = "",
                    UserName = "",                   
                    Role = "",
                    Is_valid = "1"
                },
                defaultRow = new
                {
                    UserAccount="",
                    UserName="",
                    UnionCode="",
                    Tel="",
                    Email="",
                    AreaName="LH",
                    Role="User",
                    Is_valid="1"
                },
                setting = new
                {
                    idField = "UserAccount",
                    postListFields = new string[] { "UserAccount", "UserName", "UnionCode", "Tel", "Email", "AreaName", "Role", "Is_valid" }
                }
            };

            return View(model);
        }

        public List<dynamic> getRoleList(string q)    
        {
            var RoleList = SYS_UserInfoService.GetRoleList(q);
            return RoleList;
        }

        #endregion   
    }

    public class UserApiController : ApiController
    {        
        SYS_UserInfoService userService = new SYS_UserInfoService();
        // GET api/user

        //默認獲取資料的方法,BABG.cshtml页面获取列表资料        
        public dynamic Get(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name("usp_UserAccount");
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "1000" : query["rows"].ToString());
            ps.Parameter("ActionType", "GetUserList");
            ps.Parameter("Account", query["UserAccount"].ToString());
            ps.Parameter("UserName", query["UserName"].ToString());
            ps.Parameter("Role", query["Role"].ToString());
            ps.Parameter("is_valid", query["is_valid"].ToString());

            var UserListNew = userService.GetDynamicListWithPaging(ps);
            return UserListNew;
        }

        [System.Web.Http.HttpPost]
        public dynamic Edit(dynamic data)  
        {
            JObject gridData = JObject.Parse(data["list"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name("usp_UserAccount");

            if (gridChange)
            {
                ps.Parameter("ActionType", "SaveUser");

                foreach (var item in gridData)
                {
                    string itemKey = item.Key;//deleted,updated,inserted
                    if (item.Value.HasValues)
                    {
                        ps.Parameter("ActionItem", itemKey);

                        JArray ActionData = item.Value as JArray; 
                        foreach (var itemDetail in ActionData)
                        {
                            JObject dr = itemDetail as JObject;
                            foreach (var drField in dr)
                            {
                                ps.Parameter(drField.Key, drField.Value.ToString());
                            }
                            userService.StoredProcedureNoneQuery(ps);
                        }
                    }
                }
                
            }
            //更新單身表格      
            #region
            
            #endregion

            //按原有条件重新查詢資料
            return "OK";

        }

        //// 其实这个方法多余
        public string GetH()
        {
            return "0";
        }
       

        [System.Web.Http.HttpPost]
        public List<dynamic> GetSiteList()
        {
            var siteList = SYS_UserInfoService.GetAreaNameList();
            return siteList;
        }

        
        public List<dynamic> getRoleList(string q)   
        {
            var RoleList = SYS_UserInfoService.GetRoleList(q);
            return RoleList;
        }

        public List<dynamic> GetRoleListForInsert(string q)   
        {
            var RoleList = SYS_UserInfoService.GetRoleListForInsert(q);
            return RoleList;
        }

        public dynamic GetUserList(dynamic form)
        {
            ParamSP ps = new ParamSP().Name("usp_UserAccount");
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = 1;
            psd.PagingItemsPerPage = 20; 
            ps.Parameter("ActionType", "GetUserList");
            ps.Parameter("Account", form["UserAccount"].ToString());
            ps.Parameter("UserName", form["UserName"].ToString());
            ps.Parameter("Role", form["Role"].ToString());
            ps.Parameter("is_valid", form["Is_valid"].ToString());

            var UserListNew = userService.GetDynamicListWithPaging(ps);
            return UserListNew;            
        }
       
    }
}
