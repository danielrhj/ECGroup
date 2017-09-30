using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using Zephyr.Core;

namespace ECGroup.Models
{
    [Module("EC")]
    public class SaleQuoteService : ServiceBase<SaleQuote>
    {
        public static string strSP = "DG_QuoteHeader_SP";
        internal IList<SaleQuote> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<SaleQuote> myList = db.Select<SaleQuote>(strSQL).QueryMany();

            return myList;
        }

        public static List<dynamic> getQuoteStatusList()
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "getQuoteStatusList");
            List<dynamic> resultA = new SaleQuoteService().GetDynamicList(ps);

            return resultA;
        }

        internal static string ExportSQExcel(ParamSP ps)
        {
            string fileTemp = Zephyr.Utils.ZHttp.RootPhysicalPath + @"Content\Template\SaleQuote.xlsx";
            DataSet ds = new SaleQuoteService().StoredProcedureDS(ps);
            EPPlusNew myExcel = new EPPlusNew(fileTemp);

            DataRow dtHeaderRow = ds.Tables[0].Rows[0]; //QuoteID,QuoteNo,CustName,CustAdd,Contact,Tel,CellNo,PriceType,Currency
            DataTable dtSub = ds.Tables[1];

            string QuoteNo = dtHeaderRow["QuoteNo"].ToString();
            string Currency = dtHeaderRow["Currency"].ToString();
            string filename = QuoteNo + ".xlsx";
            string fileURL = "/FJLBS/TempFiles/" + filename;  // 格式： /FJLBS/TempFiles/myExcel.xlsx
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            using (var cK4 = myExcel.myExcelST.Cells["A4"])
            {
                cK4.Value = "TO: " + dtHeaderRow["Contact"].ToString() + " " + dtHeaderRow["CustName"].ToString();
                cK4.Style.Font.Bold = true;
                cK4.Style.Font.Size = 16;
            }
            
            using (var cK2 = myExcel.myExcelST.Cells["F4"])
            {
                cK2.Value ="NO: "+ QuoteNo;
                cK2.Style.Font.Bold = true;
                cK2.Style.Font.Size = 16;
            }

            using (var cK2 = myExcel.myExcelST.Cells["F7"])
            {
                cK2.Value = "结算币别: " + Currency;
                cK2.Style.Font.Bold = true;
                cK2.Style.Font.Size = 14;
            }          

            //写明细 SuppPN,CDesc,Brand,BuyPrice,Qty,Unit
            string refCell = "A11", cellQty = "", cellPrice = "";
            for (int i = 0; i < dtSub.Rows.Count; i++)
            {
                DataRow drSub = dtSub.Rows[i];

                myExcel.myExcelST.Cells[refCell].Value = drSub["Brand"].ToString();
                myExcel.myExcelST.Cells[refCell].Offset(0, 1).Value = drSub["CDesc"].ToString();
                myExcel.myExcelST.Cells[refCell].Offset(0, 2).Value = drSub["CustPN"].ToString();

                cellPrice = myExcel.myExcelST.Cells[refCell].Offset(0, 3).Address;
                myExcel.myExcelST.Cells[cellPrice].Value = decimal.Parse(drSub["BuyPrice"].ToString());
                myExcel.myExcelST.Cells[cellPrice].Style.Numberformat.Format = "#,##0.000";
                myExcel.myExcelST.Cells[cellPrice].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                cellQty = myExcel.myExcelST.Cells[refCell].Offset(0, 4).Address;
                myExcel.myExcelST.Cells[cellQty].Value = decimal.Parse(drSub["Qty"].ToString());
                myExcel.myExcelST.Cells[cellQty].Style.Numberformat.Format = "#,##0";
                myExcel.myExcelST.Cells[cellQty].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                myExcel.myExcelST.Cells[refCell].Offset(0, 5).Formula = cellQty + "*" + cellPrice;
                myExcel.myExcelST.Cells[refCell].Offset(0, 5).Style.Numberformat.Format = "#,##0.00";
                myExcel.myExcelST.Cells[refCell].Offset(0, 5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                myExcel.myExcelST.Cells[refCell].Offset(0, 6).Value = drSub["LeadTime"].ToString();
                myExcel.myExcelST.Cells[refCell].Offset(0, 6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                refCell = myExcel.myExcelST.Cells[refCell].Offset(1, 0).Address;
            }

            //汇总信息
            string cellSum1 = myExcel.myExcelST.Cells["A11"].Offset(0, 5).Address;
            string cellSum2 = myExcel.myExcelST.Cells[refCell].Offset(-1, 5).Address;
            myExcel.myExcelST.Cells[refCell].Offset(0, 5).Formula = string.Format("SUBTOTAL(9,{0})", new ExcelAddress(cellSum1 + ":" + cellSum2).Address);
            myExcel.myExcelST.Cells[refCell].Offset(0, 5).Style.Numberformat.Format = "#,##0.00";
            myExcel.myExcelST.Cells[refCell].Offset(0, 5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            using (var grid = myExcel.myExcelST.Cells[refCell].Offset(0, 4))
            {
                grid.Value = "TOTAL:"; grid.Style.Font.Size = 12; grid.Style.Font.Name = "Arial"; grid.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                grid.Offset(0, 1).Style.Font.Size = 12; grid.Offset(0, 1).Style.Font.Name = "Arial";
            }
            //设定表格边框
            using (var grid = myExcel.myExcelST.Cells["A11:" + myExcel.myExcelST.Cells[refCell].Offset(-1, 7).Address])
            {
                grid.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                grid.Style.Font.Size = 12; grid.Style.Font.Name = "Arial";
            }            

            //写最后一行客户签回:/Date: 
            using (var grid = myExcel.myExcelST.Cells[refCell].Offset(3, 0))
            {
                grid.Value = "客户签回:"; grid.Style.Font.Size = 12; grid.Style.Font.Name = "Arial";
                grid.Offset(0, 4).Value = "Date:" + DateTime.Today.ToString("yyyy-MM-dd"); grid.Offset(0, 4).Style.Font.Size = 12; grid.Offset(0, 4).Style.Font.Name = "Arial";
            }            
           
            //设定行高
            for (int i = 0; i < dtSub.Rows.Count + 1; i++)
            {
                myExcel.myExcelST.Row(11 + i).Height = 21;
            }

            myExcel.myExcelST.HeaderFooter.OddFooter.RightAlignedText =
                string.Format("Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
            myExcel.myExcelST.HeaderFooter.OddFooter.CenteredText = "EC Group " + ExcelHeaderFooter.SheetName;

            myExcel.myExcelST.PrinterSettings.FitToPage = true;
            myExcel.myExcelST.PrinterSettings.FitToWidth = 1;
            myExcel.myExcelST.PrinterSettings.FitToHeight = 0;

            myExcel.SaveXls(path);
            myExcel = null;
            return "/FJLBS/TempFiles/" + filename;

        }

        internal static string ExportExcel(ParamSP ps)
        {
            //QuoteNo,QuoteDate,IncoTerms,CustAbbr,CustPN,SuppPN,CDesc,Qty,ReplyPrice,Unit,Currency,SPQ,MOQ,LeadTime,VATFlag,TaxRate,QuoteStatus,Remarks
            string filename = "SaleQuoteList.xlsx";
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            DataTable dt = new SaleQuoteService().StoredProcedureDS(ps).Tables[0];

            double[] titleWidth = { 12, 12, 10, 12, 20, 20, 15, 10, 10, 8, 10, 12, 12, 12,8,15,15 };
            string[] title = { "报价单號", "报价日期", "狀態", "客户", "客户料号", "MPN", "品名", "数量", "价格", "单位", "幣別", "SPQ", "MOQ", "LeadTime", "含税", "税率", "备注" };

            IList<string> BuyOrderList = new List<string>();

            foreach (DataRow dr in dt.Rows)
            {
                if (!BuyOrderList.Contains(dr["QuoteNo"].ToString()) && !string.IsNullOrEmpty(dr["CustPN"].ToString()))
                { BuyOrderList.Add(dr["QuoteNo"].ToString()); }
            }
            EPPlusNew myExcel = new EPPlusNew("");

            //标题行
            for (int i = 0; i < title.Length; i++)
            {
                using (var grid = myExcel.myExcelST.Cells[1, i + 1])
                {
                    grid.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    grid.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    grid.Value = title[i];
                }
                myExcel.myExcelST.Column(i + 1).Width = titleWidth[i];
            }

            string startCell = "A2";
            string QuoteNo = "", QuoteDate = "", CustAbbr = "", QuoteStatus = "";
            decimal PendingQty = 0;
            foreach (string item in BuyOrderList)
            {
                QuoteNo = item; DataRow[] drDetail = dt.Select("QuoteNo='" + QuoteNo + "' and len(CustPN)>1");
                QuoteDate = drDetail[0]["QuoteDate"].ToString(); CustAbbr = drDetail[0]["CustAbbr"].ToString();
                QuoteStatus = drDetail[0]["QuoteStatus"].ToString(); 

                for (int i = 0; i < drDetail.Length; i++)
                {
                    using (var grid = myExcel.myExcelST.Cells[startCell])
                    {
                        if (i == 0)
                        {
                            grid.Value = QuoteNo; grid.Style.Font.Color.SetColor(Color.Blue);
                            grid.Offset(0, 1).Value = DateTime.Parse(QuoteDate); grid.Offset(0, 1).Style.Numberformat.Format = "yyyy/MM/dd";
                            grid.Offset(0, 2).Value = QuoteStatus; grid.Offset(0, 3).Value = CustAbbr;

                            for (int p = 0; p < 4; p++)
                            {
                                grid.Offset(0, p).Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                grid.Offset(0, p).Style.Fill.PatternType = ExcelFillStyle.Solid;
                                grid.Offset(0, p).Style.Fill.BackgroundColor.SetColor(Color.Cyan);
                            }
                        }

                        //CustPN,SuppPN,CDesc,Qty,ReplyPrice,Unit,Currency,SPQ,MOQ,LeadTime,VATFlag,TaxRate,QuoteStatus,Remarks

                        grid.Offset(0, 4).Value = drDetail[i]["CustPN"].ToString(); grid.Offset(0, 4).Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        grid.Offset(0, 5).Value = drDetail[i]["SuppPN"].ToString();
                        grid.Offset(0, 6).Value = drDetail[i]["CDesc"].ToString();
                        grid.Offset(0, 7).Value = decimal.Parse(drDetail[i]["Qty"].ToString()); grid.Offset(0, 7).Style.Numberformat.Format = "#,##0";
                        grid.Offset(0, 8).Value = decimal.Parse(drDetail[i]["ReplyPrice"].ToString()); grid.Offset(0, 8).Style.Numberformat.Format = "#,##0.00";
                        grid.Offset(0, 9).Value = drDetail[i]["Unit"].ToString();
                        grid.Offset(0, 10).Value = drDetail[i]["Currency"].ToString();
                        grid.Offset(0, 11).Value = drDetail[i]["SPQ"].ToString();
                        grid.Offset(0, 12).Value = drDetail[i]["MOQ"].ToString();
                        grid.Offset(0, 13).Value = drDetail[i]["LeadTime"].ToString();
                        grid.Offset(0, 14).Value = drDetail[i]["VATFlag"].ToString();
                        grid.Offset(0, 15).Value = drDetail[i]["TaxRate"].ToString();
                        grid.Offset(0, 16).Value = drDetail[i]["Remarks"].ToString();                        

                        for (int k = 4; k <= 16; k++)
                        {
                            grid.Offset(0, k).Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        }
                        startCell = myExcel.myExcelST.Cells[startCell].Offset(1, 0).Address;
                    }
                }

                myExcel.myExcelST.Cells[startCell + ":" + myExcel.myExcelST.Cells[startCell].Offset(0, titleWidth.Length - 1).Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            }

            myExcel.myExcelST.HeaderFooter.OddFooter.RightAlignedText =
                string.Format("Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
            myExcel.myExcelST.HeaderFooter.OddFooter.CenteredText = "EC Tek Sale Quote List";

            myExcel.myExcelST.PrinterSettings.Orientation = eOrientation.Landscape;
            myExcel.myExcelST.PrinterSettings.FitToPage = true;
            myExcel.myExcelST.PrinterSettings.FitToWidth = 1;
            myExcel.myExcelST.PrinterSettings.FitToHeight = 0;

            myExcel.myExcelST.View.ZoomScale = 100;

            myExcel.SaveXls(path);
            myExcel = null;
            return "/FJLBS/TempFiles/" + filename;
        }
    }
    public class SaleQuote : ModelBase
    {
        [PrimaryKey]
        public int QuoteID { get; set; }
        public string QuoteNo { get; set; }
        public DateTime? QuoteDate { get; set; }
        public string IncoTerms { get; set; }
        public string CustCode { get; set; }
        //public string SuppAddress { get; set; }
        public string Contact { get; set; }
        public string Tel { get; set; }
        //public string Email { get; set; }
        public string Currency { get; set; }
        public string Remarks { get; set; }
        public string QuoteStatus { get; set; }
        public string PriceType { get; set; }
        public string InputBy { get; set; }
        public DateTime? InputDT { get; set; }
        public string CFMFlag { get; set; }
        //public DateTime? CFMDate { get; set; }

        /*QuoteID,QuoteNo,QuoteDate,IncoTerms,CustCode,Contact,Tel,Currency,Remarks,QuoteStatus,PriceType,InputBy,InputDT,CFMFlag 
                g, */

    }
}