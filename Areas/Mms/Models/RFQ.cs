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
    public class RFQService : ServiceBase<RFQ>
    {
        public static string strSP = "DG_RFQHeader_SP";
        internal IList<RFQ> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<RFQ> myList = db.Select<RFQ>(strSQL).QueryMany();

            return myList;
        }

        public static List<dynamic> getRFQStatusList()
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "getRFQStatusList");
            List<dynamic> resultA = new SaleQuoteService().GetDynamicList(ps);

            return resultA;
        }

        internal static string ExportExcel(ParamSP ps)
        {            
            //RFQNo,RFQDate,SuppAbbr,Currency,SuppPN,CDesc,Qty,RFQPriceT,RFQPrice,TaxRate,MinQty,MinUnit,MinQtyUnit,MOQ,LeadTime,Status,Remarks
            //报价单号	日期	代理商	币别	MPN	描述	数量	含税价	不含税价	税率	最小数量	最小单位	最小采购单位	最小采购数量	LeadTime	报价单状态	Remarks
            
            //dt.Columns["RFQNo"].ColumnName = "报价单号"; dt.Columns["RFQDate"].ColumnName = "日期";
            //dt.Columns["SuppAbbr"].ColumnName = "代理商"; dt.Columns["Currency"].ColumnName = "币别";
            //dt.Columns["SuppPN"].ColumnName = "MPN"; dt.Columns["CDesc"].ColumnName = "描述";
            //dt.Columns["Qty"].ColumnName = "数量"; dt.Columns["RFQPriceT"].ColumnName = "含税价"; dt.Columns["RFQPrice"].ColumnName = "不含税价";
            //dt.Columns["TaxRate"].ColumnName = "税率";
            //dt.Columns["MinQty"].ColumnName = "最小数量";
            //dt.Columns["MinUnit"].ColumnName = "最小单位";
            //dt.Columns["MinQtyUnit"].ColumnName = "最小采购单位";
            //dt.Columns["MOQ"].ColumnName = "最小采购数量";
            //dt.Columns["Status"].ColumnName = "报价单状态";

            //报表中的状态为采购单的交货状态,不是客户订单的出货状态
            double[] titleWidth = { 12, 12, 12, 8, 20, 20, 10, 10, 10, 8, 12, 12, 12, 12, 12, 12,15 };
            string[] title = { "报价单号", "日期", "代理商", "币别", "MPN", "描述", "数量", "含税价", "不含税价", "税率", "最小数量", "最小单位", "最小采购单位", "最小采购数量", "LeadTime", "报价单状态", "Remarks" };

            DataTable dt = new RFQService().StoredProcedureDS(ps).Tables[0];
            IList<string> RFQList = new List<string>();

            foreach (DataRow dr in dt.Rows)
            {
                if (!RFQList.Contains(dr["RFQNo"].ToString()))
                { RFQList.Add(dr["RFQNo"].ToString()); }
            }

            EPPlusNew myExcel = new EPPlusNew("");

            //标题行
            for (int i = 0; i < title.Length; i++)
            {
                using (var grid = myExcel.myExcelST.Cells[1, i + 1])
                {
                    grid.Style.Font.Color.SetColor(Color.Blue);
                    grid.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    grid.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    grid.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    grid.Value = title[i];
                }
                myExcel.myExcelST.Column(i + 1).Width = titleWidth[i];
            }



            string startCell = "A2";
            string RFQNo = "", RFQDate = "",SuppAbbr = "";            
            foreach (string item in RFQList)
            {
                RFQNo = item; DataRow[] drDetail = dt.Select("RFQNo='" + RFQNo + "'");
                RFQDate = drDetail[0]["RFQDate"].ToString(); SuppAbbr = drDetail[0]["SuppAbbr"].ToString();

                for (int i = 0; i < drDetail.Length; i++)
                {
                    using (var grid = myExcel.myExcelST.Cells[startCell])
                    {
                        if (i == 0)
                        {
                            grid.Value = RFQNo; grid.Style.Font.Color.SetColor(Color.Blue);
                            grid.Offset(0, 1).Value = DateTime.Parse(RFQDate); grid.Offset(0, 1).Style.Numberformat.Format = "yyyy/MM/dd";
                            grid.Offset(0, 2).Value = SuppAbbr;

                            for (int p = 0; p < 3; p++)
                            {
                                grid.Offset(0, p).Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                grid.Offset(0, p).Style.Fill.PatternType = ExcelFillStyle.Solid;
                                grid.Offset(0, p).Style.Fill.BackgroundColor.SetColor(Color.Cyan);
                            }
                        }

                        grid.Offset(0, 3).Value = drDetail[i]["Currency"].ToString(); grid.Offset(0, 3).Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        grid.Offset(0, 4).Value = drDetail[i]["SuppPN"].ToString();
                        grid.Offset(0, 5).Value = drDetail[i]["CDesc"].ToString();
                        grid.Offset(0, 6).Value = decimal.Parse(drDetail[i]["Qty"].ToString()); grid.Offset(0, 6).Style.Numberformat.Format = "#,##0";
                        grid.Offset(0, 7).Value = decimal.Parse(drDetail[i]["RFQPriceT"].ToString()); grid.Offset(0, 7).Style.Numberformat.Format = "#,##0.00";
                        grid.Offset(0, 8).Value = decimal.Parse(drDetail[i]["RFQPrice"].ToString()); grid.Offset(0, 8).Style.Numberformat.Format = "#,##0.00";
                        grid.Offset(0, 9).Value = decimal.Parse(drDetail[i]["TaxRate"].ToString()); grid.Offset(0, 9).Style.Numberformat.Format = "#,##0.0";
                        grid.Offset(0, 10).Value = decimal.Parse(drDetail[i]["MinQty"].ToString()); grid.Offset(0, 10).Style.Numberformat.Format = "#,##0";
                        grid.Offset(0, 11).Value = drDetail[i]["MinUnit"].ToString();
                        grid.Offset(0, 12).Value = drDetail[i]["MinQtyUnit"].ToString();
                        grid.Offset(0, 13).Value = decimal.Parse(drDetail[i]["MOQ"].ToString()); grid.Offset(0, 13).Style.Numberformat.Format = "#,##0";
                        grid.Offset(0, 14).Value = drDetail[i]["LeadTime"].ToString();
                        grid.Offset(0, 15).Value = drDetail[i]["Status"].ToString();
                        grid.Offset(0, 16).Value = drDetail[i]["Remarks"].ToString();

                        for (int k = 3; k <= 16; k++)
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
            myExcel.myExcelST.HeaderFooter.OddFooter.CenteredText = "EC Tek Supplier Quotation List";

            myExcel.myExcelST.PrinterSettings.Orientation = eOrientation.Landscape;
            myExcel.myExcelST.PrinterSettings.FitToPage = true;
            myExcel.myExcelST.PrinterSettings.FitToWidth = 1;
            myExcel.myExcelST.PrinterSettings.FitToHeight = 0;

            myExcel.myExcelST.View.ZoomScale = 100;

            string filename = "RFQList.xlsx";
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            myExcel.SaveXls(path);
            myExcel = null;
            return "/FJLBS/TempFiles/" + filename;            
        }
       
    }
    public class RFQ : ModelBase
    {
        [PrimaryKey]
        public int RFQID { get; set; }
        public string RFQNo { get; set; }
        public DateTime? RFQDate { get; set; }
        public string SuppCode { get; set; }
        public string SuppAbbr { get; set; }
        public string Contact { get; set; }
        public string Tel { get; set; }
        public string Currency { get; set; }
        public string Remarks { get; set; }
        public string InputBy { get; set; }
        public DateTime? InputDT { get; set; }
        public string Status { get; set; }
        public DateTime? ApproveDT { get; set; }

        /*RFQID, RFQNo, RFQDate,  SuppCode, SuppAbbr, Contact, Tel,  Currency, Remarks,InputBy, InputDT, Status, ApproveDT */

    }
}