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
    public class CompanyController : Controller
    {
        //
        // GET: /Mms/VMIBill/

        public ActionResult Index()
        {
            var model = new
            {
                urls = new
                {
                    query = "/api/mms/company/Get",
                    edit = "/mms/company/Edit/",
                    remove = "/api/mms/company/Delete/",
                    add = "/api/mms/company/GetH",
                    excel = "/api/mms/company/GetcreateExcel/"
                },
                dataSource = new
                {
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                resx = new
                {
                    editSuccess = "保存成功！",
                    deleteSuccess = "數據已刪除！"
                },
                idField = "AutoID",
                form = new
                {
                    companyCode = ""
                },
                defaultRow = new
                {
                    AutoID="0",
                    OwnerID="",
                    CName = "",
                    CAdd = "",
                    Contact = "",
                    Tel = "",
                    Fax = "",
                    Email = "",
                    Remarks = "",
                    AccountNo = "",
                    BankName = "",
                    Currency = ""
                },
                setting = new
                {
                    idField = "AutoID",
                    postListFields = new string[] { "AutoID", "OwnerID","CName", "CAdd","Contact", "Tel","Fax","Email","Remarks","AccountNo","BankName","Currency"}
                }
            };
            return View(model);
        }

        public ActionResult Edit(string id = "")
        {
            var model = new
            {
                urls = new
                {
                    edit = "/api/mms/company/Edit/",
                    getdata = "/api/mms/company/GetPageData/"
                },
                resx = new
                {
                    editSuccess = "保存成功！"
                },
                dataSource = new
                {
                    pageData = new CompanyApiController().GetPageData(id),
                    buttonsList = new sys_menuService().GetCurrentUserMenuButtonsNew()
                },
                form = new
                {
                    defaults = new Company().Extend(new
                    {
                        AutoID = "0",
                        OwnerID = "",
                        CName = "",
                        CAdd = "",
                        Contact = "",
                        Tel = "",
                        Fax = "",
                        Email = "",
                        Remarks = "",
                        AccountNo = "",
                        Currency = "",
                        BankName = ""
                    }),
                    primaryKeys = new string[] { "OwnerID" },
                    idField = id
                    // AutoID, OwnerID, CName, CAdd, Contact, Tel, Fax, Email, Remarks, AccountNo, BankName, Currency
                }
            };
            return View(model);
        }

    }

    public class CompanyApiController : ApiController
    {
        CompanyService cService = new CompanyService();
        public dynamic Get(RequestWrapper query)
        {
            ParamSP ps = new ParamSP().Name(CompanyService.strSP);
            ParamSPData psd = ps.GetData();
            psd.PagingCurrentPage = int.Parse(query["page"] == null ? "1" : query["page"].ToString());
            psd.PagingItemsPerPage = int.Parse(query["rows"] == null ? "1000" : query["rows"].ToString());
            ps.Parameter("ActionType", "GetOwnerList");
            ps.Parameter("OwnerID", query["companyCode"].ToString());
            var BusInessTypeList = cService.GetDynamicListWithPaging(ps, "OwnerID");
            return BusInessTypeList;
        }

        [System.Web.Http.HttpPost]
        public dynamic Edit(dynamic data)
        {
            JObject formData = JObject.Parse(data["form"].ToString());
            bool formChange = (bool)formData.GetValue("_changed");

            string OwnerID = formData["OwnerID"].ToString();
            ParamSP ps = new ParamSP().Name(CustomerService.strSP);
            //更新單頭
            if (formChange)
            {
                ps.Parameter("ActionType", "SaveCompany");

                formData["CName"] = MmsHelper.HZ(formData["CName"].ToString(), 1);
                formData["CAdd"] = MmsHelper.HZ(formData["CAdd"].ToString(), 1);
                formData["BankName"] = MmsHelper.HZ(formData["BankName"].ToString(), 1);
                formData.Remove("_changed");
                foreach (var item in formData)
                {
                    ps.Parameter(item.Key, item.Value.ToString().Trim());
                }

                if (!string.IsNullOrEmpty(OwnerID))
                { cService.StoredProcedureNoneQuery(ps); }
                else
                { OwnerID = cService.StoredProcedureScalar(ps); }
            }
            return GetPageData(OwnerID);
        }

        public dynamic GetPageData(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                ParamSP ps = new ParamSP().Name(CompanyService.strSP);
                ps.Parameter("ActionType", "GetOwnerList");
                ps.Parameter("OwnerID", id);

                var payeeListNew = cService.GetDynamicListForDataSet(ps);
                var result = new
                {
                    form = payeeListNew[0][0]
                };
                return result;
            }
            else
            {
                var result = new
                {
                    form = new Company().Extend(new
                    {
                        AutoID = "0",
                        OwnerID = "",
                        CName = "",
                        CAdd = "",
                        Contact = "",
                        Tel = "",
                        Fax = "",
                        Email = "",
                        Remarks = "",
                        AccountNo = "",
                        Currency = "",
                        BankName = ""
                    })
                };
                return result;
            }
        }

        [System.Web.Http.HttpDelete]
        public void Delete(string id)
        {
            ParamSP ps = new ParamSP().Name(CompanyService.strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "DeleteOwnerInfo");
            ps.Parameter("AutoID", id);
            bool bk = cService.StoredProcedureNoneQuery(ps);
        }
        public string GetH()
        {
            return "0";
        }
        public List<dynamic> getcompanyCode(string q)    
        {
            var List = CompanyService.getcompanyCode(q);
            return List;
        }

        [System.Web.Http.HttpPost]
        public dynamic GetcreateExcel(dynamic query)
        {
            ParamSP ps = new ParamSP().Name(CompanyService.strSP);
            ps.Parameter("ActionType", "GetOwnerList");
            ps.Parameter("OwnerID", query["companyCode"].ToString());

            try
            {
                string fileurl = CompanyService.ExportExcel(ps);
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
