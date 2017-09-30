using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Zephyr.Core;
using Zephyr.Utils;
using ECGroup.Models;
using ECGroup.Areas.Mms.Common;
using System.Web;
using System.IO;

namespace ECGroup.Areas.Sys.Controllers
{
    public class RequestController : Controller
    {
        //
        // GET: /Sys/Request/

        public ActionResult Index()
        {
            string userRole = new SYS_UserInfoService().GetUserModel(User.Identity.Name).Role.ToString().ToLower();
            var model = new
            {
                dataSource = new
                {
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                urls = new
                {
                    query = "/api/sys/Request/Get",
                    edit = "/sys/Request/edit/",
                    remove = "/api/sys/Request/GetDelete/",
                    userinfo = "/api/sys/Request/getUserInfo/",
                },
                resx = new
                {
                    noneSelect = "请先选择一条数据！",
                    editSuccess = "保存成功！",
                    deleteSuccess="刪除成功！"
                },
                form = new
                {
                    Creator=(userRole=="superadmin"?"":User.Identity.Name),
                    BeginDate = DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd") + " 到 " + DateTime.Today.ToString("yyyy-MM-dd") 
                },
                idField = "AutoID"
            };
            return View(model);
        }

        public ActionResult Edit(string id)
        {
            var model = new
            {
                urls = new
                {
                    getdata = "/api/sys/Request/GetRequestInfo/",
                    edit = "/api/sys/Request/Edit/",
                    remove = "/api/sys/Request/Delete/",
                    sign = "/api/sys/Request/EditSign/",
                    query = "/api/sys/Request/GetAttach/"
                },
                resx = new
                {
                    editSuccess = "保存成功！"
                },

                dataSource = new
                {
                    pageData=new RequestApiController().GetRequestInfo(id),
                    TaskGradeList = new Sys_RequestService().GetTaskGrade(),
                    CatList=new Sys_RequestService().GetRequestTypeList(),
                    RejectList = MmsHelper.GetYON(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                form = new
                {
                    defaults = new Sys_Request().Extend(new { Subject = "", CatID = "", TaskGrade = "一般", Creator = "", CreateDate = "", Request = "", ITPerson = "", CloseDate = "", ITComments = "", RejectFlag="N"})
                },
                setting = new
                {
                    idField = "AutoID",
                    role=new SYS_UserInfoService().GetUserModel(User.Identity.Name).Role.ToString().ToLower(),
                    postListFields = new string[] { "AutoID","CatID","Subject","Request","ITPerson","ITComments","CloseDate","RejectFlag","Creator","TaskGrade" }
                }
            };

            return View(model);
        }
    }

    public class RequestApiController : ApiController
    {
        Sys_RequestService Requestservice = new Sys_RequestService();

        public dynamic Get(RequestWrapper query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");

            ParamSP ps = new ParamSP().Name("ups_Sys_Request");
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "1000" : query["rows"].ToString());
            ps.Parameter("ActionType", "GetRequestList");
            ps.Parameter("Creator", query["Creator"].ToString());
            ps.Parameter("BeginDate", sendDate1.Replace("-", ""));
            ps.Parameter("EndDate", sendDate2.Replace("-", ""));

            var RequestList = Requestservice.GetDynamicListWithPaging(ps, "AutoID");
            return RequestList;
        }

        public dynamic GetRequestInfo(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                ParamSP ps = new ParamSP().Name("ups_Sys_Request");
                ps.Parameter("ActionType", "GetRequestInfo");
                ps.Parameter("AutoID", id);

                var RequestNew = Requestservice.GetDynamicListForDataSet(ps);
                var result = new
                {
                    //主表数据,相當於DataSet的Tables[0].Rows[0]
                    form = RequestNew[0][0]
                };
                return result;
            }
            else
            {
                var result = new
                {
                    form = new Sys_Request().Extend(new
                    {
                        AutoID="0",
                        Subject = "",
                        CatID = "",
                        TaskGrade = "一般",
                        Creator = FormsAuth.GetUserData().UserCode,
                        CreateDate = ZDateTime.GetDate(),
                        Request = "",
                        ITPerson = "",
                        CloseDate = "",
                        AcceptDate = "",
                        ITComments = ""
                    })
                };
                return result;
            }
        }

        [System.Web.Http.HttpPost]
        public dynamic PostFile()
        {
            // 设置上传目录
            // var provider = new MultipartFormDataStreamProvider(@"D:\");
            // 接收数据，并保存文件
            // Request.Content.ReadAsMultipartAsync(provider);

            try
            {
                string originalFileName = "", fileType = "",newFileName="",RequestID="";
                var context = HttpContext.Current;
                var request = context.Request;
                RequestID = request["autoID"].ToString();
                string uploadPath = HttpContext.Current.Server.MapPath("~/FJLBS/Request/");
                if (!Directory.Exists(uploadPath))
                { Directory.CreateDirectory(uploadPath); }

                //保存文件
                var postFile = request.Files[0];

                originalFileName = postFile.FileName.Substring(postFile.FileName.LastIndexOf("\\") + 1);
                fileType = originalFileName.Substring(originalFileName.LastIndexOf("."));                
                newFileName =  DateTime.Today.ToString("yyMMdd") + "-" + Guid.NewGuid().ToString() + fileType;

                request.Files[0].SaveAs(uploadPath+newFileName);

                Requestservice.InsertFileRecord(new { RequestID = RequestID, UserCode = User.Identity.Name, OldFileName = MmsHelper.HZ(originalFileName, 0), FileID = newFileName });
                
            }
            catch (Exception e)
            {
                return new { error = e.Message, preventRetry = true };
            }

            //返回前台
            return new { success = true, message = "上傳成功!" };
        }

        public dynamic GetAttach(RequestWrapper query)
        {
            string id = query["AutoID"].ToString();
            if (!string.IsNullOrEmpty(id))
            {
                ParamSP ps = new ParamSP().Name("ups_Sys_Request");
                ps.Parameter("ActionType", "GetAttachInfo");
                ps.Parameter("AutoID", id);

                var RequestNew = Requestservice.GetDynamicList(ps);
                return RequestNew;
            }
            else
            { return new List<dynamic>(); }
        }

        [System.Web.Http.HttpPost]
        public void GetDelete(dynamic data)
        {
            ParamSP ps = new ParamSP().Name("ups_Sys_Request");
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "DeleteRequest");
            ps.Parameter("AutoID", data["value"].ToString());
            bool bk = Requestservice.StoredProcedureNoneQuery(ps);
        }

