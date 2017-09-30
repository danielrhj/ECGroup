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
    public class RFQController : Controller
    {
        //
        // GET: /Mms/AR_Invoice/

        public ActionResult Index()
        {
            var model = new
            {
                
                urls = new
                {
                    query = "/api/mms/RFQ/Get/",
                    edit = "/mms/RFQ/Edit/", //打開采购单明細   
                    remove = "/api/mms/RFQ/GetDelete/",
                    excel = "/api/mms/RFQ/GetcreateExcel/"
                },
                resx = new
                {
                    detailTitle = "供应商报价单明細",
                    noneSelect = "请先选择一条数据！",
                    editSuccess = "保存成功！",
                    detailTitleRMB = "供应商报价单",
                    deleteConfirm = "確定要刪除所選的供应商报价单么？",
                    deleteSuccess = "刪除成功",
                },
                form = new
                {
                    RFQNo = "",
                    SuppAbbr = "",
                    CustPN = "",
                    SuppPN = "",
                    Status = "",
                    BeginDate = DateTime.Today.AddDays(-20).ToString("yyyy-MM-dd") + " 到 " + DateTime.Today.ToString("yyyy-MM-dd")                   

                },
                dataSource = new
                {
                    RFQStatusList = RFQService.getRFQStatusList(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                idField = "RFQID"
                  
            };            
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = new
            {
                urls = new
                {
                    getdata = "/api/mms/RFQ/GetPageData/",
                    edit = "/api/mms/RFQ/Edit/",
                    getrowid = "/api/mms/RFQ/GetRowId/",
                    getRFQDetail = "/api/mms/RFQ/GetRFQDetail/",
                    //createSaleQuoteExcel = "/api/mms/RFQ/GetSaleQuoteExcel/"
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
                    pageData = new RFQApiController().GetPageData(id),
                    CNCYList = MmsHelper.GetCurrencyType(),
                    RFQStatusList = new RFQApiController().getQuoteStatusList(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()    
                },
                form = new
                {
                    //RFQID, RFQNo, RFQDate,  SuppCode, SuppAbbr, Contact, Tel,  Currency, Remarks,InputBy, InputDT, Status, ApproveDT
                    defaults = new RFQ().Extend(new { RFQID = id, RFQDate = DateTime.Today.ToString("yyyy-MM-dd"), Currency = "RMB", Status = "录入", InputBy = User.Identity.Name, InputDT = DateTime.Today.ToString("yyyy-MM-dd"), ApproveDT = DateTime.Today.ToString("yyyy-MM-dd") }),
                    primaryKeys = new string[] { "RFQID" },
                    idField = id
                },
                tabs = new object[]{
                    new{
                      type = "grid",
                      rowId = "SNO",
                      relationId = "RFQID", //SNO, RFQID,  SuppPN, CDesc, Brand, RFQPriceT, RFQPrice, LeadTime,MinQty, MinUnit, MinQtyUnit, MOQ, Remarks, TaxRate
                      defaults = new {SNO = "0",RFQID = id,SuppPN = "",CDesc = "",Brand="",RFQPriceT = "0",RFQPrice = "0",LeadTime="",Qty="0",MinQty="1",MinUnit = "PC",MinQtyUnit="PC",MOQ="1",Remarks="",TaxRate="0"},
                      postFields = new string[] { "SNO","RFQID","SuppPN","CDesc","Brand","RFQPriceT","RFQPrice","LeadTime","Qty","MinQty","MinUnit","MinQtyUnit","TaxRate","MOQ","Remarks"}
                    }
                }
            };

            return View(model);

        }
     
    }

    public class RFQApiController : ApiController
    {
        RFQService SQService = new RFQService();
        public dynamic Get(RequestWrapper query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(RFQService.strSP);
            //ParamSPData psd = ps.GetData();
            //psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            //psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getlist");
            ps.Parameter("RFQNo", query["RFQNo"].ToString());
            ps.Parameter("CustPN", query["CustPN"].ToString()); ps.Parameter("SuppPN", query["SuppPN"].ToString());
            ps.Parameter("SuppAbbr", query["SuppAbbr"].ToString());
            ps.Parameter("Status", query["Status"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);    // QuoteNo,CustPN,CustAbbr,QuoteStatus,BeginDate,EndDate  

            var ColleteeList = SQService.GetDynamicList(ps, "RFQID");            
            return ColleteeList;
        }

        public dynamic GetRFQDetail(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name(RFQService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetRFQDetail");
            ps.Parameter("RFQID", query["RFQID"].ToString());
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            var result = SQService.GetDynamicListWithPaging(ps);
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
                ParamSP ps = new ParamSP().Name(RFQService.strSP);
                ps.Parameter("ActionType", "GetRFQHeader");
                ps.Parameter("RFQID", id);
                var ARListNew = SQService.GetDynamicListForDataSet(ps);
                
                var result = new
                {
                    form = ARListNew[0][0],
                    tab0 = ARListNew[1],
                    footer = new { SuppPN = "合计:", Qty = "1200" }
                };

                return result;
            }
            else
            {
                var result = new
                {
                    form = new RFQ().Extend(new
                    {
                        Currency = "RMB",
                        Status = "录入",
                        InputBy=User.Identity.Name,
                        InputDT = DateTime.Today.ToString("yyyy-MM-dd"),
                        ApproveDT = DateTime.Today.ToString("yyyy-MM-dd"),
                        RFQDate=DateTime.Today.ToString("yyyy-MM-dd"),
                        SuppCode="",
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
            string RFQID = formData["RFQID"].ToString();
            //更新單頭
            ParamSP ps = new ParamSP().Name(RFQService.strSP);
            if (formChange)
            {
                formData.Remove("InputDT"); formData.Remove("Amount"); formData.Remove("_changed");
                ps.Parameter("ActionType", "SaveRFQHeader");
                foreach (var item in formData)
                {
                    { ps.Parameter(item.Key, item.Value.ToString()); }
                }

                try
                {
                    if (RFQID == "0")
                    { RFQID = SQService.StoredProcedureScalar(ps); }
                    else { SQService.StoredProcedureNoneQuery(ps); }
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
                        ParamSP ps1 = new ParamSP().Name(RFQService.strSP);
                        ps1.Parameter("ActionType", "SaveRFQSub");
                        ps1.Parameter("ActionItem", itemKey);

                        JArray ActionData = item.Value as JArray; //注意這個數組的每個元素對應表格的一行編輯的記錄
                        foreach (var itemDetail in ActionData)
                        {
                            JObject dr = itemDetail as JObject;
                            dr["CDesc"] = MmsHelper.HZ(dr["CDesc"].ToString(), 1);
                            
                            foreach (var drField in dr)
                            {
                                ps1.Parameter(drField.Key, drField.Value.ToString());
                            }

                            try
                            {
                                SQService.StoredProcedureNoneQuery(ps1);
                            }
                            catch (Exception ex)
                            {
                                ZScript.ShowMessage("保存明細："+ex.Message);
                            }
                        }
                    }
                }
            }
            return GetPageData(int.Parse(RFQID));
        }

        [System.Web.Http.HttpPost]
        public dynamic GetcreateExcel(dynamic query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(RFQService.strSP);           
            ps.Parameter("ActionType", "getlist"); ps.Parameter("ActionItem", query["type"].ToString());
            ps.Parameter("RFQNo", query["RFQNo"].ToString());
            ps.Parameter("CustPN", query["CustPN"].ToString()); ps.Parameter("SuppPN", query["SuppPN"].ToString());
            ps.Parameter("SuppAbbr", query["SuppAbbr"].ToString());
            ps.Parameter("Status", query["Status"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);

            try
            {
                string fileurl = RFQService.ExportExcel(ps);
                return new { success = true, Msg = "", url = fileurl };
            }
            catch (Exception ex)
            {
                string AA = ex.Message;
                return new { success = false, Msg = ex.Message, url = "" };
            }  
        
        }

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
            ParamSP ps = new ParamSP().Name(RFQService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "DeleteRFQ");
            ps.Parameter("CustPN", data["value"].ToString());
            bool bk = SQService.StoredProcedureNoneQuery(ps);

            return bk;
        }

        public List<dynamic> getQuoteStatusList()
        {
            var RoleList = RFQService.getRFQStatusList();
            return RoleList;
        }
    }
}
