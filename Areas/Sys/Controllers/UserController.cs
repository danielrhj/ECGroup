
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zephyr.Core;
using ECGroup.Models;
using Zephyr.Utils;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Web.Mvc;

namespace ECGroup.Areas.Sys.Controllers
{
    public class UserController : Controller
    {
        SYS_UserInfoService userService = new SYS_UserInfoService();

        #region UserList.cshtml
        public ActionResult Index()
        {
            var model = new
            {
                dataSource = new
                {
                    AreaNameList = SYS_UserInfoService.GetAreaNameList(),
                    UsedFlagList = SYS_UserInfoService.GetYN()
                },
                roleid=new SYS_UserInfoService().GetUserModel("").Role,
                urls = new
                {
                    query = "/api/sys/user/",
                    add = "/api/sys/user/GetH", //注意此处不能用add开头的方法名，不论大小写都不行
                    edit = "/api/sys/user/edit"
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
                    UsedFlag = "1"
                },
                defaultRow = new
                {
                    UserAccount = "",
                    UserName = "",
                    UnionCode = "",
                    Tel = "",
                    Email = "",
                    AreaName = "LH",
                    Role = "User",
                    Is_valid = "1",Supervisor=""
                },
                setting = new
                {
                    idField = "UserAccount",
                    postListFields = new string[] { "UserAccount", "UserName", "UnionCode", "Tel", "Email", "AreaName", "Role", "Is_valid", "Supervisor" }
                }
            };

            return View(model);
        }

        #endregion

        public ActionResult ModifyUser()
        {
            SYS_UserInfo um = new SYS_UserInfoService().GetUserModel(this.User.Identity.Name);
            return View(um);
        }

        [System.Web.Http.HttpPost]
        public string UploadSignMap()
        {
            string Msg = "";
            string UserID = User.Identity.Name;
            string strFilesID = Request["UploadID"].ToString();
            string strFileName = Request.Files[strFilesID].FileName;
            int Size = Request.Files[strFilesID].ContentLength;
            if (strFileName.ToString().Trim().Equals(""))
            {
                Msg = "{'error':'請選擇要上傳的文件！'}";
            }
            string extName = strFileName.Substring(strFileName.LastIndexOf(".") + 1);
            string[] fileType={ "tif", "tiff", "png", "bmp", "gif", "jpg", "jpeg" }; 

            if (!fileType.Contains(extName))
            {
                Msg = "{'error':'請選擇圖片文件！'}";
            }
            string SaveName = UserID + "-" + DateTime.Now.ToString("yyyyMMddhhmmss");
            if (Size > 0)
            {
                string strSaveName = SaveName + "." + extName;
                string strPath = "~/FJLBS/SignatureMap/" + strSaveName;
                try
                {
                    Request.Files[strFilesID].SaveAs(System.Web.HttpContext.Current.Server.MapPath(strPath));
                    Msg = "{'NewFileName':'" + strSaveName + "'}"; //注意這裡的四個單引號都必須要
                    bool bk = new SYS_UserInfoService().updateSignMap(UserID, strSaveName);

                    return Msg;
                }
                catch (Exception ex)
                {
                    Msg = "{'error':'" + ex.Message + "'}";
                }
            }
            return Msg;
        }
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
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "GetUserList");
            ps.Parameter("Account", query["UserAccount"].ToString());
            ps.Parameter("UserName", query["UserName"].ToString());
            ps.Parameter("Role", query["Role"].ToString());
            ps.Parameter("is_valid", query["UsedFlag"].ToString());

            var UserListNew = userService.GetDynamicListWithPaging(ps);
            return UserListNew;
        }

        [System.Web.Http.HttpPost]
        public dynamic Edit(dynamic data)  //新增和修改已有資料時使用的方法
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

                        JArray ActionData = item.Value as JArray; //注意這個數組的每個元素對應表格的一行編輯的記錄
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
            //按原有条件重新查詢資料
            return "OK"; //GetUserList(data.form);

        }

        [System.Web.Http.HttpPost]
        public dynamic getSaveUser(dynamic data)
        {
            SYS_UserInfo um = new SYS_UserInfo();
            um.UserAccount = this.User.Identity.Name;
            um.UserName = data["UserName"].ToString();
            um.Tel = data["Tel"].ToString();
            um.Email = data["Email"].ToString();
            um.Role = data["Role"].ToString();
            um.is_valid =byte.Parse(data["Is_valid"].ToString());   //已轉換為1、0
            um.Department = data["Department"].ToString();
            um.SignatureLine = data["SignatureLine"].ToString();

            bool result = false;
            result = userService.ModifyUserByModel(um);
            return new {success=result };
        }

        //// 其实这个方法多余
        public string GetH()
        {
            return "0";
        }

        public List<dynamic> getRoleList(string q)
        {
            var RoleList = SYS_UserInfoService.GetRoleList(q);
            return RoleList;
        }

        public List<dynamic> GetSiteList()   
        {
            var RoleList = SYS_UserInfoService.GetAreaNameList();
            return RoleList;
        }

        public List<dynamic> getUserCodeList(string q)    
        {
            var RoleList = SYS_UserInfoService.GetUserCodeList(q);
            return RoleList;
        }

        public List<dynamic> GetRoleListForInsert(string q)    //注意这里的参数名称q不能改，否则就找不到这个方法了
        {
            var RoleList = SYS_UserInfoService.GetRoleListForInsert(q);
            return RoleList;
        }

        [System.Web.Http.HttpPost]
        public void EditPermission(string id, dynamic data)
        {
            var service = new sys_menuService();            
            service.EditUserMenuButton(id, data.buttons as JToken);
        }
     
    }
}