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
    public class ShipingController : Controller
    {
        //
        // GET: /Mms/AR_Invoice/

        public ActionResult Index()
        {
            var model = new
            {
                
                urls = new
                {
                    query = "/api/mms/Shiping/Get/",
                    edit = "/mms/Shiping/Edit/", //打開出库单明細 
                    remove = "/api/mms/Shiping/GetDelete/",
                    excel = "/api/mms/Shiping/GetcreateExcel/"
                    
                },
                resx = new
                {
                    detailTitle = "出库单明細",
                    noneSelect = "请先选择一条数据！",
                    editSuccess = "保存成功！",
                    detailTitleRMB = "出库单",
                    deleteConfirm = "確定要刪除所選的出库单么？",
                    deleteSuccess = "刪除成功",
                },
                form = new
                {
                    ShipNo = "",
                    Customer = "",
                    CustPN = "",
                    ShipStatus = "",
                    BeginDate = DateTime.Today.AddDays(-180).ToString("yyyy-MM-dd") + " 到 " + DateTime.Today.ToString("yyyy-MM-dd"),
                    

                },
                dataSource = new
                {
                    ShipingStatusList = ShipingService.getShipingStatusList(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                idField = "ShipID"
                  
            };            
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = new
            {
                urls = new
                {
                    getdata = "/api/mms/Shiping/GetPageData/",
                    edit = "/api/mms/Shiping/Edit/",
                    getrowid = "/api/mms/Shiping/GetRowId/",
                    getShipDetail = "/api/mms/Shiping/GetShipDetail/",
                    updateInventory = "/api/mms/Shiping/GetupdateInventory/",
                    createShipingExcel = "/api/mms/Shiping/GetShipingExcel/"
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
                    pageData = new ShipingApiController().GetPageData(id),
                    IncotermList = ShipingService.GetIncotermList(),
                    CFMFlagList = MmsHelper.GetYON(),
                    CNCYList = MmsHelper.GetCurrencyType(),
                    ShipingStatusList = ShipingService.getShipingStatusList(),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()    
                },
                form = new
                {
                    //ShipID,ShipNo, ShipDate, BLNo, Customer, Incoterms,Currency, Plts, Ctns, CustCode, Consignee, Address, Contact,Tel, Destination,ShipStatus,Remark, CFMFlag, InputBy, InputDT
                    defaults = new Shiping().Extend(new { ShipID = id, Incoterms = "现金交易", ShipDate = DateTime.Today.ToString("yyyy-MM-dd"),Plts="0",Ctns="1", Amount = "0", Currency = "RMB", CFMFlag = "N", ShipStatus = "录入", InputBy = User.Identity.Name, InputDT = "" }),
                    primaryKeys = new string[] { "ShipID" },
                    idField = id
                },
                tabs = new object[]{
                    new{
                      type = "grid",
                      rowId = "SNO",
                      relationId = "ShipID", //SNO, ShipID, PO, CustPN, SuppPN, CDesc, EDesc, CSpec, Qty, Unit, UnitPrice
                      defaults = new {SNO = "0",ShipID = id,CustPN = "",PO="",SuppPN = "",CDesc = "",CSpec="",UnitPrice = "0",Qty="0",Unit = "PC",Amount = "0"},
                      postFields = new string[] { "SNO","ShipID","PO","CustPN","SuppPN","CDesc","CSpec","UnitPrice","Qty","Unit"}
                    }
                }
            };

            return View(model);

        }
    }

    public class ShipingApiController : ApiController
    {
        ShipingService SPService = new ShipingService();
        public dynamic Get(RequestWrapper query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');

            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(ShipingService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "20" : query["rows"].ToString());
            ps.Parameter("ActionType", "getlist");
            ps.Parameter("ShipNo", query["ShipNo"].ToString());
            ps.Parameter("CustPN", query["CustPN"].ToString());
            ps.Parameter("Customer", query["Customer"].ToString());
            ps.Parameter("ShipStatus", query["ShipStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2);    // PO,Customer,CustPN,SaleOrderStatus,

            var ColleteeList = SPService.GetDynamicListWithPaging(ps, "ShipID");
            
            return ColleteeList;
        }

        public dynamic GetShipDetail(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name(ShipingService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetShipingDetail");
            ps.Parameter("ShipID", query["ShipID"].ToString());
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
                ParamSP ps = new ParamSP().Name(ShipingService.strSP);
                ps.Parameter("ActionType", "GetShipingHeader");
                ps.Parameter("ShipID", id);
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
                    form = new Shiping().Extend(new
                    {
                        Currency = "RMB",
                        Incoterms = "现金交易",
                        CFMFlag="N",
                        ShipStatus = "录入",
                        InputBy=User.Identity.Name,
                        InputDT = DateTime.Today.ToString("yyyy-MM-dd"),
                        ShipDate=DateTime.Today.ToString("yyyy-MM-dd"),
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
            string ShipID = formData["ShipID"].ToString();
            //更新單頭
            ParamSP ps = new ParamSP().Name(ShipingService.strSP);
            if (formChange)
            {
                formData.Remove("InputDT"); formData.Remove("Amount"); formData.Remove("_changed");
                ps.Parameter("ActionType", "SaveShipingHeader");
                foreach (var item in formData)
                {
                    { ps.Parameter(item.Key, item.Value.ToString()); }
                }

                try
                {
                    if (ShipID == "0")
                    { ShipID = SPService.StoredProcedureScalar(ps); }
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
                        ParamSP ps1 = new ParamSP().Name(ShipingService.strSP);
                        ps1.Parameter("ActionType", "SaveShipingSub");
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
            return GetPageData(int.Parse(ShipID));
        }

        [System.Web.Http.HttpPost]
        public dynamic GetShipingExcel(string id)
        {
            ParamSP ps = new ParamSP().Name(ShipingService.strSP);
            ps.Parameter("ActionType", "CreateShipingExcel");
            ps.Parameter("ShipID", id);

            try
            {
                string fileurl = ShipingService.ExportSHExcel(ps);
                return new { success = true, Msg = "", url = fileurl };
            }
            catch (Exception ex)
            {
                string AA = ex.Message;
                return new { success = false, Msg = ex.Message, url = "" };
            }
        }

        [System.Web.Http.HttpPost]
        public dynamic GetupdateInventory(dynamic data)
        {
            string ShipID = data["ShipID"].ToString();
            string Action = data["Action"].ToString();

            ParamSP ps = new ParamSP().Name(ShipingService.strSP);
            ps.Parameter("ActionType", Action);
            ps.Parameter("ShipID", ShipID);
            ps.Parameter("EditBy", User.Identity.Name);
            var result = SPService.GetDynamic(ps);
            return result;
        }

        [System.Web.Http.HttpPost]
        public dynamic GetDelete(dynamic data)
        {
            ParamSP ps = new ParamSP().Name(ShipingService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "DeleteShiping");
            ps.Parameter("ShipId", data["value"].ToString());
            bool bk = SPService.StoredProcedureNoneQuery(ps);

            return bk;
        }

        [System.Web.Http.HttpPost]
        public dynamic GetcreateExcel(dynamic query)
        {
            string[] daterow1 = query["BeginDate"].ToString().Split('到');
            string sendDate1 = daterow1 != null ? daterow1[0].Trim() : "";
            string sendDate2 = sendDate1 == "" ? "" : (daterow1.Length > 1 ? daterow1[1].Trim() : "");
            ParamSP ps = new ParamSP().Name(ShipingService.strSP);
            
            ps.Parameter("ActionType", "getlist");
            ps.Parameter("ShipNo", query["ShipNo"].ToString());
            ps.Parameter("CustPN", query["CustPN"].ToString());
            ps.Parameter("Customer", query["Customer"].ToString());
            ps.Parameter("ShipStatus", query["ShipStatus"].ToString());
            ps.Parameter("BeginDate", sendDate1);
            ps.Parameter("EndDate", sendDate2); 

            try
            {
                string fileurl = ShipingService.ExportExcel(ps);
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
