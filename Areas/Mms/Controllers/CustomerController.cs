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
using System.IO;

namespace ECGroup.Areas.Mms.Controllers
{
    public class CustomerController : Controller
    {
        //
        // GET: /Mms/Customer/

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
                    query = "/api/mms/customer/Get",
                    add = "/api/mms/customer/GetH",
                    edit = "/mms/customer/Edit/",
                    remove = "/api/mms/customer/GetDelete/",
                    excel = "/api/mms/customer/GetcreateExcel/",
                    queryCustPN = "/api/mms/customer/GetCustPN/"
                    
                },
                idField = "CustID",
                resx = MmsHelper.GetIndexResx("CustCode"),
                form = new
                {
                    CustomerInfo = ""
                },
                defaultRow = new
                {
                    CustID="0",
                    CustAbbr="",
                    CustCode="",
                    CustName="",
                    CustAdd="",
                    Contact="",
                    Tel="",
                    CellNo="",
                    Email = "",
                    SWIFICode = "",
                    AccountName = "",
                    AccountNo = "",
                    Currency = "",
                    PayTerms="",
                    BankName = ""
                },
                setting = new
                {
                    idField = "CustID",
                    postListFields = new string[] { "CustID","CustAbbr","CustCode","CustName","CustAdd","Contact","Tel","CellNo","Email","SWIFICode","AccountName","AccountNo","Currency","PayTerms","BankName" }
                }
            };
            return View(model);
        }

        public ActionResult relate()
        {
            var model = new
            {
                dataSource = new
                {
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew(),
                    CustCodeList1=CustomerService.getCustomerCodeListForCombobox(),
                    SuppCodeList1=SupplierService.getSupplierCodeList("")
                },
                urls = new
                {
                    query = "/api/mms/customer/GetCustPN/",
                    //add = "/api/mms/customer/GetH",
                    //edit = "/mms/customer/Edit/",
                    //remove = "/api/mms/customer/GetDelete/",
                    excel = "/api/mms/customer/GetcreateExcelRelate/"
                   

                },
                idField = "AutoID",
                resx = MmsHelper.GetIndexResx("CustCode"),
                form = new
                {
                    SuppPN="",
                    CustCode = "",
                    CustPN = "",
                    SuppCode = ""
                },
                defaultRow = new
                {                  
                },
                setting = new
                {
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
                    edit = "/api/mms/customer/Edit/",
                    getdata = "/api/mms/customer/GetPageData/",
                    getPNInfo="/api/mms/partno/getPNInfoByPNID",    //根據PNID獲取品名規格
                    getPNList = "/api/mms/partno/getPNListBySuppCode", //根據選擇的供應商代碼獲取其已關聯的料號清單                    
                    queryCustPN = "/api/mms/customer/GetCustPN/"
                },
                resx = new
                {
                    editSuccess = "保存成功！"
                },
                dataSource = new
                {
                    pageData = new CustomerApiController().GetPageData(id),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew(),
                    suppCodeList = SupplierService.getSupplierCodeList("0"),
                    SuppPNList0 = SupplierService.getSuppPNListForCombo(false)
                },
                form = new
                {
                    defaults = new Customer().Extend(new {  CustAbbr = "", 
                                                            CustCode = "", 
                                                            CustName = "", 
                                                            CustAdd = "", 
                                                            Contact = "", 
                                                            Tel = "", 
                                                            CellNo = "", 
                                                            Email = "", 
                                                            SWIFICode = "", 
                                                            AccountName = "",
                                                            AccountNo="", 
                                                            Currency = "", 
                                                            BankName = "",
                                                            PayTerms="",
                                                            CustID="0"
                                                                    }),
                    primaryKeys = new string[] { "CustID" },
                    idField = id
                   // CustID,CustAbbr,CustCode,CustName,CustAdd,Contact,Tel,CellNo,Email,SWIFICode,AccountName,AccountNo,Currency,PayTerms,BankName
                },
                tabs = new object[]{
                    new{
                      type = "grid",
                      rowId = "AutoID",
                      relationId = "CustCode",
                      defaults = new {AutoID = "0",SuppCode = "",CustCode="",CustAbbr = "",PNID = "",SuppPN="",CDesc = "",CSpec = "",TypeName = ""},
                      postFields = new string[] { "AutoID","PNID","SuppPN","CustPN","CustCode","CustAbbr","CDesc","SuppCode"}
                    }
                }
            };
            return View(model);
        }
       
    }
    public class CustomerApiController : ApiController
    {
        CustomerService payeeservice = new CustomerService();
        public dynamic Get(RequestWrapper query)
        {
            object payeeList="";
            bool noSizeLimit = query["noSizeLimit"] == null ? false : bool.Parse(query["noSizeLimit"]);
            ParamSP ps = new ParamSP().Name(CustomerService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "10" : query["rows"].ToString());
            ps.Parameter("ActionType", "GetCustomerList");
            ps.Parameter("CustCode", query["CustomerInfo"].ToString().Trim());

            if (noSizeLimit) //excel
            {
                payeeList = payeeservice.GetDynamicList(ps);
            }
            else
            {
                payeeList = payeeservice.GetDynamicListWithPaging(ps, "CustID");
            }
            return payeeList;
        }

        public dynamic GetCustPN(RequestWrapper query)
        {
            string action = query["action"].ToString();
            ParamSP ps = new ParamSP().Name(CustomerService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "10" : query["rows"].ToString());
            ps.Parameter("ActionType", "getCustPNListByCustCode");
            ps.Parameter("CustCode", query["CustCode"].ToString());

            if (action == "ListCustPNRelate")
            {
                ps.Parameter("SuppCode", query["SuppCode"].ToString());
                ps.Parameter("CustPN", query["CustPN"].ToString());
                ps.Parameter("SuppPN", query["SuppPN"].ToString());
                ps.Parameter("ActionType", "getCustPNListRelate");
            }
            var ColleteeList = payeeservice.GetDynamicListWithPaging(ps);
            return ColleteeList;
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
       [System.Web.Http.HttpPost]
        public dynamic Edit(dynamic data)
        {           
            JObject formData = JObject.Parse(data["form"].ToString());
            bool formChange = (bool)formData.GetValue("_changed"); 
            JObject TabData = JObject.Parse(data["tabs"][0].ToString());
            bool tabChange = (bool)TabData.GetValue("_changed");

            string CustID = formData["CustID"].ToString();
            ParamSP ps = new ParamSP().Name(CustomerService.strSP);
            //更新單頭
            if (formChange)
            {
                ps.Parameter("ActionType", "SaveCustomer");

                formData["CustAbbr"] = MmsHelper.HZ(formData["CustAbbr"].ToString(), 1);
                formData["CustName"] = MmsHelper.HZ(formData["CustName"].ToString(), 1);
                formData["CustAdd"] = MmsHelper.HZ(formData["CustAdd"].ToString(), 1);
                formData["AccountName"] = MmsHelper.HZ(formData["AccountName"].ToString(), 1);
                formData["BankName"] = MmsHelper.HZ(formData["BankName"].ToString(), 1);
                formData.Remove("_changed");
                foreach (var item in formData)
                {
                    ps.Parameter(item.Key, item.Value.ToString().Trim());
                }

                if (int.Parse(CustID) > 0)
                { payeeservice.StoredProcedureNoneQuery(ps); }
                else
                { CustID = payeeservice.StoredProcedureScalar(ps); }
            }

            //更新單身表格           
            if (tabChange)
            {
                foreach (var item in TabData)
                {
                    string itemKey = item.Key;//deleted,updated,inserted
                    if (item.Value.HasValues)
                    {
                        ParamSP ps1 = new ParamSP().Name(PartNoService.strSP);
                        ps1.Parameter("ActionType", "SavePartnoCustomerPNByCustCode");
                        ps1.Parameter("ActionItem", itemKey);

                        JArray ActionData = item.Value as JArray; //注意這個數組的每個元素對應表格的一行編輯的記錄
                        foreach (var itemDetail in ActionData)
                        {
                            JObject dr = itemDetail as JObject;

                            foreach (var drField in dr)
                            {
                                ps1.Parameter(drField.Key, drField.Value.ToString());
                            }

                            try
                            {
                                payeeservice.StoredProcedureNoneQuery(ps1);
                            }
                            catch (Exception ex)
                            {
                                ZScript.ShowMessage("保存明細：" + ex.Message);
                            }
                        }
                    }
                }
            }
            return GetPageData(CustID);
        }
     
       /// <returns></returns>  **如果要在js里面直接调用api方法,無論get/post,此api方法的参数名必须改为id,如果只由普通controller調用，那麼參數名就沒有限定
       
       public dynamic GetPageData(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                ParamSP ps = new ParamSP().Name(CustomerService.strSP);
                ps.Parameter("ActionType", "GetCustomerDataByCustID");
                ps.Parameter("CustID", id);              

                var payeeListNew = payeeservice.GetDynamicListForDataSet(ps);
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
                    form = new Customer().Extend(new
                    {
                        CustAbbr = "",
                        CustCode = "",
                        CustName = "",
                        CustAdd = "",
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
                        CustID = "0"
                    })
                };

                return result;
            }
        }

        [System.Web.Http.HttpPost]
       public void GetDelete(dynamic data)
        {
            ParamSP ps = new ParamSP().Name(CustomerService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "DeleteCustomer");
            ps.Parameter("CustID", HttpUtility.UrlDecode(data["value"].ToString()));
            bool bk = payeeservice.StoredProcedureNoneQuery(ps);
        }

        public string GetH()
        {
            return "0";
        }

        public List<dynamic> getPayeeList(string q)  
        {            
            var RoleList = CustomerService.getPayeeList(q);            
            return RoleList;
        }

        public List<dynamic> getCustomerAbbr(string q)  
        {
            var RoleList = CustomerService.getCustomerAbbr(q);            
            return RoleList;
        }


        public List<dynamic> getCustomerListForCombobox()    
        {
            var RoleList = CustomerService.getCustomerCodeListForCombobox();
            return RoleList;
        }

        public dynamic getCustomerInfoList(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name(CustomerService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "lookup-Customer");
            ps.Parameter("value", query["value"].ToString());
            ps.Parameter("text", query["text"].ToString());
            var ColleteeList = payeeservice.GetDynamicListWithPaging(ps);
            return ColleteeList;
        }

        [System.Web.Http.HttpPost]
        public dynamic GetcreateExcel(dynamic query)
        {
            ParamSP ps = new ParamSP().Name(CustomerService.strSP);
            ps.Parameter("ActionType", "GetCustomerList");
            ps.Parameter("CustCode", query["CustomerInfo"].ToString().Trim());

            try
            {
                string fileurl = CustomerService.ExportExcel(ps);
                return new { success = true, Msg = "", url = fileurl };
            }
            catch (Exception ex)
            {
                string AA = ex.Message;
                return new { success = false, Msg = ex.Message, url = "" };
            }
        }

        [System.Web.Http.HttpPost]
        public dynamic GetcreateExcelRelate(dynamic query)
        {
            //SuppPN,CustCode,CustPN,SuppCode
            ParamSP ps = new ParamSP().Name(PartNoService.strSP);
            ps.Parameter("ActionType", "getCustPNListExcelRelate");
            ps.Parameter("CustCode", query["CustCode"].ToString().Trim());
            ps.Parameter("SuppCode", query["SuppCode"].ToString().Trim());
            ps.Parameter("SuppPN", query["SuppPN"].ToString().Trim());
            ps.Parameter("CustPN", query["CustPN"].ToString().Trim());
            
            try
            {
                string fileurl = CustomerService.ExportExcelRelate(ps);
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
