using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Zephyr.Core;
using ECGroup.Models;
using Newtonsoft.Json.Linq;
using ECGroup.Areas.Mms.Common;
//using Zephyr.Web;

namespace ECGroup.Areas.Mms.Controllers
{
    [MvcMenuFilter(false)]
    public class HomeController : Controller
    {
        //
        // GET: /MMS/Home/
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("~/Home/Default");

            return View();
        }

        public ActionResult LookupMaterial()
        {
            return View();
        }

        public ActionResult LookupSupplier()
        {
            return View();
        }

        public ActionResult RelateCustPN()
        {
            //RelateType: 'RelateCustPN', pnid: PNID,supppn:SuppPN,cdesc:CDesc
            JObject gridData = JObject.Parse(Request["para"].ToString());
            string PNID = gridData["pnid"].ToString();
            string SuppPN = gridData["supppn"].ToString();
            string CDesc = gridData["cdesc"].ToString();

            var model = new
            {
                urls = new
                {
                    query = "/api/mms/home/GetCustPN/"+PNID,
                    edit = "/api/mms/home/Edit",
                    remove = "/api/mms/home/GetDelete/"
                },
                dataSource = new
                {
                    CustCodeList=CustomerService.getCustomerCodeListForCombobox(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                resx = new
                {
                    editSuccess = "保存成功！",
                    deleteSuccess = "數據已刪除！",
                    noneSelect = "請選擇一條數據！"
                },
                idField = "AutoID",
                form = new
                {
                    CustPN = ""
                },
                defaultRow = new
                {
                    AutoID = "0",
                    PNID = PNID,
                    SuppPN = SuppPN,
                    CDesc = CDesc
                },
                setting = new
                {
                    idField = "AutoID",
                    postListFields = new string[] { "AutoID", "PNID","SuppPN","CustPN","CustCode","CustAbbr","CDesc" }
                }
            };
            return View(model);
        }

        public ActionResult RelateProxy()
        {
            //RelateType: 'RelateCustPN', pnid: PNID,supppn:SuppPN,cdesc:CDesc
            JObject gridData = JObject.Parse(Request["para"].ToString());
            string PNID = gridData["pnid"].ToString();
            string SuppPN = gridData["supppn"].ToString();
            string CDesc = gridData["cdesc"].ToString();

            var model = new
            {
                urls = new
                {
                    query = "/api/mms/home/GetProxy/" + PNID,
                    edit = "/api/mms/home/EditProxy",
                    remove = "/api/mms/home/GetDeleteProxy/"
                },
                dataSource = new
                {
                    SupplierCodeList = SupplierService.getSupplierCodeList(""),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                resx = new
                {
                    editSuccess = "保存成功！",
                    deleteSuccess = "數據已刪除！",
                    noneSelect = "請選擇一條數據！"
                },
                idField = "AutoID",
                form = new
                {
                    PNID = PNID
                },
                defaultRow = new
                {
                    AutoID = "0",
                    PNID = PNID,
                    SuppPN = SuppPN,
                    CDesc = CDesc,
                    OrderUnit="PC",
                    MOQ = "1",SupplierCode="",
                    LeadTime = "1"
                },
                setting = new
                {
                    idField = "AutoID", //AutoID,PNID,SuppAbbr,SupplierCode,OrderUnit,MOQ,MinQty,LeadTime
                    postListFields = new string[] { "AutoID", "PNID", "SuppAbbr", "SupplierCode", "OrderUnit", "MOQ", "LeadTime" }
                }
            };
            return View(model);
        }

        public ActionResult UploadFile()
        {
            //RelateType: 'RelateCustPN', pnid: PNID,supppn:SuppPN,cdesc:CDesc
            JObject gridData = JObject.Parse(Request["para"].ToString());
            string BizID = gridData["BizID"].ToString();    //對應附件的關聯ID
            string BizTable = gridData["BizTable"].ToString();//對應附件的關聯表名
            string BizCode = gridData["BizCode"].ToString();

            var model = new
            {
                urls = new
                {
                    query = "/api/mms/home/GetUploadFile/",       //?BizID=" + BizID + "&BizTable=" + BizTable,
                    edit = "/api/mms/home/EditUploadFile",
                    remove = "/api/mms/home/GetDeleteUploadFile/",
                    save = "/mms/home/SaveFile/"
                },
                dataSource = new
                {
                    FileTypeList = MmsHelper.GetFileType(0),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                resx = new
                {
                    editSuccess = "保存成功！",
                    deleteSuccess = "數據已刪除！",
                    noneSelect = "請選擇一條數據！"
                },
                idField = "AutoID",
                form = new
                {
                    BizID = BizID,
                    BizTable=BizTable,
                    BizCode=BizCode,
                    BizType=""
                },
                defaultRow = new
                {
                    AutoID = "0",                    
                    BizID = "0",
                    BizTable = "",
                    BizType = "",
                    FileName = ""
                },
                setting = new
                {
                    idField = "AutoID", 
                    postListFields = new string[] { "AutoID","BizType","FileName"}
                }
            };
            return View(model);
        }

        [System.Web.Mvc.HttpPost]
        public string SaveFile()
        {
            string Msg = "{{\"error\":\"{0}\"}}";
            string strFilesID = Request["UploadID"].ToString();
            string BizID = Request["BizID"].ToString();
            string BizTable = Request["BizTable"].ToString();
            string BizType = Request["BizType"].ToString();    
            string strFilePath = Request.Files[strFilesID].FileName;
            int Size = Request.Files[strFilesID].ContentLength;
            double sizeMB = Size / 1024 / 1024; //以MB為單位的大小

            if (strFilePath.ToString().Trim().Equals(""))
            {
                return String.Format(Msg,"請選擇要上傳的文件！");
            }
            string fileName = strFilePath.Substring(strFilePath.LastIndexOf("\\") + 1);
            string extName = strFilePath.Substring(strFilePath.LastIndexOf(".") + 1);

            if (sizeMB > 3.00)
            {
                return String.Format(Msg, "Excel文件大小不能超過3M!");
            }

            string SaveName = DateTime.Now.ToString("yyyyMMdd") + "-" + DateTime.Now.Ticks.ToString() + "." + extName;
            if (Size > 0)
            {
                string strPath = "~/FJLBS/Doc/" + SaveName;
                try
                {
                    string localPath = System.Web.HttpContext.Current.Server.MapPath(strPath);
                    Request.Files[strFilesID].SaveAs(localPath);

                    //写入文件记录
                    ParamInsert pi = new ParamInsert(); //BizID, BizTable, BizType, FileID, FileName, Creator
                    pi.Insert("DG_Attach");
                    pi.Column("BizID", BizID); pi.Column("BizTable", BizTable); pi.Column("BizType", BizType);
                    pi.Column("FileID", SaveName); pi.Column("FileName", fileName); pi.Column("Creator", User.Identity.Name);

                   int i=new ServiceBase("EC").Insert(pi);
                   return String.Format(Msg, (i > 0) ? "" : "写入DB失败");
                }
                catch (Exception ex)
                {
                    return String.Format(Msg, ex.Message);
                }
            }
            else { return String.Format(Msg, "無效的文件:文件大小為0"); }

        }
    }

    public class HomeApiController : ApiController
    {
        PartNoService service = new PartNoService();
        //弹出材料选择窗口数据
        public dynamic GetMaterial(RequestWrapper request)
        {
            string LookupType = request["LookupType"].ToString();
            string strSP = PartNoService.strSP;
            string BuyNoFlag = "";
            string[] searchType = {"LookupSuppPNForRFQ","LookupSuppPNForBuyOrder","LookupSuppPNForReceiving","LookupCustPNForSaleQuote","LookupCustPNForSaleOrder","LookupCustPNForShiping" };

            if (searchType.Contains(LookupType))
            {
                string actionType = "";

                if (LookupType == "LookupSuppPNForBuyOrder" || LookupType == "LookupSuppPNForRFQ" || LookupType == "LookupSuppPNForReceiving")
                { 
                    actionType="GetPartNoForBuyOrderPopup";
                }
                else if (LookupType == "LookupCustPNForSaleQuote")
                { actionType = "GetPartNoForSaleQuotePopup"; }
                else if (LookupType == "LookupCustPNForSaleOrder")
                { actionType = "GetPartNoForSaleOrderPopup"; }
                else if (LookupType == "LookupCustPNForShiping")
                { 
                    actionType = "GetPartNoForShipingPopup";
                    strSP = ShipingService.strSP;
                }
                else { actionType = ""; }

                string CustPN = request["CustPN"] == null ? "" : request["CustPN"].ToString();
                string SuppPN = request["SuppPN"] == null ? "" : request["SuppPN"].ToString();
                string CDesc = request["CDesc"] == null ? "" : request["CDesc"].ToString();
                string Proxy = request["SuppCode"] == null ? "" : request["SuppCode"].ToString();  //這個是製造商代碼,不是料號
                string PO = request["PO"] == null ? "" : request["PO"].ToString();
                BuyNoFlag = request["BuyNoFlag"] == null ? "N" : request["BuyNoFlag"].ToString();

                ParamSP ps = new ParamSP().Name(strSP);
                ParamSPData psd = ps.GetData();
                psd.PagingCurrentPage = int.Parse(request["page"] == null ? "1" : request["page"].ToString());
                psd.PagingItemsPerPage = int.Parse(request["rows"] == null ? "20" : request["rows"].ToString());
                ps.Parameter("ActionType", actionType);
                ps.Parameter("SuppPN", SuppPN);
                ps.Parameter("CDesc", CDesc);

                if (LookupType == "LookupSuppPNForRFQ"||LookupType == "LookupCustPNForSaleQuote" || LookupType == "LookupCustPNForSaleOrder")
                { ps.Parameter("CustPN", CustPN); }

                if (LookupType == "LookupSuppPNForReceiving")
                { ps.Parameter("BuyNoFlag", BuyNoFlag); ps.Parameter("SupplierCode", Proxy); }

                if (LookupType == "LookupCustPNForShiping")
                {
                    string CustCode = request["CustCode"] == null ? "" : request["CustCode"].ToString();
                    ps.Parameter("CustCode", CustCode);
                    ps.Parameter("CustPN", CustPN);
                    ps.Parameter("PO", PO);
                }

                var kk=service.GetDynamicListWithPaging(ps);
                return kk;
            }
            else
            { return null; }

        }

        public dynamic getSupplierInfoList(RequestWrapper query)
        {
            string type = query["type"] == null ? "" : query["type"].ToString();
            string actionType=(type == "SaleQuote" || type == "SaleOrder" || type == "Shiping")?"lookup-Customer":"lookup-Supplier";
            string SuppCode = query["SuppCode"] == null ? "" : query["SuppCode"].ToString();
            string SuppAbbr = query["SuppAbbr"] == null ? "" : query["SuppAbbr"].ToString();
            ParamSP ps = new ParamSP().Name(SupplierService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", actionType);
            ps.Parameter("value", SuppCode);
            ps.Parameter("text", query["SuppAbbr"].ToString());
            var ColleteeList = service.GetDynamicListWithPaging(ps);
            return ColleteeList;
        }

        //獲取客戶料號關聯
        public dynamic GetCustPN(string id)
        {
            ParamSP ps = new ParamSP().Name(PartNoService.strSP);
            ParamSPData psd = ps.GetData();
            //psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            //psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "GetPartNoCustPN");
            ps.Parameter("PNID", id);

            var CustPNList = service.GetDynamicList(ps);
            return CustPNList;
        }

        //獲取供应商關聯
        public dynamic GetProxy(string id)
        {
            ParamSP ps = new ParamSP().Name(PartNoService.strSP);
            ParamSPData psd = ps.GetData();
            //psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            //psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "GetPartNoProxy");
            ps.Parameter("PNID", id);

            var ProxyList = service.GetDynamicList(ps);
            return ProxyList;
        }

        public dynamic GetUploadFile(RequestWrapper request)
        {
            string BizID = request["BizID"].ToString();
            string BizTable = request["BizTable"].ToString();

            ParamSP ps = new ParamSP().Name("DG_BasicInfo");
            ParamSPData psd = ps.GetData();
            //psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            //psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "GetAttachList");
            ps.Parameter("BizID", BizID);
            ps.Parameter("BizTable", BizTable);

            var attach = service.GetDynamicList(ps);
            return attach;
        }

        
        [System.Web.Http.HttpPost]
        public dynamic EditUploadFile(dynamic data)
        {
            JObject gridData = JObject.Parse(data["list"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name("DG_BasicInfo");
            if (gridChange)
            {
                ps.Parameter("ActionType", "SaveAttach");

                foreach (var item in gridData)
                {
                    string itemKey = item.Key;//deleted,updated,inserted
                    if (item.Value.HasValues)
                    {
                        ps.Parameter("ActionItem", itemKey);
                        JArray ActionData = item.Value as JArray;
                        foreach (var itemDetail in ActionData)
                        {
                            JObject dr = itemDetail as JObject; //AutoID,PNID,SuppPN,CustPN,CustCode,CDesc
                            dr["FileName"] = MmsHelper.HZ(dr["FileName"].ToString(), 1);                          

                            foreach (var drField in dr)
                            {
                                ps.Parameter(drField.Key, drField.Value.ToString());
                            }
                            service.StoredProcedureNoneQuery(ps);
                        }
                    }
                }
            }
            return "OK";
        }

        [System.Web.Http.HttpPost]
        public void GetDeleteUploadFile(dynamic data)
        {
            ParamSP ps = new ParamSP().Name("DG_BasicInfo");
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "DeleteAttach");
            ps.Parameter("CAdd", data["value"].ToString());
            bool bk = service.StoredProcedureNoneQuery(ps);

        }

        //保存客戶料號關聯
        [System.Web.Http.HttpPost]
        public dynamic Edit(dynamic data)
        {
            JObject gridData = JObject.Parse(data["list"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name(PartNoService.strSP);
            if (gridChange)
            {
                ps.Parameter("ActionType", "SavePartNoCustPN");

                foreach (var item in gridData)
                {
                    string itemKey = item.Key;//deleted,updated,inserted
                    if (item.Value.HasValues)
                    {
                        ps.Parameter("ActionItem", itemKey);
                        JArray ActionData = item.Value as JArray;
                        foreach (var itemDetail in ActionData)
                        {
                            JObject dr = itemDetail as JObject; //AutoID,PNID,SuppPN,CustPN,CustCode,CDesc
                            dr["CDesc"] = MmsHelper.HZ(dr["CDesc"].ToString(), 1);                          

                            foreach (var drField in dr)
                            {
                                ps.Parameter(drField.Key, drField.Value.ToString());
                            }
                            service.StoredProcedureNoneQuery(ps);
                        }
                    }
                }
            }
            return "OK";
        }

        [System.Web.Http.HttpPost]
        public dynamic EditProxy(dynamic data)
        {
            JObject gridData = JObject.Parse(data["list"].ToString());
            bool gridChange = (bool)gridData.GetValue("_changed");

            ParamSP ps = new ParamSP().Name(PartNoService.strSP);
            if (gridChange)
            {
                ps.Parameter("ActionType", "SavePartnoProxy");

                foreach (var item in gridData)
                {
                    string itemKey = item.Key;//deleted,updated,inserted
                    if (item.Value.HasValues)
                    {
                        ps.Parameter("ActionItem", itemKey);
                        JArray ActionData = item.Value as JArray;
                        foreach (var itemDetail in ActionData)
                        {
                            JObject dr = itemDetail as JObject; //AutoID,PNID,SuppAbbr,SupplierCode,OrderUnit,MOQ,LeadTime*
                            foreach (var drField in dr)
                            {
                                ps.Parameter(drField.Key, drField.Value.ToString());
                            }
                            service.StoredProcedureNoneQuery(ps);
                        }
                    }
                }
            }
            return "OK";
        }

        [System.Web.Http.HttpPost]
        public void GetDelete(dynamic data)
        {
            ParamSP ps = new ParamSP().Name(PartNoService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "DeleteRelateCustPN");
            ps.Parameter("ESpec", data["value"].ToString());
            bool bk = service.StoredProcedureNoneQuery(ps);

        }

        [System.Web.Http.HttpPost]
        public void GetDeleteProxy(dynamic data)
        {
            ParamSP ps = new ParamSP().Name(PartNoService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "DeleteRelateProxy");
            ps.Parameter("ESpec", data["value"].ToString());
            bool bk = service.StoredProcedureNoneQuery(ps);

        }
        
    }
}
