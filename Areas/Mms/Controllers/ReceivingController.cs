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
    public class ReceivingController : Controller
    {
        //
        // GET: /Mms/AR_Invoice/

        public ActionResult Index()
        {
            var model = new
            {
                
                urls = new
                {
                    query = "/api/mms/Receiving/Get/",
                    edit = "/mms/Receiving/Edit/", //打開采购单明細  
                    remove = "/api/mms/Receiving/GetDelete/",
                    excel = "/api/mms/Receiving/GetcreateExcel/"
                },
                resx = new
                {
                    detailTitle = "入庫单明細",
                    noneSelect = "请先选择一条数据！",
                    editSuccess = "保存成功！",
                    detailTitleRMB = "入庫",
                    deleteConfirm = "確定要刪除所選的入庫单么？",
                    deleteSuccess = "刪除成功"
                },
                form = new
                {
                    RcvNo = "",
                    BuyNo = "",
                    DO = "",
                    SuppAbbr = "",
                    RcvStatus = "",
                    BeginDate = DateTime.Today.AddDays(-60).ToString("yyyy-MM-dd") + " 到 " + DateTime.Today.ToString("yyyy-MM-dd")

                },
                dataSource = new
                {
                    ReceivingStatusList = ReceivingService.getReceivingStatus(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                idField = "RcvID"
                  
            };            
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = new
            {
                urls = new
                {
                    getdata = "/api/mms/Receiving/GetPageData/",
                    edit = "/api/mms/Receiving/Edit/",
                    getrowid = "/api/mms/Receiving/GetRowId/",
                    getorderDetail = "/api/mms/Receiving/GetReceivingDetail/",
                    updateAPList = "/api/mms/Receiving/GetUpdateAPList/"
                },
                resx = new
                {
                    rejected = "已撤消修改！",
                    editSuccess = "保存成功！"
                },

                dataSource = new
                {
                    pageData = new ReceivingApiController().GetPageData(id),
                    PriceTypeList = new ReceivingApiController().PriceTypeList(),
                    CNCYList = MmsHelper.GetCurrencyType(),
                    UnitList = MaterialTypeService.getUnitListCombo(false),
                    TaxRateList = MaterialTypeService.getTaxRateListCombo(false),
                    ReceivingStatusList = new ReceivingApiController().getReceivingStatus(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()    
                },
                form = new
                {
                    //RcvID, BuyNo, RcvNo, DO, RcvDate, SuppCode, SuppAbbr, Contact, Tel, Email, Currency, Remarks,RcvStatus, StatusDT, PriceType, InputBy, InputDT,CFMFlag, CFMDate
                    defaults = new Receiving().Extend(new { RcvID = id, BuyDate = DateTime.Today.ToString("yyyy-MM-dd"), Amount = "0", Currency = "RMB", PriceType = "含税", RcvStatus = "收货预录入", InputBy = User.Identity.Name, InputDT = DateTime.Today.ToString("yyyy-MM-dd") }),
                    primaryKeys = new string[] { "RcvID" },
                    idField = id
                },
                tabs = new object[]{
                    new{
                      type = "grid",
                      rowId = "SNO",
                      relationId = "RcvID", //SNO, RcvID, SuppPN, CDesc, CSpec, Brand, RcvPrice, Qty, Unit, Amount, PayNo, InvoiceNo, BuyCost,OtherCost
                      defaults = new {SNO = "0",RcvID = id,BuyNo="",SuppPN = "",CDesc = "",CSpec="",Brand="",RcvPrice = "0",TaxRate="0",Qty="0",Unit = "PC",Amount = "0"},
                      postFields = new string[] { "SNO","RcvID","BuyNo","Brand","SuppPN","CDesc","CSpec","RcvPrice","TaxRate","Qty","Unit"}
                    }
                }
            };

            return View(model);

        }       

    }

    public class ReceivingApiController : ApiController
    {
        ReceivingService RcvService = new ReceivingService();
        public dynamic Get(RequestWrapper query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(ReceivingService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getlist");
            ps.Parameter("RcvNo", query["RcvNo"].ToString());
            ps.Parameter("DO", query["DO"].ToString());
            ps.Parameter("BuyNo", query["BuyNo"].ToString());
            ps.Parameter("SuppAbbr", query["SuppAbbr"].ToString());
            ps.Parameter("RcvStatus", query["RcvStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);
            var ColleteeList = RcvService.GetDynamicListWithPaging(ps, "RcvID");
            
            return ColleteeList;
        }

        public dynamic GetReceivingDetail(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name(ReceivingService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetReceivingDetail");
            ps.Parameter("RcvID", query["RcvID"].ToString());
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            var result = RcvService.GetDynamicListWithPaging(ps);
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
                ParamSP ps = new ParamSP().Name(ReceivingService.strSP);
                ps.Parameter("ActionType", "GetReceivingHeader");
                ps.Parameter("RcvID", id);
                var ARListNew = RcvService.GetDynamicListForDataSet(ps);
                
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
                    form = new Receiving().Extend(new
                    {
                        Currency = "RMB",
                        PriceType = "含税",
                        CFMFlag="N",
                        RcvStatus="收货预录入",
                        InputBy=User.Identity.Name,
                        InputDT = DateTime.Today.ToString("yyyy-MM-dd"),
                        RcvDate=DateTime.Today.ToString("yyyy-MM-dd"),
                        Amount=0,
                        SuppAbbr=""
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
            string RcvId = formData["RcvID"].ToString();
            //更新單頭
            ParamSP ps = new ParamSP().Name(ReceivingService.strSP);
            if (formChange)
            {
                //@RcvNo,@BuyNo,@DO,@RcvDate,@SuppCode,@Contact,@Tel,@Currency,@PriceType,@CFMFlag,@RcvStatus,@Remarks,@InputBy
                formData.Remove("InputDT"); formData.Remove("Amount"); formData.Remove("_changed");
                formData.Remove("StatusDT"); formData.Remove("CFMDate");
                ps.Parameter("ActionType", "SaveReceivingHeader");
                foreach (var item in formData)
                {
                    { ps.Parameter(item.Key, item.Value.ToString()); }
                }

                try
                {
                    if (RcvId == "0")
                    { RcvId = RcvService.StoredProcedureScalar(ps); }
                    else { RcvService.StoredProcedureNoneQuery(ps); }
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
                        ParamSP ps1 = new ParamSP().Name(ReceivingService.strSP);
                        ps1.Parameter("ActionType", "SaveReceivingSub");
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
                                RcvService.StoredProcedureNoneQuery(ps1);
                            }
                            catch (Exception ex)
                            {
                                ZScript.ShowMessage("保存明細："+ex.Message);
                            }
                        }
                    }
                }
            }
            return GetPageData(int.Parse(RcvId));
        }

        [System.Web.Http.HttpPost]
        public dynamic GetDelete(dynamic data)
        {
            ParamSP ps = new ParamSP().Name(ReceivingService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "DeleteReceiving");
            ps.Parameter("RcvID",data["value"].ToString());
            bool bk = RcvService.StoredProcedureNoneQuery(ps);

            return bk;
        }

        public List<dynamic> getReceivingStatus()
        {
            var RoleList = ReceivingService.getReceivingStatus();
            return RoleList;
        }

        public List<dynamic> PriceTypeList()
        {
            var RoleList = ReceivingService.getPriceTypeList();
            return RoleList;
        }

        [System.Web.Http.HttpPost]
        public dynamic GetUpdateAPList(dynamic data)
        {
            ParamSP ps = new ParamSP().Name(ReceivingService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", data["Action"].ToString());
            ps.Parameter("RcvNo", data["RcvNo"].ToString());
            ps.Parameter("InputBy", User.Identity.Name);
            string msg=RcvService.StoredProcedureScalar(ps);
            return new {error=msg };
        }

        [System.Web.Http.HttpPost]
        public dynamic GetcreateExcel(dynamic query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(ReceivingService.strSP);
            
            ps.Parameter("ActionType", "getlist");
            ps.Parameter("RcvNo", query["RcvNo"].ToString());
            ps.Parameter("DO", query["DO"].ToString());
            ps.Parameter("BuyNo", query["BuyNo"].ToString());
            ps.Parameter("SuppAbbr", query["SuppAbbr"].ToString());
            ps.Parameter("RcvStatus", query["RcvStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);

            try
            {
                string fileurl = ReceivingService.ExportExcel(ps);
                return new { success = true, Msg = "", url = fileurl };
            }
            catch (Exception ex)
            {
                string AA = ex.Message;
                return new { success = false, Msg = ex.Message, url = "" };
            }
        }

        //[System.Web.Http.HttpPost]
        //public dynamic GetreloadInvoice(string id)
        //{
        //    ParamSP ps = new ParamSP().Name("ups_AR_ReceiveInvoice");
        //    ps.Parameter("ActionType", "ReloadInvoice");
        //    ps.Parameter("ReceiveInvoiceNo", id);
        //    ps.Parameter("UserCode", User.Identity.Name);
        //    var result = new AR_InvoiceService().GetDynamic(ps);
        //    return result;
        //}
        //[System.Web.Http.HttpPost]
        //public bool GetSendInvoice(string id)
        //{
        //    ParamSP ps = new ParamSP().Name("ups_AR_ReceiveInvoice");
        //    ps.Parameter("ActionType", "SendIdeasInvoice");
        //    ps.Parameter("ReceiveInvoiceNo", id);
        //    ps.Parameter("UserCode", User.Identity.Name);
        //    bool bk = new AR_InvoiceService().StoredProcedureNoneQuery(ps);
        //    return bk;
        //}

        
    }
}
