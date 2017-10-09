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
    public class APListController : Controller
    {
       
        public ActionResult Index()
        {
            var model = new
            {                
                urls = new
                {
                    query = "/api/mms/APList/Get",
                    edit = "/api/mms/APList/Edit",
                    remove = "/api/mms/APList/Delete/",
                    createbatch = "/api/mms/APList/GetCreateBatch",
                    createBatchExcel = "/api/mms/APList/GetBatchExcel/",
                    saveClearOnce = "/api/mms/APList/EditClearOnce/"
                },
                dataSource = new
                {   APStatusList=APListService.getAPStatusList(),              
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                resx = new
                {
                    editSuccess="保存成功！",
                    batchConfirm = "确定生成对账单吗?",
                    noneSelect = "請選擇一條數據！",
                    BatchNoSuccess = "生成对账成功！",
                    exportBatchExcelSuccess = "导出对账成功！"
                },
                idField = "ARID",
                form = new
                {
                    BatchNo="",RcvNo="",Supplier="",
                    APStatus = "未对账",
                    BuyNo = "",
                    BeginDate= DateTime.Today.AddDays(-20).ToString("yyyy-MM-dd") + " 到 " + DateTime.Today.ToString("yyyy-MM-dd")
                },
                defaultRow = new 
                {
                    APID = "0",
                    APStatus = "未对账"                    
                },
                setting = new
                {
                    idField = "APID",
                    postListFields = new string[] { "APID", "DueDate", "APStatus", "RemitNo", "RemitDate", "Remarks" }
                }
            };
            return View(model);
        }

        public ActionResult Detail(string id)
        {
            var model = new
            {
                urls = new
                {
                    query = "/api/mms/APList/GetDetail",
                    edit = "/api/mms/APList/EditDetail",
                    remove = "/api/mms/APList/getDeleteDetail/"
                },
                dataSource = new
                {
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                resx = new
                {
                    editSuccess = "保存成功！",
                    noneSelect = "請選擇一條數據！",
                },
                idField = "SNO",
                form = new
                {
                    APID = id
                },
                defaultRow = new
                {//SNO,ARID,RcvNo,RcvDate,CostItem,Amount,Remarks,CreateDate
                    SNO = "0",
                    APID = id,
                    PayNo = "",
                    PayDate = DateTime.Today.ToString("yyyy-MM-dd"),
                    CostItem = "",
                    Amount = "0",
                    Remarks = "",
                    CreateDate = DateTime.Today.ToString("yyyy-MM-dd")
                },
                setting = new
                {
                    idField = "SNO",
                    postListFields = new string[] { "SNO", "APID", "PayNo", "PayDate", "CostItem", "Amount", "Remarks" }
                }
            };
            return View(model);
        }
    }

    public class APListApiController : ApiController
    {
        APListService pService = new APListService();
        public dynamic Get(RequestWrapper query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");

            ParamSP ps = new ParamSP().Name(APListService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getlist");
            ps.Parameter("BatchNo", query["BatchNo"].ToString().Trim());
            ps.Parameter("RcvNo", query["RcvNo"].ToString().Trim());
            ps.Parameter("BuyNo", query["BuyNo"].ToString().Trim());
            ps.Parameter("APStatus", query["APStatus"].ToString().Trim());
            ps.Parameter("Supplier", query["Supplier"].ToString().Trim());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);

            var BusInessTypeList = pService.GetDynamicListWithPaging(ps);
            return BusInessTypeList;
        }

        public dynamic GetDetail(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name(APListService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "10" : query["rows"].ToString());
            ps.Parameter("ActionType", "getlistDetail");
            ps.Parameter("APID", query["APID"].ToString().Trim());

            var arDetail = pService.GetDynamicListWithPaging(ps);
            return arDetail;
        }

        [System.Web.Http.HttpPost]
        public dynamic Edit(dynamic data)
        {
            JObject gridData = JObject.Parse(data["list"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name(APListService.strSP);
            if (gridChange)
            {
                ps.Parameter("ActionType", "SaveAPHeader");

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
                            ps.Parameter("APID", itemDetail["APID"].ToString());
                            ps.Parameter("DueDate", itemDetail["DueDate"].ToString());
                            ps.Parameter("APStatus", itemDetail["APStatus"].ToString());
                            ps.Parameter("Remarks", itemDetail["Remarks"].ToString());
                            ps.Parameter("RemitNo", itemDetail["RemitNo"].ToString());
                            ps.Parameter("RemitDate", itemDetail["RemitDate"].ToString());

                            pService.StoredProcedureNoneQuery(ps);
                        }
                    }
                }
            }
            return "OK";
        }

        [System.Web.Http.HttpPost]
        public dynamic EditDetail(dynamic data)
        {
            JObject gridData = JObject.Parse(data["list"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name(APListService.strSP);
            if (gridChange)
            {
                ps.Parameter("ActionType", "SaveAPSub");//SNO,ARID,RcvNo,RcvDate,CostItem,Amount,Remarks

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
                            ps.Parameter("SNO", itemDetail["SNO"].ToString());
                            ps.Parameter("APID", itemDetail["APID"].ToString());
                            ps.Parameter("PayNo", itemDetail["PayNo"].ToString());
                            ps.Parameter("PayDate", itemDetail["PayDate"].ToString());
                            ps.Parameter("CostItem", itemDetail["CostItem"].ToString());
                            ps.Parameter("Amount", itemDetail["Amount"].ToString());
                            ps.Parameter("Remarks", itemDetail["Remarks"].ToString());

                            pService.StoredProcedureNoneQuery(ps);
                        }
                    }
                }
            }
            return "OK";
        }

        [System.Web.Http.HttpPost]
        public void getDeleteDetail(dynamic data)
        {
            ParamSP ps = new ParamSP().Name(APListService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "deleteAPSub");
            ps.Parameter("APIDList", data["value"].ToString());
            bool bk = pService.StoredProcedureNoneQuery(ps);
        }   

        public List<dynamic> getAPStatusList(string q)    
        {
            var RoleList = APListService.getAPStatusList();
            return RoleList;
        }

        [System.Web.Http.HttpPost]
        public string GetCreateBatch(dynamic data)
        {
            string ARIDList = data["value"].ToString();
            ParamSP ps = new ParamSP().Name(APListService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "CreateAPBatchList");
            ps.Parameter("APIDList", ARIDList);
            string AA = pService.StoredProcedureScalar(ps);
            return AA;
        }    
        
        [System.Web.Http.HttpPost]
        public dynamic GetBatchExcel(string id)
        {
            ParamSP ps = new ParamSP().Name(APListService.strSP);
            ps.Parameter("ActionType", "CreateAPExcelByBatchNo");
            ps.Parameter("BatchNo", id);

            try
            {
                string fileurl = APListService.ExportAPListExcel(ps);
                return new { success = true, Msg = "", url = fileurl };
            }
            catch (Exception ex)
            {
                string AA = ex.Message;
                return new { success = false, Msg = ex.Message, url = "" };
            }  
        }

        [System.Web.Http.HttpPost]
        public dynamic EditClearOnce(dynamic data)
        {
            //post = { key: apid, duedate: DueDate, apstatus: APStatus, payno: PayNo, paydate: PayDate, remarks: Remarks }
            try
            {
                string APID = data["key"].ToString();
                string DueDate = data["duedate"].ToString();
                string PayNo = data["payno"].ToString();
                string PayDate = data["paydate"].ToString();
                string Remarks = data["remarks"].ToString();

                ParamSP ps = new ParamSP().Name(APListService.strSP);
                ParamSPData psd = ps.GetData();
                ps.Parameter("ActionType", "SaveClearOnce");
                ps.Parameter("APID", APID);
                ps.Parameter("DueDate", DueDate);
                ps.Parameter("PayNo", PayNo);
                ps.Parameter("PayDate", PayDate);
                ps.Parameter("Remarks", Remarks);

                string AA = pService.StoredProcedureScalar(ps);
                return new { success = true, Msg = AA };
            }
            catch (Exception ex)
            {
                string AA = ex.Message;
                return new { success = false, Msg = ex.Message };
            }
        }    

    }
}
