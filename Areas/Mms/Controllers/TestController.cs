using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zephyr.Utils;
using System.Dynamic;
using Newtonsoft.Json;
using System.Web.Http;
using Microsoft.VisualBasic;
using ECGroup.Areas.Mms.Common;

namespace ECGroup.Areas.Mms.Controllers
{
    public class TestController : Controller
    {
        //
        // GET: /Mms/Test/FL
        public ActionResult Index()
        {
            var model = new
            {
                form = new {
                msg=""
                }
            };
            return View(model);
        }

        public ActionResult FL()
        {
            return View();
        }      

        public string uploadTest()
        {
            dynamic resultA = new ExpandoObject();
            resultA.success = false;
            resultA.error = "";

            string strFilesID = "qqfile";
            string strFileName = Request.Files[strFilesID].FileName;
            string fileExt = strFileName.Substring(strFileName.LastIndexOf(".") + 1).ToLower();

            int Size = Request.Files[strFilesID].ContentLength;
            double sizeMB = Size / 1000000.00; //以MB為單位的大小

            if (sizeMB > 3.00)
            {
                resultA.error = "圖片文件大小不能超過3M!";
            }
            else
            {
                string SaveName = DateTime.Now.ToString("yyyyMMdd") + "_" + Request["qqfilename"].ToString();
                if (Size > 0)
                {
                    try
                    {
                        string strPath = Server.MapPath("~/FJLBS/FormalPreviewNo/" + SaveName);
                        ZFiles.DeleteFiles(strPath);
                        Request.Files[strFilesID].SaveAs(strPath);
                        resultA.success = true;
                        //return "{'success':'" + SaveName + "'}";
                    }
                    catch (Exception ex)
                    {
                        resultA.error = ex.Message;
                    }
                }
                else
                {
                    resultA.error = "無效的文件大小！";
                }
            }
            string kk = JsonConvert.SerializeObject(resultA);
            return kk;
        }

    }

    public class TestApiController : ApiController
    {
        [System.Web.Http.HttpPost]
        public dynamic EditFJ(dynamic data)
        {
            string id = data.key.Value;           
            string AA = MmsHelper.HZ(id, 0);
            dynamic ret = new ExpandoObject();
            ret.msg = AA;

            return ret;
        }

        [System.Web.Http.HttpPost]
        public dynamic EditNum(dynamic data)
        {
            string id = data.key.Value;           
            string ch = MoneyToString.GetCnString(id);
            string en = MoneyToString.GetEnString(id);
            dynamic ret = new ExpandoObject();
            ret.msg = ch;
            ret.msg2 = en;
            return ret;
        }
        
    }
}
