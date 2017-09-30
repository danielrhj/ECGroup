
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using Zephyr.Utils;
//using System.Web;
//using System.Web.UI;

namespace ECGroup.Models
{
    //此方法已廢
    public class PDFHelper : PDFHelperBase
    {
        private static LocalReport myreport = new LocalReport();

        public static dynamic ExportPDF(PDFHelperBase ParamPDF)
        {
            dynamic myMsg = new ExpandoObject();
            DataSet ds = ParamPDF.reportDataSet;
            string outType, outPutPath = "/FJLBS/TempFiles";
            int oType = ParamPDF.reportFileType;
            if (oType == 0)
            {
                outType = "Excel";
            }
            else if (oType == 1)
            {
                outType = "Word";
            }
            else
            {
                outType = "PDF";
            }
            try
            {
                Warning[] warnings;
                string[] streamids;

                string mimeType,encoding,extension;

                myreport.EnableExternalImages = true;
                myreport.ReportPath = @"Content\page\reports\"+ParamPDF.reportDoc;
                myreport.DataSources.Clear();

                int i = 0;
                foreach(string item in ParamPDF.reportTableName)
                {   
                    myreport.DataSources.Add(new ReportDataSource(item, ds.Tables[i]));
                    i++;
                }
                byte[] bytes = myreport.Render(outType, null, out mimeType, out encoding, out extension,out streamids, out warnings);
                string fileName1 = ParamPDF.fileName + "." + extension;

                File.WriteAllBytes(ZHttp.RootPhysicalPath+outPutPath + fileName1, bytes);
                myMsg.Flag = true; myMsg.Path = outPutPath + fileName1;
                return myMsg;
           
                //這一段只能在aspx頁面中使用
                //Response.Clear();
                //Response.Buffer = true;
                //Response.ContentType = mimeType;
                //Response.AddHeader("content-disposition", "attachment;filename=" + ParamPDF.fileName + "." + extension);
                //Response.BinaryWrite(bytes);

                //Response.Flush();

            }

            catch (Exception ex)
            {
                myMsg.Flag = false; myMsg.Path = "導出文件出錯:" + ex.Message; return myMsg;
            }
        }
        
    }

    public class PDFHelperBase
    {
        public string reportDoc;
        public string[] reportTableName;
        public int reportFileType;
        public string fileName;
        public DataSet reportDataSet;
    }
}