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
    public class SaleQuoteController : Controller
    {
        //
        // GET: /Mms/AR_Invoice/

        public ActionResult Index()
        {
            var model = new
            {
                
                urls = new
                {
                    query = "/api/mms/SaleQuote/Get/",
                    edit = "/mms/SaleQuote/Edit/", //打開采购单明細  
                    remove = "/api/mms/SaleQuote/GetDelete/",
                    excel = "/api/mms/SaleQuote/GetcreateExcel/"
                },
                resx = new
                {
                    detailTitle = "客户报价单明細",
                    noneSelect = "请先选择一条数据！",
                    editSuccess = "保存成功！",
                    detailTitleRMB = "客户报价单",
                    deleteConfirm = "確定要刪除所選的客户报价单么？",
                    deleteSuccess = "刪除成功",
                },
                form = new
                {
                    QuoteNo = "",
                    CustAbbr = "",
                    CustPN = "",
                    SuppPN = "",
                    QuoteStatus = "",
                    BeginDate = DateTime.Today.AddDays(-180).ToString("yyyy-MM-dd") + " 到 " + DateTime.Today.ToString("yyyy-MM-dd"),
                    

                },
                dataSource = new
                {
                    SaleQuoteStatusList = SaleQuoteService.getQuoteStatusList(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                idField = "QuoteID"
                  
            };            
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = new
            {
                urls = new
                {
                    getdata = "/api/mms/SaleQuote/GetPageData/",
                    edit = "/api/mms/SaleQuote/Edit/",
                    getrowid = "/api/mms/SaleQuote/GetRowId/",
                    getSaleQuoteDetail = "/api/mms/SaleQuote/GetSaleQuoteDetail/",
                    createSaleQuoteExcel = "/api/mms/SaleQuote/GetSaleQuoteExcel/"
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
                    pageData = new SaleQuoteApiController().GetPageData(id),
                    PriceTypeList = new SaleQuoteApiController().PriceTypeList(),
                    CFMFlagList = MmsHelper.GetYON(),
                    CNCYList = MmsHelper.GetCurrencyType(),
                    QuoteStatusList = new SaleQuoteApiController().getQuoteStatusList(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()    
                },
                form = new
                {
                    //QuoteID,QuoteNo,QuoteDate,IncoTerms,CustCode,Contact,Tel,Currency,Remarks,QuoteStatus,PriceType,InputBy,InputDT,CFMFlag
                    defaults = new SaleOrder().Extend(new { QuoteID = id, QuoteDate = DateTime.Today.ToString("yyyy-MM-dd"), Amount = "0", Currency = "RMB", PriceType = "含税", CFMFlag = "N", QuoteStatus = "录入", InputBy = User.Identity.Name, InputDT = "" }),
                    primaryKeys = new string[] { "QuoteID" },
                    idField = id
                },
                tabs = new object[]{
                    new{
                      type = "grid",
                      rowId = "SNO",
                      relationId = "QuoteID", //SNO,QuoteID,CustPN,SuppPN,CSpec,CDesc,Brand,ReplyPrice,Qty,Unit,Amount,VATFlag,SPQ,MOQ,LeadTime,Remarks
                      defaults = new {SNO = "0",QuoteID = id,CustPN = "",SuppPN = "",CDesc = "",CSpec="",ReplyPrice = "0",Qty="0",Unit = "PC",Amount = "0",VATFlag="N",SPQ="0",MOQ="0",LeadTime="",Remarks=""},
                      postFields = new string[] { "SNO","QuoteID","CustPN","SuppPN","CDesc","CSpec","ReplyPrice","Qty","Unit","VATFlag","SPQ","MOQ","LeadTime","Remarks"}
                    }
                }
            };

            return View(model);

        }
    }

    public class SaleQuoteApiController : ApiController
    {
        SaleQuoteService SQService = new SaleQuoteService();
        public dynamic Get(RequestWrapper query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(SaleQuoteService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getlist");
            ps.Parameter("QuoteNo", query["QuoteNo"].ToString());
            ps.Parameter("CustPN", query["CustPN"].ToString()); ps.Parameter("SuppPN", query["SuppPN"].ToString());
            ps.Parameter("CustAbbr", query["CustAbbr"].ToString());
            ps.Parameter("QuoteStatus", query["QuoteStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);    // QuoteNo,CustPN,CustAbbr,QuoteStatus,BeginDate,EndDate  

            var ColleteeList = SQService.GetDynamicListWithPaging(ps, "QuoteID");
            
            return ColleteeList;
        }

        public dynamic GetSaleQuoteDetail(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name(SaleQuoteService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetSaleQuoteDetail");
            ps.Parameter("QuoteID", query["QuoteID"].ToString());
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
                ParamSP ps = new ParamSP().Name(SaleQuoteService.strSP);
                ps.Parameter("ActionType", "GetSaleQuoteHeader");
                ps.Parameter("QuoteID", id);
                var ARListNew = SQService.GetDynamicListForDataSet(ps);
                
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
                    form = new SaleQuote().Extend(new
                    {
                        Currency = "RMB",
                        PriceType = "含税",
                        CFMFlag="N",
                        QuoteStatus = "录入",
                        InputBy=User.Identity.Name,
                        InputDT = DateTime.Today.ToString("yyyy-MM-dd"),
                        QuoteDate=DateTime.Today.ToString("yyyy-MM-dd"),
                        Amount=0,
                        CustAbbr=""
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
            string QuoteId = formData["QuoteID"].ToString();
            //更新單頭
            ParamSP ps = new ParamSP().Name(SaleQuoteService.strSP);
            if (formChange)
            {
                formData.Remove("InputDT"); formData.Remove("Amount"); formData.Remove("_changed");
                ps.Parameter("ActionType", "SaveSaleQuoteHeader");
                foreach (var item in formData)
                {
                    { ps.Parameter(item.Key, item.Value.ToString()); }
                }

                try
                {
                    if (QuoteId == "0")
                    { QuoteId = SQService.StoredProcedureScalar(ps); }
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
                        ParamSP ps1 = new ParamSP().Name(SaleQuoteService.strSP);
                        ps1.Parameter("ActionType", "SaveSaleQuoteSub");
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
            return GetPageData(int.Parse(QuoteId));
        }      

        [System.Web.Http.HttpPost]
        public dynamic GetDelete(dynamic data)
        {
            ParamSP ps = new ParamSP().Name(SaleQuoteService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "DeleteSaleQuote");
            ps.Parameter("QuoteId", data["value"].ToString());
            bool bk = SQService.StoredProcedureNoneQuery(ps);

            return bk;
        }

        public List<dynamic> getQuoteStatusList()
        {
            var RoleList = SaleQuoteService.getQuoteStatusList();
            return RoleList;
        }

        public List<dynamic> PriceTypeList()
        {
            var RoleList = BuyOrderService.getPriceTypeList();
            return RoleList;
        }

        //根据模板生成报价单
        [System.Web.Http.HttpPost]
        public dynamic GetSaleQuoteExcel(string id)
        {
            ParamSP ps = new ParamSP().Name(SaleQuoteService.strSP);
            ps.Parameter("ActionType", "CreateSaleQuoteExcel");
            ps.Parameter("QuoteID", id);

            try
            {
                string fileurl = SaleQuoteService.ExportSQExcel(ps);
                return new { success = true, Msg = "", url = fileurl };
            }
            catch (Exception ex)
            {
                string AA = ex.Message;
                return new { success = false, Msg = ex.Message, url = "" };
            }
        }

        //根据条件导出报价清单
        [System.Web.Http.HttpPost]
        public dynamic GetcreateExcel(dynamic query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(SaleQuoteService.strSP);
            ps.Parameter("ActionType", "getlist"); ps.Parameter("ActionItem","excel");
            ps.Parameter("QuoteNo", query["QuoteNo"].ToString());
            ps.Parameter("CustPN", query["CustPN"].ToString()); ps.Parameter("SuppPN", query["SuppPN"].ToString());
            ps.Parameter("CustAbbr", query["CustAbbr"].ToString());
            ps.Parameter("QuoteStatus", query["QuoteStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2); 

            try
            {
                string fileurl = SaleQuoteService.ExportExcel(ps);
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
