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
    public class EcomController : Controller
    {
        //
        // GET: /Mms/AR_Invoice/

        public ActionResult Index()
        {
            var model = new
            {
                
                urls = new
                {
                    query = "/api/mms/Ecom/Get/",
                    edit = "/mms/Ecom/Edit/",              
                    remove = "/api/mms/Ecom/GetDelete/"
                },
                resx = new
                {
                    detailTitle = "营收明細",
                    noneSelect = "请先选择一条数据！",
                    editSuccess = "保存成功！",
                    detailTitleRMB = "出库单",
                    deleteConfirm = "確定要刪除所選的出库单么？",
                    deleteSuccess = "刪除成功",
                },
                form = new
                {
                    SaleNo = "",
                    SaleMonth1 = "",   
                    SaleMonth2 = ""  

                },
                dataSource = new
                {                   
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                idField = "EcomID"
                  
            };            
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = new
            {
                urls = new
                {
                    getdata = "/api/mms/Ecom/GetPageData/",
                    edit = "/api/mms/Ecom/Edit/",
                    getrowid = "/api/mms/Ecom/GetRowId/",
                    getEcomDetail = "/api/mms/Ecom/GetEcomDetail/",
                    getEPNPrice = "/api/mms/Ecom/GetEPNPrice/",
                    getExcel = "/api/mms/Ecom/GetMonthSale/",
                    //sendIdeas = "/api/mms/AR_Invoice/GetSendInvoice/"
                },
                resx = new
                {
                    rejected = "已撤消修改！",
                    editSuccess = "保存成功！"
                },

                dataSource = new
                {
                    pageData = new EcomApiController().GetPageData(id),
                    CNCYList = MmsHelper.GetCurrencyType(),
                    EcomPNList = EcomService.GetPNList(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()    
                },
                form = new
                {
                    //EcomID,SaleNo,SaleMonth,ExRate,SaleTotal, SaleNet,SaleCNCY, AdsCost, ManCost, OtherCost, Currency, OffSet,SettleCNCY,Remark, Plat,InputBy,InputDT
                    defaults = new Ecom().Extend(new { EcomID = id,SaleMonth=DateTime.Today.ToString("yyyyMM"), SaleCNCY = "JPY", SettleCNCY = "RMB", InputBy = User.Identity.Name}),
                    primaryKeys = new string[] { "EcomID" },
                    idField = id
                },
                tabs = new object[]{
                    new{
                      type = "grid",
                      rowId = "SNO",
                      relationId = "ShipID", //SNO,EcomID,PNID,PN,Qty,Price,LogCost,PKGCost,OtherCost,Currency,Remark
                      defaults = new {SNO = "0",EcomID = id,PNID = "",PN="",Qty="1",Price = "0",LogCost = "0",PKGCost="0",OtherCost = "0",Currency = "RMB",Remark = ""},
                      postFields = new string[] { "SNO","EcomID","PNID","PN","Qty","Price","LogCost","PKGCost","OtherCost","Currency","Remark"}
                    }
                }
            };

            return View(model);

        }

    }

    public class EcomApiController : ApiController
    {
        EcomService SPService = new EcomService();
        public dynamic Get(RequestWrapper query)
        {
            string BeginDate = query["SaleMonth1"].ToString();
            string EndDate = query["SaleMonth2"].ToString();
            object ColleteeList="";

            ParamSP ps = new ParamSP().Name(EcomService.strSP);
            ParamSPData psd = ps.GetData();
            bool noSizeLimit = query["noSizeLimit"] == null ? false : bool.Parse(query["noSizeLimit"]);
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getlist");
            ps.Parameter("SaleNo", query["SaleNo"].ToString());
            ps.Parameter("BeginDate", BeginDate);
            ps.Parameter("EndDate", EndDate);    // PO,Customer,CustPN,SaleOrderStatus,

            if (noSizeLimit) //excel
            {
                ColleteeList = SPService.GetDynamicList(ps);
            }
            else {
                ColleteeList = SPService.GetDynamicListWithPaging(ps, "EcomID");
            }
            return ColleteeList;
        }

        public dynamic GetEcomDetail(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name(EcomService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetEcomDetail");
            ps.Parameter("EcomID", query["EcomID"].ToString());
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            var result = SPService.GetDynamicListWithPaging(ps);
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
                ParamSP ps = new ParamSP().Name(EcomService.strSP);
                ps.Parameter("ActionType", "GetEcomHeader");
                ps.Parameter("EcomID ", id);
                var ARListNew = SPService.GetDynamicListForDataSet(ps);
                
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
                    form = new Ecom().Extend(new
                    {
                        SaleMonth=DateTime.Today.ToString("yyyyMM"),
                        SaleCNCY = "JPY",
                        SettleCNCY = "RMB",
                        InputBy=User.Identity.Name

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
            string EcomID = formData["EcomID"].ToString();
            //更新單頭
            ParamSP ps = new ParamSP().Name(EcomService.strSP);
            if (formChange)
            {
                formData.Remove("InputDT"); formData.Remove("Amount"); formData.Remove("_changed");
                formData.Remove("MainCost"); formData.Remove("LogCost");
                ps.Parameter("ActionType", "SaveEcomHeader");
                foreach (var item in formData)
                {
                    { ps.Parameter(item.Key, item.Value.ToString()); }
                }

                try
                {
                    if (EcomID == "0")
                    { EcomID = SPService.StoredProcedureScalar(ps); }
                    else { SPService.StoredProcedureNoneQuery(ps); }
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
                        ParamSP ps1 = new ParamSP().Name(EcomService.strSP);
                        ps1.Parameter("ActionType", "SaveEcomSub");
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
                                SPService.StoredProcedureNoneQuery(ps1);
                            }
                            catch (Exception ex)
                            {
                                ZScript.ShowMessage("保存明細："+ex.Message);
                            }
                        }
                    }
                }
            }
            return GetPageData(int.Parse(EcomID));
        }

        [System.Web.Http.HttpPost]
        public dynamic GetMonthSale(string id)
        {
            ParamSP ps = new ParamSP().Name(EcomService.strSP);
            ps.Parameter("ActionType", "CreateMonthSaleExcel");
            ps.Parameter("EcomID", id);

            try
            {
                string fileurl = EcomService.ExportMonthSaleExcel(ps);
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
            ParamSP ps = new ParamSP().Name(EcomService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "DeleteEcom");
            ps.Parameter("EcomId", data["value"].ToString());
            bool bk = SPService.StoredProcedureNoneQuery(ps);

            return bk;
        }  

        [System.Web.Http.HttpPost]
        public dynamic GetEPNPrice(dynamic data)
        {
            ParamSP ps = new ParamSP().Name(PartNoService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetEPNPriceByPNID");
            ps.Parameter("PNID", data["pnid"].ToString());
            var bk = SPService.GetDynamic(ps);

            return bk;
        }  
        
    }
}