        [System.Web.Http.HttpPost]
        public dynamic Edit(dynamic data)
        {
            JObject gridData = JObject.Parse(data["form"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name("ups_Sys_Request");
            if (gridChange)
            {
                ps.Parameter("ActionType", "SaveRequest");
                gridData["Request"] = MmsHelper.HZ(gridData["Request"].ToString(), 1);
                gridData["Subject"] = MmsHelper.HZ(gridData["Subject"].ToString(), 1);
                gridData["ITComments"] = MmsHelper.HZ(gridData["ITComments"].ToString(), 1);
                
                foreach (var item in gridData)
                {
                    ps.Parameter(item.Key, item.Value.ToString().Trim());
                }
                Requestservice.StoredProcedureNoneQuery(ps);
            }
            return GetRequestInfo(data.AutoID);
        }
        [System.Web.Http.HttpPost]
        public bool EditSign(dynamic query)
        {
            ParamSP ps = new ParamSP().Name("ups_Sys_Request");
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", query["ActionName"].ToString());
            ps.Parameter("ITComments", query["ITComments"].ToString());
            ps.Parameter("AutoId", query["AutoId"].ToString());
            ps.Parameter("ITPerson", User.Identity.Name.ToString());
            var result = Requestservice.StoredProcedureNoneQuery(ps);   
            return result;
        }

        public List<dynamic> getUserInfo(RequestWrapper query)
        {
            string UserCode = query["key"].ToString();
            var signHis = Requestservice.GetUserInfo(UserCode);
            return signHis;
        }

        [System.Web.Http.HttpDelete]
        public dynamic Delete(dynamic data)
        {
            string filename = data.fileid.ToString();
            string uploadPath = HttpContext.Current.Server.MapPath("~/FJLBS/Request/");
            ParamSP ps = new ParamSP().Name("ups_Sys_Request");
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "deleteFile");
            ps.Parameter("AutoID",data.autoID.ToString());

            try
            {
                bool bk = Requestservice.StoredProcedureNoneQuery(ps);

                if (bk)
                { File.Delete(uploadPath + filename); }

                return new { succcess = bk,msg=bk?"":"刪除失敗." };

            }
            catch (Exception ex)
            { return new { succcess = false, msg = ex.Message }; }

        }
    }
}
