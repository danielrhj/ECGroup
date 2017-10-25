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
    public class MaterialTypeController : Controller
    {
        //

        public ActionResult Index()
        {
            var model = new
            {                
                urls = new
                {
                    query = "/api/mms/MaterialType/Get",
                    edit = "/api/mms/MaterialType/Edit",
                    remove = "/api/mms/MaterialType/Delete/",
                    add = "/api/mms/MaterialType/GetH" 
                },
                dataSource = new
                {                   
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                resx = new
                {
                    editSuccess="保存成功！",
                    deleteSuccess="數據已刪除！",
                    noneSelect = "請選擇一條數據！"
                },
                idField = "AutoID",
                form = new
                {
                    TypeName = ""
                },
                defaultRow = new 
                {
                    AutoID = "0",
                    TypeCode = "",
                    TypeName = ""
                },
                setting = new
                {
                    idField = "AutoID",
                    postListFields = new string[] {"AutoID","TypeCode","TypeName"}
                }
            };
            return View(model);
        }

    }

    public class MaterialTypeApiController : ApiController
    {
        MaterialTypeService mService = new MaterialTypeService();
        public dynamic Get(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name(MaterialTypeService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "GetMaterialTypeList");
            ps.Parameter("TypeName", query["TypeName"].ToString());
            var BusInessTypeList = mService.GetDynamicListWithPaging(ps);
            return BusInessTypeList;
        }

        public dynamic getBrand(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name(MaterialTypeService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "GetBrandList");
            ps.Parameter("Brand", query["Brand"].ToString());
            var BusInessTypeList = mService.GetDynamicListWithPaging(ps);
            return BusInessTypeList;
        }

        [System.Web.Http.HttpPost]
        public dynamic Edit(dynamic data)
        {
            JObject gridData = JObject.Parse(data["list"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name(MaterialTypeService.strSP);
            if (gridChange)
            {
                ps.Parameter("ActionType", "SaveMaterialType");

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
                            if (itemKey == "deleted")
                            { ps.Parameter("AutoID", itemDetail["AutoID"].ToString()); }
                            else
                            {
                                ps.Parameter("TypeCode", itemDetail["TypeCode"].ToString());
                                ps.Parameter("TypeName", itemDetail["TypeName"].ToString());
                            }
                            mService.StoredProcedureNoneQuery(ps);
                        }
                    }
                }
            }
            return "OK";
        }

        public void Delete(string id)
        {
            ParamSP ps = new ParamSP().Name(MaterialTypeService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "deletedMaterialType");
            ps.Parameter("SuppID", id);
            bool bk = mService.StoredProcedureNoneQuery(ps);
        }

        [System.Web.Http.HttpPost]
        public dynamic editBrand(dynamic data)
        {
            JObject gridData = JObject.Parse(data["list"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name(MaterialTypeService.strSP);
            if (gridChange)
            {
                ps.Parameter("ActionType", "SaveBrand");

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

                            ps.Parameter("AutoID", itemDetail["AutoID"].ToString());
                            ps.Parameter("Brand", itemDetail["Brand"].ToString().Trim());
                            ps.Parameter("Remarks", itemDetail["Remark"].ToString().Trim());

                            mService.StoredProcedureNoneQuery(ps);
                        }
                    }
                }
            }
            return "OK";
        }

        public List<dynamic> getTypeCodeList(string q)    
        {
            var RoleList = MaterialTypeService.getTypeCodeList(false);
            return RoleList;
        }

        public List<dynamic> getBrandList(string q)
        {
            var RoleList = MaterialTypeService.getBrandList(false);
            return RoleList;
        }

        public string GetH()
        {
            ParamSP ps = new ParamSP().Name(MaterialTypeService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetNewTypeCode");
            string AA = mService.StoredProcedureScalar(ps);
            return AA;
        }
    
    }
}
