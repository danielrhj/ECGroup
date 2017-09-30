using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zephyr.Core;
//using Zephyr.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections.Specialized;
//using Zephyr.Web;
//using Zephyr.Web.Areas.Mms.Common;
using ECGroup.Areas.Mms.Common;
using ECGroup.Models;

namespace ECGroup.Controllers
{
    [MvcMenuFilter(false)]
    public class PluginsController : Controller
    {
        //
        // GET: /Plugins/

        public ActionResult Lookup()
        {
            return View();
        }

        public ActionResult GetLookupData(string index)
        {
            var type = Request.QueryString["_lookupType"].Split('.');
            string lookupType = type[0].ToString();
            var requestData = new NameValueCollection(Request.QueryString);

            if (lookupType == "supplier")
            {
                string text = Request.QueryString["text"] == null ? "" : Request.QueryString["text"].ToString();
                string value = Request.QueryString["value"] == null ? "" : Request.QueryString["value"].ToString();

                ParamSP ps = new ParamSP().Name(SupplierService.strSP);
                ParamSPData psd = ps.GetData();
                psd.PagingCurrentPage = 1;
                psd.PagingItemsPerPage = 20;
                ps.Parameter("ActionType", "lookup-"+lookupType);
                ps.Parameter("text", text);
                ps.Parameter("value", value);
                var data = SupplierService.getSupplierInfoList(ps);
                var json = JsonConvert.SerializeObject(data);
                return Content(json, "application/json");
            }

            else //(type.Length <= 2)
            {
                var xmlPath = string.Format("~/Views/Shared/Xml/{0}.xml", type[type.Length - 1]);
                if (type.Length > 1)
                    xmlPath = string.Format("~/Areas/{0}/Views/Shared/Xml/{1}.xml", type);

                var das = RequestWrapper.Instance().LoadSettingXml(xmlPath);
                var query = das.SetRequestData(requestData).ToParamQuery();

                var valueField = das["_valueFeild"];
                if (!string.IsNullOrEmpty(valueField))
                    query.ClearWhere().AndWhere(das.getFieldName(valueField, true), string.Format("'{0}'", das[valueField].Replace(",", "','")), Cp.In);

                var service = das.GetService();
                var data = service.GetDynamicListWithPaging(query);
                var json = JsonConvert.SerializeObject(data);
                return Content(json, "application/json");
            }
            
        }
        
    }
}
