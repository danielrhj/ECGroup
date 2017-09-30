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
using System.Web;

namespace ECGroup.Areas.Mms.Controllers
{
    public class SupplierController : Controller
    {
        //
        // GET: /Mms/Supplier/

        public ActionResult Index()
        {
            var model = new
            {
                dataSource = new
                {
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                urls = new
                {
                    query = "/api/mms/Supplier/Get",
                    add = "/api/mms/Supplier/GetH", 
                    edit = "/mms/Supplier/edit/",
                    remove = "/api/mms/Supplier/GetDelete/",
                    excel = "/api/mms/Supplier/GetcreateExcel/"
                },
                resx = new
                {
                    noneSelect = "请先选择一条数据！",
                    deleteSuccess="删除成功",
                    editSuccess = "保存成功！"
                },
                form = new
                {
                    SupplierInfo = ""
                },
                defaultRow = new
                {
                    SuppID="0",
                    SuppAbbr="",
                    SuppCode = "",
                    SuppName = "",
                    SuppAdd = "",
                    Contact = "",
                    Tel = "",
                    CellNo = "",
                    Email = "",
                    SWIFICode = "",
                    AccountName = "",
                    AccountNo = "",
                    Currency = "",
                    PayTerms = "",
                    BankName = ""
                },
                setting = new
                {
                    idField = "SuppID",
                    postListFields = new string[] { "SuppID","SuppAbbr","SuppCode","SuppName","SuppAdd","Contact","Tel","CellNo","Email","SWIFICode","AccountName","AccountNo","Currency","PayTerms","BankName" }
                }
            };
            return View(model);
        }

        public ActionResult Edit(string id = "")
        {

            var model = new
            {
                urls = new
                {
                    edit = "/api/mms/supplier/Edit/",
                    getdata = "/api/mms/supplier/GetPageData/"
                },
                resx = new
                {
                    editSuccess = "保存成功！"
                },
                dataSource = new
                {
                    pageData = new SupplierApiController().GetPageData(id),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                form = new
                {
                    defaults = new Supplier().Extend(new
                    {
                        SuppAbbr = "",
                        SuppCode = "",
                        SuppName = "",
                        SuppAdd = "",
                        Contact = "",
                        Tel = "",
                        CellNo = "",
                        Email = "",
                        SWIFICode = "",
                        AccountName = "",
                        AccountNo = "",
                        Currency = "",
                        BankName = "",
                        PayTerms = "",
                        SuppID = "0"
                    }),
                    primaryKeys = new string[] { "SuppID" },
                    idField = id
                    // CustID,CustAbbr,CustCode,CustName,CustAdd,Contact,Tel,CellNo,Email,SWIFICode,AccountName,AccountNo,Currency,PayTerms,BankName
                }
            };
            return View(model);
        }
       
    }

    public class SupplierApiController : ApiController
    {
        SupplierService supplierService = new SupplierService();
        public dynamic Get(RequestWrapper query)
        {
            object supplierList = "";
            bool noSizeLimit = query["noSizeLimit"] == null ? false : bool.Parse(query["noSizeLimit"]);
            ParamSP ps = new ParamSP().Name(SupplierService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "GetSupplierList");
            ps.Parameter("SuppCode", query["SupplierInfo"].ToString().Trim());

            if (noSizeLimit) //excel
            {
                supplierList = supplierService.GetDynamicList(ps);
            }
            else
            {
                supplierList = supplierService.GetDynamicListWithPaging(ps, "SuppID");
            }
            return supplierList;
            
        }

        public dynamic GetPageData(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                ParamSP ps = new ParamSP().Name(SupplierService.strSP);
                ps.Parameter("ActionType", "GetSupplierDataBySuppID");
                ps.Parameter("SuppID", id);

                var payeeListNew = supplierService.GetDynamicListForDataSet(ps);
                var result = new
                {
                    form = payeeListNew[0][0]
                };
                return result;
            }
            else
            {
                var result = new
                {
                    form = new Supplier().Extend(new
                    {
                        SuppAbbr = "",
                        SuppCode = "",
                        SuppName = "",
                        SuppAdd = "",
                        Contact = "",
                        Tel = "",
                        CellNo = "",
                        Email = "",
                        SWIFICode = "",
                        AccountName = "",
                        AccountNo = "",
                        Currency = "",
                        BankName = "",
                        PayTerms = "",
                        SuppID = "0"
                    })
                };

                return result;
            }
        }

        public dynamic GetSupplierContact(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name(SupplierService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "GetSupplierContact");
            ps.Parameter("VENDER_CODE", query["VendorCode"].ToString());
            var ColleteeList = supplierService.GetDynamicListWithPaging(ps);
            return ColleteeList;
        }

        [System.Web.Http.HttpPost]
        public dynamic Edit(dynamic data)
        {
             JObject formData = JObject.Parse(data["form"].ToString());
            bool formChange = (bool)formData.GetValue("_changed");

            string SuppID = formData["SuppID"].ToString();
            ParamSP ps = new ParamSP().Name(CustomerService.strSP);
            //更新單頭
            if (formChange)
            {
                ps.Parameter("ActionType", "SaveSupplier");

                formData["SuppAbbr"] = MmsHelper.HZ(formData["SuppAbbr"].ToString(), 1);
                formData["SuppName"] = MmsHelper.HZ(formData["SuppName"].ToString(), 1);
                formData["SuppAdd"] = MmsHelper.HZ(formData["SuppAdd"].ToString(), 1);
                formData["AccountName"] = MmsHelper.HZ(formData["AccountName"].ToString(), 1);
                formData["BankName"] = MmsHelper.HZ(formData["BankName"].ToString(), 1);
                formData.Remove("_changed");
                foreach (var item in formData)
                {
                    ps.Parameter(item.Key, item.Value.ToString().Trim());
                }

                if (int.Parse(SuppID) > 0)
                { supplierService.StoredProcedureNoneQuery(ps); }
                else
                { SuppID = supplierService.StoredProcedureScalar(ps); }
            }
            return GetPageData(SuppID);
        }

        [System.Web.Http.HttpPost]
        public dynamic EditSupplierContact(dynamic data)
        {
            JObject gridData = JObject.Parse(data["list"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name(SupplierService.strSP);
            if (gridChange)
            {
                ps.Parameter("ActionType", "SaveSupplierContact");

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
                            supplierService.StoredProcedureNoneQuery(ps);
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

        [System.Web.Http.HttpPost]
        public void GetDelete(dynamic data)
        {
            ParamSP ps = new ParamSP().Name(SupplierService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "DeleteSupplier");
            ps.Parameter("SuppID", HttpUtility.UrlDecode(data["value"].ToString()));
            bool bk = supplierService.StoredProcedureNoneQuery(ps);
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

        public List<dynamic> getSupplierCodeList(string q)    
        {
            var RoleList = SupplierService.getSupplierCodeList(q);
            return RoleList;
        }

        public List<dynamic> getSupplierAbbr(string q)  //autocomplete数据源
        {
            var RoleList = SupplierService.getSupplierAbbr(q);
            return RoleList;
        }

        [System.Web.Http.HttpPost]
        public dynamic GetcreateExcel(dynamic query)
        {
            ParamSP ps = new ParamSP().Name(SupplierService.strSP);
            ps.Parameter("ActionType", "GetSupplierList");
            ps.Parameter("SuppCode", query["SupplierInfo"].ToString().Trim());

            try
            {
                string fileurl = SupplierService.ExportExcel(ps);
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
