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
    public class ARListController : Controller
    {
        //

        public ActionResult Index()
        {
            var model = new
            {                
                urls = new
                {
                    query = "/api/mms/ARList/Get",
                    edit = "/api/mms/ARList/Edit",
                    remove = "/api/mms/ARList/Delete/",
                    createbatch = "/api/mms/ARList/GetCreateBatch" ,
                    createBatchExcel = "/api/mms/ARList/GetBatchExcel/",
                    saveClearOnce = "/api/mms/ARList/EditClearOnce/"
                },
                dataSource = new
                {   ARStatusList=ARListService.getARStatusList(),              
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
                    BatchNo="",ShipNo="",Customer="",
                    ARStatus = "未对账",
                    PO = "",
                    BeginDate= DateTime.Today.AddDays(-20).ToString("yyyy-MM-dd") + " 到 " + DateTime.Today.ToString("yyyy-MM-dd")
                },
                defaultRow = new 
                {
                    ARID = "0",
                    ARStatus = "未對賬"                    
                },
                setting = new
                {
                    idField = "ARID",
                    postListFields = new string[] { "ARID", "DueDate", "ARStatus", "RcvNo", "RcvDate", "Remarks" }
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
                    query = "/api/mms/ARList/GetDetail",
                    edit = "/api/mms/ARList/EditDetail",
                    remove = "/api/mms/ARList/getDeleteDetail/"
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
                    ARID = id
                },
                defaultRow = new
                {//SNO,ARID,RcvNo,RcvDate,CostItem,Amount,Remarks,CreateDate
                    SNO = "0",ARID = id,RcvNo = "",
                    RcvDate = DateTime.Today.ToString("yyyy-MM-dd"),
                    CostItem = "",Amount = "0",Remarks = "",
                    CreateDate = DateTime.Today.ToString("yyyy-MM-dd")
                },
                setting = new
                {
                    idField = "SNO",
                    postListFields = new string[] { "SNO", "ARID", "RcvNo", "RcvDate", "CostItem", "Amount", "Remarks" }
                }
            };
            return View(model);
        }
    }

    public class ARListApiController : ApiController
    {
        ARListService mService = new ARListService();
        public dynamic Get(RequestWrapper query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");

            ParamSP ps = new ParamSP().Name(ARListService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getlist");
            ps.Parameter("BatchNo", query["BatchNo"].ToString().Trim());
            ps.Parameter("ShipNo", query["ShipNo"].ToString().Trim());
            ps.Parameter("PO", query["PO"].ToString().Trim());
            ps.Parameter("ARStatus", query["ARStatus"].ToString().Trim());
            ps.Parameter("Customer", query["Customer"].ToString().Trim());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2); 

            var BusInessTypeList = mService.GetDynamicListWithPaging(ps);
            return BusInessTypeList;
        }

        public dynamic GetDetail(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name(ARListService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getlistDetail");
            ps.Parameter("ARID", query["ARID"].ToString().Trim());

            var arDetail = mService.GetDynamicListWithPaging(ps);
            return arDetail;
        }

        [System.Web.Http.HttpPost]
        public dynamic Edit(dynamic data)
        {
            JObject gridData = JObject.Parse(data["list"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name(ARListService.strSP);
            if (gridChange)
            {
                ps.Parameter("ActionType", "SaveARHeader");

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
                            ps.Parameter("ARID", itemDetail["ARID"].ToString());
                            ps.Parameter("DueDate", itemDetail["DueDate"].ToString());
                            ps.Parameter("ARStatus", itemDetail["ARStatus"].ToString());
                            ps.Parameter("Remarks", itemDetail["Remarks"].ToString());
                            ps.Parameter("RcvNo", itemDetail["RcvNo"].ToString());
                            ps.Parameter("RcvDate", itemDetail["RcvDate"].ToString());
                           
                            mService.StoredProcedureNoneQuery(ps);
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

            ParamSP ps = new ParamSP().Name(ARListService.strSP);
            if (gridChange)
            {
                ps.Parameter("ActionType", "SaveARSub");//SNO,ARID,RcvNo,RcvDate,CostItem,Amount,Remarks

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
                            ps.Parameter("ARID", itemDetail["ARID"].ToString());
                            ps.Parameter("RcvNo", itemDetail["RcvNo"].ToString());
                            ps.Parameter("RcvDate", itemDetail["RcvDate"].ToString());
                            ps.Parameter("CostItem", itemDetail["CostItem"].ToString());
                            ps.Parameter("Amount", itemDetail["Amount"].ToString());
                            ps.Parameter("Remarks", itemDetail["Remarks"].ToString());

                            mService.StoredProcedureNoneQuery(ps);
                        }
                    }
                }
            }
            return "OK";
        }     

        [System.Web.Http.HttpPost]
        public void getDeleteDetail(dynamic data)
        {
            ParamSP ps = new ParamSP().Name(ARListService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "deleteARSub");
            ps.Parameter("ARIDList", data["value"].ToString());
            bool bk = mService.StoredProcedureNoneQuery(ps);
        }        

        public List<dynamic> getARStatusList(string q)    
        {
            var RoleList = ARListService.getARStatusList();
            return RoleList;
        }

        [System.Web.Http.HttpPost]
        public string GetCreateBatch(dynamic data)
        {
            string ARIDList = data["value"].ToString();
            ParamSP ps = new ParamSP().Name(ARListService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "CreateARBatchList");
            ps.Parameter("ARIDList", ARIDList);
            string AA = mService.StoredProcedureScalar(ps);
            return AA;
        }    
        
        [System.Web.Http.HttpPost]
        public dynamic GetBatchExcel(string id)
        {
            ParamSP ps = new ParamSP().Name(ARListService.strSP);
            ps.Parameter("ActionType", "CreateARExcelByBatchNo");
            ps.Parameter("BatchNo", id);

            try
            {
                string fileurl = ARListService.ExportARListExcel(ps);
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
            //post = { key: arid, duedate: DueDate, arstatus: ARStatus, rcvno: RcvNo, rcvdate: RcvDate,remarks:Remarks }
            try
            {
                string ARID = data["key"].ToString();
                string DueDate = data["duedate"].ToString();
                string RcvNo = data["rcvno"].ToString();
                string RcvDate = data["rcvdate"].ToString();
                string Remarks = data["remarks"].ToString();

                ParamSP ps = new ParamSP().Name(ARListService.strSP);
                ParamSPData psd = ps.GetData();
                ps.Parameter("ActionType", "SaveClearOnce");
                ps.Parameter("ARID", ARID);
                ps.Parameter("DueDate", DueDate);
                ps.Parameter("RcvNo", RcvNo);
                ps.Parameter("RcvDate", RcvDate);
                ps.Parameter("Remarks", Remarks);

                string AA = mService.StoredProcedureScalar(ps);
                return new { success = true, Msg = AA };
            }
            catch (Exception ex)
            {
                string AA = ex.Message;
                return new { success = false, Msg = ex.Message};
            }  
        }    
    }
}
