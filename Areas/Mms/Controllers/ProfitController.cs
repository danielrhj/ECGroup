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
    public class ProfitController : Controller
    {
        //
        // GET: /Mms/AR_Invoice/

        public ActionResult Index()
        {
            var model = new
            {
                
                urls = new
                {
                    query = "/api/mms/Profit/Get/",
                    excel = "/api/mms/Profit/GetcreateExcel/" //导出excel                   
                   
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
                    ShipingStatus = "",
                    BeginDate = DateTime.Today.AddDays(-20).ToString("yyyy-MM-dd") + " 到 " + DateTime.Today.ToString("yyyy-MM-dd")                    

                },
                dataSource = new
                {
                    ShipingStatusList = ProfitService.getShipingStatusList(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                idField = "POID"
                  
            };            
            return View(model);
        }

        public ActionResult Stock()
        {
            var model = new
            {

                urls = new
                {
                    query = "/api/mms/Profit/GetStockList/",
                    excel = "/api/mms/Profit/excelSimple/" //导出excel                   

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
                    BuyNo = "",
                    Supplier = "",
                    SuppPN = "",
                    CustPN = "",
                    CDesc = "",
                    BeginDate = DateTime.Today.AddDays(-20).ToString("yyyy-MM-dd") + " 到 " + DateTime.Today.ToString("yyyy-MM-dd")

                },
                dataSource = new
                {                    
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                idField = "BuyNo"

            };
            return View(model);
        }
        
    }

    public class ProfitApiController : ApiController
    {
        ProfitService SOService = new ProfitService();
        public dynamic Get(RequestWrapper query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(ProfitService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getlist");
            ps.Parameter("PO", query["PO"].ToString());
            ps.Parameter("Customer", query["Customer"].ToString());
            ps.Parameter("POStatus", query["ShipingStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);    // PO,Customer,CustPN,SaleOrderStatus,

            var ColleteeList = SOService.GetDynamicListWithPaging(ps);
            
            return ColleteeList;
        }

        public dynamic GetStockList(RequestWrapper query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(ProfitService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getStocklist");
            ps.Parameter("BuyNo", query["BuyNo"].ToString());
            ps.Parameter("Supplier", query["Supplier"].ToString());
            ps.Parameter("SuppPN", query["SuppPN"].ToString());
            ps.Parameter("CustPN", query["CustPN"].ToString());
            ps.Parameter("CDesc", query["CDesc"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);    // PO,Customer,CustPN,SaleOrderStatus,

            var ColleteeList = SOService.GetDynamicListWithPaging(ps);

            return ColleteeList;
        }      
        
        public dynamic getBuyCostDetail(RequestWrapper data)
        {
            ParamSP ps = new ParamSP().Name(ProfitService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(data["page"] == null ? "1" : data["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(data["rows"] == null ? "20" : data["rows"].ToString());
            ps.Parameter("ActionType", "getBuyCostDetail");
            ps.Parameter("PO", data["PO"].ToString());
            ps.Parameter("CustPN", data["CustPN"].ToString());
            ps.Parameter("SuppPN", data["SuppPN"].ToString());
            var result = SOService.GetDynamicListWithPaging(ps);
            return result;
        }

        [System.Web.Http.HttpPost]
        public dynamic GetcreateExcel(dynamic query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            string type = query["type"].ToString() == "simple" ? "CreateProfitExcelSimple" : "CreateProfitExcelFull";


            ParamSP ps = new ParamSP().Name(ProfitService.strSP);
            ps.Parameter("ActionType", type);
            ps.Parameter("PO", query["PO"].ToString());
            ps.Parameter("Customer", query["Customer"].ToString());
            ps.Parameter("POStatus", query["ShipingStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2); 

            try
            {
                string fileurl = ProfitService.ExportProfitExcel(ps,type);
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
