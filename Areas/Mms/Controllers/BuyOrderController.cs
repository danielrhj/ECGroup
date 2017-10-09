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
    public class BuyOrderController : Controller
    {
        //
        // GET: /Mms/AR_Invoice/

        public ActionResult Index()
        {
            var model = new
            {
                
                urls = new
                {
                    query = "/api/mms/BuyOrder/Get/",
                    edit = "/mms/BuyOrder/Edit/", //打開采购单明細                    
                    createDirect = "/api/mms/BuyOrder/GetcreatReceivingDirect/",   //采购订单直转入库库
                    remove = "/api/mms/BuyOrder/GetDelete/",
                    excel = "/api/mms/BuyOrder/GetcreateExcel/",
                    batchReceivingConfirm = "/api/mms/BuyOrder/GetBatchRcv/"    //未入库采购单合并入库
                },
                resx = new
                {
                    batchReceivingSuccess = "批量收货成功!",
                    BatchRcvConfirm="确定要批量收货么?",
                    detailTitle = "采购单明細",
                    noneSelect = "请先选择一条数据！",
                    editSuccess = "保存成功！",
                    detailTitleRMB = "采购单",
                    deleteConfirm = "確定要刪除所選的采购单么？",
                    deleteSuccess = "刪除成功",
                },
                form = new
                {
                    BuyNo = "",
                    SuppAbbr = "",
                    BuyStatus = "",
                    BeginDate = DateTime.Today.AddDays(-180).ToString("yyyy-MM-dd") + " 到 " + DateTime.Today.ToString("yyyy-MM-dd"),
                    

                },
                dataSource = new
                {
                    BuyOrderStatusList = BuyOrderService.getBuyOrderStatusList(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                idField = "BuyID"
                  
            };            
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = new
            {
                urls = new
                {
                    getdata = "/api/mms/BuyOrder/GetPageData/",
                    edit = "/api/mms/BuyOrder/Edit/",
                    getrowid = "/api/mms/BuyOrder/GetRowId/",
                    getorderDetail = "/api/mms/BuyOrder/GetBuyOrderDetail/",
                    //batchReceivingConfirm = "/api/mms/BuyOrder/GetBatchRcv/",
                    createBuyOrderExcel = "/api/mms/BuyOrder/GetBuyOrderExcel/"
                },
                resx = new
                {
                    rejected = "已撤消修改！",
                    editSuccess = "保存成功！",
                    BatchRcvConfirm = "确定要将以下采购订单合并入库吗?",
                    batchReceivingSuccess = "采购订单合并入库成功!"
                },

                dataSource = new
                {
                    pageData = new BuyOrderApiController().GetPageData(id),
                    PriceTypeList = new BuyOrderApiController().PriceTypeList(),
                    //CFMFlagList = MmsHelper.GetYON(),
                    CNCYList = MmsHelper.GetCurrencyType(),
                    BuyOrderStatusList=new BuyOrderApiController().getBuyOrderStatusList(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()    
                },
                form = new
                {
                    //BuyID,BuyNo,BuyDate,SuppCode,SuppAbbr,Contact,Tel,Amount,Currency,PriceType,CFMFlag,BuyStatus,InputBy,InputDT
                    defaults = new BuyOrder().Extend(new { BuyID = id, BuyNo = "", BuyDate = DateTime.Today.ToString("yyyy-MM-dd"), SuppCode = "", SuppAbbr = "", Contact = "", Tel = "", Amount = "0", Currency = "RMB", PriceType = "含税",BuyStatus="录入", InputBy=User.Identity.Name, InputDT="" }),
                    primaryKeys = new string[] { "BuyID" },
                    idField = id
                },
                tabs = new object[]{
                    new{
                      type = "grid",
                      rowId = "SNO",
                      relationId = "BuyID", //SNO,BuyID,PO,SuppPN,CDesc,CSpec,BuyPrice,Qty,Unit,Amount
                      defaults = new {SNO = "0",BuyID = id,PO = "",Brand="",SuppPN = "",CDesc = "",CSpec="",BuyPrice = "0",Qty="0",Unit = "PC",Amount = "0",ReqDate=DateTime.Today.ToString("yyyy-MM-dd")},
                      postFields = new string[] { "SNO","BuyID","PO","Brand","SuppPN","CDesc","CSpec","BuyPrice","Qty","Unit","ReqDate"}
                    }
                }
            };

            return View(model);

        }

        public ActionResult Pending()
        {
            var model = new
            {

                urls = new
                {
                    query = "/api/mms/BuyOrder/GetPending/",
                    excel = "/api/mms/BuyOrder/GetcreateExcelPending/"
                },
                form = new
                {
                    BuyNo = "",
                    SuppAbbr = "",
                    PendingStatus = "",
                    BeginDate = DateTime.Today.AddDays(-20).ToString("yyyy-MM-dd") + " 到 " + DateTime.Today.ToString("yyyy-MM-dd"),


                },
                dataSource = new
                {
                    PendingStatusList = BuyOrderService.getPendingStatusList(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                idField = "BuyID"

            };
            return View(model);
        }
    }

    public class BuyOrderApiController : ApiController
    {
        BuyOrderService BOService = new BuyOrderService();
        public dynamic Get(RequestWrapper query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(BuyOrderService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getlist");
            ps.Parameter("BuyNo", query["BuyNo"].ToString());
            ps.Parameter("SuppAbbr", query["SuppAbbr"].ToString());
            ps.Parameter("BuyStatus", query["BuyStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);

            var ColleteeList = BOService.GetDynamicListWithPaging(ps, "BuyID");

            return ColleteeList;
        }

        public dynamic GetBuyOrderDetail(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name(BuyOrderService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetBuyOrderDetail");
            ps.Parameter("BuyID", query["BuyID"].ToString());
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            var result = BOService.GetDynamicListWithPaging(ps);
            return result;
        }

        public dynamic GetPending(RequestWrapper query)
        {
            object ColleteeList = "";
            bool noSizeLimit = query["noSizeLimit"] == null ? false : bool.Parse(query["noSizeLimit"]);
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(BuyOrderService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getPendinglist");
            ps.Parameter("BuyNo", query["BuyNo"].ToString());
            ps.Parameter("SuppAbbr", query["SuppAbbr"].ToString());
            ps.Parameter("PendingStatus", query["PendingStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);

            if (noSizeLimit) //excel
            {
                ColleteeList = BOService.GetDynamicList(ps);
            }
            else
            {
                ColleteeList = BOService.GetDynamicListWithPaging(ps);
            }
            return ColleteeList;
        }

        public string GetRowId()
        {
            return "0";
        }

        public dynamic GetPageData(int id)
        {
            if (id != 0)
            {
                ParamSP ps = new ParamSP().Name(BuyOrderService.strSP);
                ps.Parameter("ActionType", "GetBuyOrderHeader");
                ps.Parameter("BuyID", id);
                var ARListNew = BOService.GetDynamicListForDataSet(ps);

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
                    form = new BuyOrder().Extend(new
                    {
                        Currency = "RMB",
                        PriceType = "含税",
                        CFMFlag = "N",
                        BuyStatus = "录入",
                        InputBy = User.Identity.Name,
                        InputDT = DateTime.Today.ToString("yyyy-MM-dd"),
                        BuyDate = DateTime.Today.ToString("yyyy-MM-dd"),
                        Amount = 0,
                        SuppAbbr = ""
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
            string BuyId = formData["BuyID"].ToString();
            //更新單頭
            ParamSP ps = new ParamSP().Name(BuyOrderService.strSP);
            if (formChange)
            {
                formData.Remove("InputDT"); formData.Remove("Amount"); formData.Remove("_changed");
                ps.Parameter("ActionType", "SaveBuyOrderHeader");
                foreach (var item in formData)
                {
                    { ps.Parameter(item.Key, item.Value.ToString()); }
                }

                try
                {
                    if (BuyId == "0")
                    { BuyId = BOService.StoredProcedureScalar(ps); }
                    else { BOService.StoredProcedureNoneQuery(ps); }
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
                        ParamSP ps1 = new ParamSP().Name(BuyOrderService.strSP);
                        ps1.Parameter("ActionType", "SaveBuyOrderSub");
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
                            //ps1.Parameter("BuyId", BuyId);

                            try
                            {
                                BOService.StoredProcedureNoneQuery(ps1);
                            }
                            catch (Exception ex)
                            {
                                ZScript.ShowMessage("保存明細：" + ex.Message);
                            }
                        }
                    }
                }
            }
            return GetPageData(int.Parse(BuyId));
        }

        //[System.Web.Http.HttpPost]
        //public string getCheckInvoice(dynamic data)
        //{
        //    string strS = data["value"].ToString();
        //    var ARInvoiceService = new AR_InvoiceService();
        //    ParamSP ps = new ParamSP().Name("ups_AR_ReceiveInvoice");
        //    ps.Parameter("ActionType", "CheckPreData");
        //    ps.Parameter("ReceiveInvoiceNo", strS); 
        //    ps.Parameter("UserCode", User.Identity.Name);
        //    var result = ARInvoiceService.GetDynamic(ps);
        //    return result == null ? "" : result.Result.ToString();
        //}

        //[System.Web.Http.HttpPost]
        //public dynamic getPreAccount(dynamic data)
        //{
        //    string strS = data["value"].ToString();
        //    var ARInvoiceService = new AR_InvoiceService();
        //    ParamSP ps = new ParamSP().Name("usp_ARAccount");
        //    ps.Parameter("ActionType", "CreatePreAccountByMore");
        //    ps.Parameter("ReceiveInvoiceNo", strS); 
        //    ps.Parameter("UserCode", User.Identity.Name);
        //    var result = ARInvoiceService.GetDynamic(ps);
        //    return result;
        //}

        [System.Web.Http.HttpPost]
        public dynamic GetDelete(dynamic data)
        {
            ParamSP ps = new ParamSP().Name(BuyOrderService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "DeleteBuyOrder");
            ps.Parameter("BuyID", data["value"].ToString());
            bool bk = BOService.StoredProcedureNoneQuery(ps);

            return bk;
        }

        public List<dynamic> getBuyOrderStatusList()
        {
            var RoleList = BuyOrderService.getBuyOrderStatusList();
            return RoleList;
        }

        public List<dynamic> PriceTypeList()
        {
            var RoleList = BuyOrderService.getPriceTypeList();
            return RoleList;
        }

        [System.Web.Http.HttpPost]
        public dynamic GetcreatReceivingDirect(dynamic data)
        {
            string BuyID = data["BuyID"].ToString();
            ParamSP ps = new ParamSP().Name(BuyOrderService.strSP);
            ps.Parameter("ActionType", "CreateReceivingDirect");
            ps.Parameter("BuyID", BuyID);
            ps.Parameter("InputBy", User.Identity.Name);
            var result = BOService.GetDynamic(ps);
            return result;
        }

        //采购订单合并入库并计入应付账款
        [System.Web.Http.HttpPost]
        public dynamic GetBatchRcv(dynamic data)
        {
            string BuyIDs = data["value"].ToString();
            ParamSP ps = new ParamSP().Name(BuyOrderService.strSP);
            ps.Parameter("ActionType", "BatchReceiving");
            ps.Parameter("SuppAddress", BuyIDs);
            ps.Parameter("InputBy", User.Identity.Name);
            var msg = BOService.GetDynamic(ps);
            return msg;
        }

        [System.Web.Http.HttpPost]
        public dynamic GetBuyOrderExcel(string id)
        {
            ParamSP ps = new ParamSP().Name(BuyOrderService.strSP);
            ps.Parameter("ActionType", "CreateBuyOrderExcel");
            ps.Parameter("BuyID", id);

            try
            {
                string fileurl = BuyOrderService.ExportBOExcel(ps);
                return new { success = true, Msg = "", url = fileurl };
            }
            catch (Exception ex)
            {
                string AA = ex.Message;
                return new { success = false, Msg = ex.Message, url = "" };
            }
        }

        [System.Web.Http.HttpPost]
        public dynamic GetcreateExcel(dynamic query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(BuyOrderService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getlist"); ps.Parameter("ActionItem", query["type"].ToString());
            ps.Parameter("BuyNo", query["BuyNo"].ToString());
            ps.Parameter("SuppAbbr", query["SuppAbbr"].ToString());
            ps.Parameter("BuyStatus", query["BuyStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);

            try
            {
                string fileurl = BuyOrderService.ExportExcel(ps);
                return new { success = true, Msg = "", url = fileurl };
            }
            catch (Exception ex)
            {
                string AA = ex.Message;
                return new { success = false, Msg = ex.Message, url = "" };
            }
        }

        [System.Web.Http.HttpPost]
        public dynamic GetcreateExcelPending(dynamic query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");

            ParamSP ps = new ParamSP().Name(BuyOrderService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getPendinglist");
            ps.Parameter("BuyNo", query["BuyNo"].ToString());
            ps.Parameter("SuppAbbr", query["SuppAbbr"].ToString());
            ps.Parameter("PendingStatus", query["PendingStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);

            try
            {
                string fileurl = BuyOrderService.ExportPendingExcel(ps);
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
