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
using System.Web;
using System.Web.Mvc;

namespace ECGroup.Areas.Mms.Controllers
{
    public class SaleOrderController : Controller
    {
        //
        // GET: /Mms/AR_Invoice/

        public ActionResult Index()
        {
            var model = new
            {
                
                urls = new
                {
                    query = "/api/mms/SaleOrder/Get/",
                    edit = "/mms/SaleOrder/Edit/", //打開客戶訂单明細   
                    remove = "/api/mms/SaleOrder/GetDelete/",
                    excel = "/api/mms/SaleOrder/GetcreateExcel/"
                },
                resx = new
                {
                    detailTitle = "客户訂单明細",
                    noneSelect = "请先选择一条数据！",
                    editSuccess = "保存成功！",
                    detailTitleRMB = "客户訂单",
                    deleteConfirm = "確定要刪除所選的客户訂单么？",
                    deleteSuccess = "刪除成功",
                },
                form = new
                {
                    PO = "",
                    Customer = "",
                    CustPN = "",
                    SaleOrderStatus = "",
                    BeginDate = DateTime.Today.AddDays(-20).ToString("yyyy-MM-dd") + " 到 " + DateTime.Today.ToString("yyyy-MM-dd"),
                    

                },
                dataSource = new
                {
                    SaleOrderStatusList = SaleOrderService.getSaleOrderStatusList(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                idField = "POID"
                  
            };            
            return View(model);
        }

        public ActionResult Pending()
        {
            var model = new
            {

                urls = new
                {
                    query = "/api/mms/SaleOrder/GetPending/",
                    excel = "/api/mms/SaleOrder/GetSOPendingExcel/"
                },
                form = new
                {
                    PO = "",
                    Customer = "",
                    PendingStatus = "",
                    BeginDate = DateTime.Today.AddDays(-20).ToString("yyyy-MM-dd") + " 到 " + DateTime.Today.ToString("yyyy-MM-dd"),


                },
                dataSource = new
                {
                    PendingStatusList = SaleOrderService.getPendingStatusList(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                idField = "POID"

            };
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = new
            {
                urls = new
                {
                    getdata = "/api/mms/SaleOrder/GetPageData/",
                    edit = "/api/mms/SaleOrder/Edit/",
                    getrowid = "/api/mms/SaleOrder/GetRowId/",
                    getSaleOrderDetail = "/api/mms/SaleOrder/GetSaleOrderDetail/"
                    //eInvoice = "/api/mms/SaleQuote/GetTranseInvoice/"
                    //checkeInvoice = "/api/mms/BuyOrder/GetcheckSigneInvoice/",
                    //reloadInvoice = "/api/mms/AR_Invoice/GetreloadInvoice/",
                    //sendIdeas = "/api/mms/AR_Invoice/GetSendInvoice/"
                },
                resx = new
                {
                    rejected = "已撤消修改！",
                    editSuccess = "保存成功！"
                },

                dataSource = new
                {
                    pageData = new SaleOrderApiController().GetPageData(id),
                    IncotermList = SaleOrderService.GetIncotermList(),
                    CFMFlagList = MmsHelper.GetYON(),
                    CNCYList = MmsHelper.GetCurrencyType(),
                    SaleOrderStatusList = SaleOrderService.getSaleOrderStatusList(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()    
                },
                form = new
                {
                    //POID,PO,Customer,PODate,Incoterms,CustCode,Consignee,Address,Contact,Destination,Country,POStatus,Currency,InputBy,InputDT,CFMFlag
                    defaults = new SaleOrder().Extend(new { POID = id, PODate = DateTime.Today.ToString("yyyy-MM-dd"), Amount = "0",Country="CHINA", Currency = "RMB", CFMFlag = "N", POStatus = "录入", InputBy = User.Identity.Name, InputDT = "" }),
                    primaryKeys = new string[] { "POID" },
                    idField = id
                },
                tabs = new object[]{
                    new{
                      type = "grid",
                      rowId = "POLineNo",
                      relationId = "POID", //POID,POLineNo,CustPN,SuppPN,CDesc,CSpec,ReqDate,Qty,Unit,UnitPrice,UnitPriceT,Currency,Amount
                      defaults = new {POLineNo = "0",POID = id,CustPN = "",SuppPN = "",CDesc = "",CSpec="",ReqDate=DateTime.Today.ToString("yyyy-MM-dd"),UnitPrice = "0",Qty="0",Unit = "PC",UnitPriceT="0",Amount = "0"},
                      postFields = new string[] { "POLineNo","POID","CustPN","SuppPN","CDesc","CSpec","ReqDate","UnitPrice","UnitPriceT","Qty","Unit"}
                    }
                }
            };

            return View(model);

        }
    }

    public class SaleOrderApiController : ApiController
    {
        SaleOrderService SOService = new SaleOrderService();
        public dynamic Get(RequestWrapper query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(SaleOrderService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getlist");
            ps.Parameter("PO", query["PO"].ToString());
            ps.Parameter("CustPN", query["CustPN"].ToString());
            ps.Parameter("Customer", query["Customer"].ToString());
            ps.Parameter("SaleOrderStatus", query["SaleOrderStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);    // PO,Customer,CustPN,SaleOrderStatus,

            var ColleteeList = SOService.GetDynamicListWithPaging(ps, "POID");
            
            return ColleteeList;
        }

        public dynamic GetPending(RequestWrapper query)
        {
            object ColleteeList = "";
            bool noSizeLimit = query["noSizeLimit"] == null ? false : bool.Parse(query["noSizeLimit"]);
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(SaleOrderService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getPendinglist");
            ps.Parameter("PO", query["PO"].ToString());
            ps.Parameter("Customer", query["Customer"].ToString());
            ps.Parameter("PendingStatus", query["PendingStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);

            if (noSizeLimit) //excel
            {
                ColleteeList = SOService.GetDynamicList(ps);
            }
            else
            {
                ColleteeList = SOService.GetDynamicListWithPaging(ps);
            }
            return ColleteeList;
        }

        public dynamic GetSaleOrderDetail(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name(SaleOrderService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetSaleOrderDetail");
            ps.Parameter("POID", query["POID"].ToString());
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            var result = SOService.GetDynamicListWithPaging(ps);
            return result;
        }

        public string GetRowId()
        {
            return "0";
        }

        public dynamic GetPageData(int id)
        {            
            if (id!=0)
            {
                ParamSP ps = new ParamSP().Name(SaleOrderService.strSP);
                ps.Parameter("ActionType", "GetSaleOrderHeader");
                ps.Parameter("POID", id);
                var ARListNew = SOService.GetDynamicListForDataSet(ps);
                
                var result = new
                {
                    form = ARListNew[0][0],
                    tab0 = ARListNew[1]                    
                };
                return result;
            }
            else
            {
                var result = new
                {
                    form = new SaleOrder().Extend(new
                    {
                        Currency = "RMB",
                        Incoterms = "现金交易",
                        CFMFlag="N",
                        POStatus = "录入",
                        InputBy=User.Identity.Name,
                        InputDT = DateTime.Today.ToString("yyyy-MM-dd"),
                        PODate=DateTime.Today.ToString("yyyy-MM-dd"),
                        Amount=0,
                        Customer=""
                    }),
                    tab0 = new List<dynamic>(),
                };

                return result;
            }
        }

        [System.Web.Http.HttpPost]
        public dynamic Edit(dynamic data)
        {
            JObject formData = JObject.Parse(data["form"].ToString());
            bool formChange = (bool)formData.GetValue("_changed");
            JObject TabData = JObject.Parse(data["tabs"][0].ToString());
            bool tabChange = (bool)TabData.GetValue("_changed");
            string POID = formData["POID"].ToString();
            //更新單頭
            ParamSP ps = new ParamSP().Name(SaleOrderService.strSP);
            if (formChange)
            {
                formData.Remove("InputDT"); formData.Remove("Amount"); formData.Remove("_changed");
                ps.Parameter("ActionType", "SaveSaleOrderHeader");
                foreach (var item in formData)
                {
                    { ps.Parameter(item.Key, item.Value.ToString()); }
                }

                try
                {
                    if (POID == "0")
                    { POID = SOService.StoredProcedureScalar(ps); }
                    else { SOService.StoredProcedureNoneQuery(ps); }
                }
                catch (Exception ex)
                {
                    ZScript.ShowMessage(ex.Message);
                }
            }

            //更新單身表格           
            if (tabChange)
            {
                foreach (var item in TabData)
                {
                    string itemKey = item.Key;//deleted,updated,inserted
                    if (item.Value.HasValues)
                    {
                        ParamSP ps1 = new ParamSP().Name(SaleOrderService.strSP);
                        ps1.Parameter("ActionType", "SaveSaleOrderSub");
                        ps1.Parameter("ActionItem", itemKey);

                        JArray ActionData = item.Value as JArray; //注意這個數組的每個元素對應表格的一行編輯的記錄
                        foreach (var itemDetail in ActionData)
                        {
                            JObject dr = itemDetail as JObject;
                            dr["CDesc"] = MmsHelper.HZ(dr["CDesc"].ToString(), 1);
                            dr["CSpec"] = MmsHelper.HZ(dr["CSpec"].ToString(), 1);
                            
                            foreach (var drField in dr)
                            {
                                ps1.Parameter(drField.Key, drField.Value.ToString());
                            }

                            try
                            {
                                SOService.StoredProcedureNoneQuery(ps1);
                            }
                            catch (Exception ex)
                            {
                                ZScript.ShowMessage("保存明細："+ex.Message);
                            }
                        }
                    }
                }
            }
            return GetPageData(int.Parse(POID));
        }

        [System.Web.Http.HttpPost]
        public dynamic GetcreateExcel(dynamic query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(SaleOrderService.strSP);
            ps.Parameter("ActionType", "getlist"); ps.Parameter("ActionItem", "excel");
            ps.Parameter("PO", query["PO"].ToString());
            ps.Parameter("CustPN", query["CustPN"].ToString());
            ps.Parameter("Customer", query["Customer"].ToString());
            ps.Parameter("SaleOrderStatus", query["SaleOrderStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2); 

            try
            {
                string fileurl = SaleOrderService.ExportExcel(ps);
                return new { success = true, Msg = "", url = fileurl };
            }
            catch (Exception ex)
            {
                string AA = ex.Message;
                return new { success = false, Msg = ex.Message, url = "" };
            }
        }

        [System.Web.Http.HttpPost]
        public dynamic GetSOPendingExcel(dynamic query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(SaleOrderService.strSP);
            ps.Parameter("ActionType", "getPendinglist");
            ps.Parameter("PO", query["PO"].ToString());
            ps.Parameter("Customer", query["Customer"].ToString());
            ps.Parameter("PendingStatus", query["PendingStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);

            try
            {
                string fileurl = SaleOrderService.ExportSOPendingExcel(ps);
                return new { success = true, Msg = "", url = fileurl };
            }
            catch (Exception ex)
            {
                string AA = ex.Message;
                return new { success = false, Msg = ex.Message, url = "" };
            }
        }

        [System.Web.Http.HttpPost]
        public dynamic GetDelete(dynamic data)
        {
            ParamSP ps = new ParamSP().Name(SaleOrderService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "DeleteSaleOrder");
            ps.Parameter("POId", data["value"].ToString());
            bool bk = SOService.StoredProcedureNoneQuery(ps);

            return bk;
        }  

        
    }
}
