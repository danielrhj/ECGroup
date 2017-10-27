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
    public class PartNoController : Controller
    {
        string AA = "";
        public ActionResult Index()
        {
            AA = "Test";
            var model = new
            {
                dataSource = new
                {
                    YONList=MmsHelper.GetYON(),
                    TypeCodeList0 = MaterialTypeService.getTypeCodeList(false),
                    TypeCodeList1 = MaterialTypeService.getTypeCodeList(true),
                    SupplierCodeList=SupplierService.getSupplierCodeList(""),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                urls = new
                {
                    query = "/api/mms/PartNo/Get",
                    add = "/api/mms/PartNo/GetH",
                    edit = "/api/mms/PartNo/edit",
                    excel = "/api/mms/PartNo/GetcreateExcel/"
                },
                resx = new
                {
                    noneSelect = "请先选择一条数据！",
                    editSuccess = "保存成功！"
                },
                form = new
                {
                    SuppPN = "",
                    CDesc = "",
                    CustPN = "",
                    Supplier = "",
                    TypeCode=""
                },
                defaultRow = new
                {
                    PNID="0", 
                    SuppPN = "",
                    CDesc = "",CSpec="",
                    Brand = "",
                    TypeCode = "M001",
                    HotSale="N",
                    LeadTime = "1",
                    ProxyCount = "0",
                    CustPNCount = "0",
                    Attach = "0"
                },
                setting = new
                {
                    idField = "PNID",
                    postListFields = new string[] { "PNID", "SuppPN", "CDesc", "CSpec", "Brand", "TypeCode", "HotSale", "LeadTime"}
                }
            };
            return View(model);
        }

        public ActionResult EPN()
        {
            string BB = AA;
            var model = new
            {
                dataSource = new
                {
                    CNCYList = MmsHelper.GetCurrencyType(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                }
            };
            return View(model);
        }
       
    }

    public class PartNoApiController : ApiController
    {
        PartNoService partnoService = new PartNoService();
        public dynamic Get(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name(PartNoService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "200" : query["rows"].ToString());
            ps.Parameter("ActionType", "GetPartNoList");
            ps.Parameter("SuppPN", query["SuppPN"].ToString());
            ps.Parameter("CDesc", query["CDesc"].ToString());
            ps.Parameter("CustPN", query["CustPN"].ToString());
            ps.Parameter("Supplier", query["Supplier"].ToString());
            ps.Parameter("TypeCode", query["TypeCode"].ToString());
            var ColleteeList = partnoService.GetDynamicListWithPaging(ps);
            return ColleteeList;
            //SuppPN,CDesc,CustPN,VendorName
        }

        public List<dynamic> GetEPNAll()
        {
            var RoleList = PartNoService.getEPNList();
            return RoleList;
        }

        public dynamic GetPartNoRelated(RequestWrapper query)
        {
            string grid = query["grid"].ToString();
            string ActionType = grid == "proxy" ? "GetPartNoProxy" : "GetPartNoCustPN";
            ParamSP ps = new ParamSP().Name(PartNoService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", ActionType);
            ps.Parameter("PNID", query["PNID"].ToString());
            var ColleteeList = partnoService.GetDynamicListWithPaging(ps);
            return ColleteeList;
        }

        [System.Web.Http.HttpPost]
        public dynamic Edit(dynamic data)
        {
            JObject gridData = JObject.Parse(data["list"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name(PartNoService.strSP);
            if (gridChange)
            {
                ps.Parameter("ActionType", "SavePartNo");

                foreach (var item in gridData)
                {
                    string itemKey = item.Key;//deleted,updated,inserted
                    if (item.Value.HasValues)
                    {
                      
                        ps.Parameter("ActionItem", itemKey);
                        JArray ActionData = item.Value as JArray; 
                        foreach (var itemDetail in ActionData)
                        {
                            JObject dr = itemDetail as JObject; //SuppPN, CDesc, CSpec,Brand
                            dr["SuppPN"] = MmsHelper.HZ(dr["SuppPN"].ToString(), 1);
                            dr["CDesc"] = MmsHelper.HZ(dr["CDesc"].ToString(), 1);
                            dr["CSpec"] = MmsHelper.HZ(dr["CSpec"].ToString(), 1);
                            dr["Brand"] = MmsHelper.HZ(dr["Brand"].ToString(), 1);

                            foreach (var drField in dr)
                            {
                                ps.Parameter(drField.Key, drField.Value.ToString());
                            }
                            partnoService.StoredProcedureNoneQuery(ps);
                        }
                    }
                }
            }
            return "OK";
        }

        [System.Web.Http.HttpPost]
        public dynamic EditPN(dynamic data)
        {
            JObject gridData = JObject.Parse(data["list"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name(PartNoService.strSP);
            if (gridChange)
            {
                ps.Parameter("ActionType", "SaveEcomPN");
                ps.Parameter("Creator", User.Identity.Name);
                foreach (var item in gridData)
                {
                    string itemKey = item.Key;//deleted,updated,inserted
                    if (item.Value.HasValues)
                    {                      
                        ps.Parameter("ActionItem", itemKey);
                        JArray ActionData = item.Value as JArray; 
                        foreach (var itemDetail in ActionData)
                        {
                            JObject dr = itemDetail as JObject; //PN,PNID,ParentCode,CDesc,Qty,Price,Currency,OrderBy
                            dr["PN"] = MmsHelper.HZ(dr["PN"].ToString(), 1);
                            dr["CDesc"] = MmsHelper.HZ(dr["CDesc"].ToString(), 1);

                            foreach (var drField in dr)
                            {
                                ps.Parameter(drField.Key, drField.Value.ToString());
                            }
                            partnoService.StoredProcedureNoneQuery(ps);
                        }
                    }
                }
            }
            return "OK";
        }        

        [System.Web.Http.HttpPost]
        public dynamic EditpartnoRelated(dynamic data)
        {
            string save = data["save"].ToString();
            string ActionType = save == "proxy" ? "SavePartnoProxy" : "SavePartNoCustPN";
            JObject gridData = JObject.Parse(data["list"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name(PartNoService.strSP);
            if (gridChange)
            {
                ps.Parameter("ActionType", ActionType);

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
                                ps.Parameter(drField.Key, drField.Value.ToString().Trim());
                            }
                            partnoService.StoredProcedureNoneQuery(ps);
                        }
                    }
                }
            }
            return "OK";
        }

        public string GetH()
        {
            return "0";
        }

        public List<dynamic> getPayeeList(string q)    
        {
            var RoleList = new CustomerApiController().getPayeeList(q);
            return RoleList;
        }

        public List<dynamic> getIdeasSupplierCodeList(string q)    
        {
            var RoleList = SupplierService.getIdeasSupplierCodeList(q);
            return RoleList;
        }

        public List<dynamic> getSuppPNList(string q)    
        {
            var RoleList = PartNoService.getSuppPNList(q);
            return RoleList;
        }

        public List<dynamic> getCustPNList(string q)
        {
            var RoleList = PartNoService.getCustPNList(q);
            return RoleList;
        }

        [System.Web.Http.HttpPost]
        public dynamic getPNInfoByPNID(dynamic data)
        {
            string pnid=data["pnid"].ToString();
            var pn = PartNoService.getPNInfoByPNID(pnid);
            return pn;
        }

        [System.Web.Http.HttpPost]
        public dynamic getPNListBySuppCode(dynamic data)
        {
            string suppcode = data["suppcode"].ToString();
            var pn = PartNoService.getPNListBySuppCode(suppcode);
            return pn;
        }
        

        [System.Web.Http.HttpPost]
        public dynamic GetcreateExcel(dynamic query)
        {
            ParamSP ps = new ParamSP().Name(PartNoService.strSP);
            ps.Parameter("ActionType", "GetPartNoList");
            ps.Parameter("SuppPN", query["SuppPN"].ToString());
            ps.Parameter("CDesc", query["CDesc"].ToString());
            ps.Parameter("CustPN", query["CustPN"].ToString());
            ps.Parameter("Supplier", query["Supplier"].ToString());
            ps.Parameter("TypeCode", query["TypeCode"].ToString());

            try
            {
                string fileurl = PartNoService.ExportExcel(ps);
                return new { success = true, Msg = "", url = fileurl };
            }
            catch (Exception ex)
            {
                string AA = ex.Message;
                return new { success = false, Msg = ex.Message, url = "" };
            }
        }
        
    }
}
