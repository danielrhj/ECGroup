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
    public class EcomService : ServiceBase<Ecom>
    {
        public static string strSP = "DG_EComHeader_sp";
        internal IList<Ecom> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<Ecom> myList = db.Select<Ecom>(strSQL).QueryMany();

            return myList;
        }

        //public static List<dynamic> getShipingStatusList()
        //{
        //    ParamSP ps = new ParamSP().Name(strSP);
        //    ParamSPData psd = ps.GetData();
        //    ps.Parameter("ActionType", "getShipingStatusList");
        //    List<dynamic> resultA = new EcomService().GetDynamicList(ps);

        //    return resultA;
        //}

        public static List<dynamic> GetPNList()
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetEcomPNList");
            List<dynamic> resultA = new EcomService().GetDynamicList(ps);

            return resultA;
        }

        internal static string ExportMonthSaleExcel(ParamSP ps)
        {
            string fileTemp = Zephyr.Utils.ZHttp.RootPhysicalPath + @"Content\Template\MonthSaleEcom.xlsx";
            DataSet ds = new EcomService().StoredProcedureDS(ps);
            EPPlusNew myExcel = new EPPlusNew(fileTemp);

            DataRow dtHeaderRow = ds.Tables[0].Rows[0]; //SaleNo,SaleMonth,SaleCNCY,SettleCNCY,ExRate,SaleTotal,SaleNet,OffSet,AdsCost,ManCost,OverHead,FixedCost			
            DataTable dtSub = ds.Tables[1];// PNID,PN,Qty,Price,LogCost,PKGCost,OtherCost,Currency

            string SaleNo = dtHeaderRow["SaleNo"].ToString();
            string SaleMonth = dtHeaderRow["SaleMonth"].ToString();
            string SaleCNCY = dtHeaderRow["SaleCNCY"].ToString();
            string filename = SaleNo + ".xlsx";
            string fileURL = "/FJLBS/TempFiles/" + filename;  // 格式： /FJLBS/TempFiles/myExcel.xlsx
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            string[] cellAddress = { "A2", "B2", "C2", "D2", "E2", "F2", "H2", "J2", "C5","C6","C7","C8" };
            string[] cols = { "SaleNo", "SaleMonth", "SaleCNCY", "SettleCNCY", "ExRate", "SaleTotal","Offset", "SaleNet", "AdsCost", "ManCost","OverHead","FixedCost" };

            for (int i = 0; i < cellAddress.Length;i++ )
            {
                var cK4 = myExcel.myExcelST.Cells[cellAddress[i]];
                cK4.Value = dtHeaderRow[cols[i]].ToString();
                cK4.Style.Font.Size = 12;

                if (i==6)
                { cK4.Style.Numberformat.Format = "#,##0.0"; }
                if (i == 7)
                { cK4.Style.Numberformat.Format = "#,##0"; }
                if (i == 4 || i == 5)
                { cK4.Style.Numberformat.Format = "#,##0.00"; }
            }

            //总营业额原币折算为本币
            using (var cK2 = myExcel.myExcelST.Cells["G2"])
            {
                cK2.Formula = "F2/E2";
                cK2.Style.Numberformat.Format = "#,##0.00";
            }

            //提点金额折算为本币
            using (var cK2 = myExcel.myExcelST.Cells["I2"])
            {
                cK2.Formula = "F2*H2/E2/100";
                cK2.Style.Numberformat.Format = "#,##0.00";
            }

            //营业净值折算为本币
            using (var cK2 = myExcel.myExcelST.Cells["K2"])
            {
                cK2.Formula = "J2/E2";
                cK2.Style.Numberformat.Format = "#,##0.00";
            }

            //营业净值扣点后(本币)
            using (var cK2 = myExcel.myExcelST.Cells["L2"])
            {
                cK2.Formula = "K2-I2";
                cK2.Style.Numberformat.Format = "#,##0.00";
            }

            //写明细 PN,Qty,Price,LogCost,PKGCost,OtherCost,Currency
            string refCell = "A12", cellQty = "", cellPrice = "",cellSingleCost="";
            for (int i = 0; i < dtSub.Rows.Count; i++)
            {
                DataRow drSub = dtSub.Rows[i];
                myExcel.myExcelST.Cells[refCell].Value = (i + 1).ToString(); myExcel.myExcelST.Cells[refCell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                myExcel.myExcelST.Cells[refCell].Offset(0, 1).Value = drSub["PN"].ToString();
                myExcel.myExcelST.Cells[refCell].Offset(0, 2).Value = drSub["Qty"].ToString();myExcel.myExcelST.Cells[refCell].Offset(0, 2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                cellPrice = myExcel.myExcelST.Cells[refCell].Offset(0, 3).Address;
                myExcel.myExcelST.Cells[cellPrice].Value = decimal.Parse(drSub["Price"].ToString());
                myExcel.myExcelST.Cells[cellPrice].Style.Numberformat.Format = "#,##0.00";
                myExcel.myExcelST.Cells[cellPrice].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                cellQty = myExcel.myExcelST.Cells[refCell].Offset(0, 4).Address;
                myExcel.myExcelST.Cells[cellQty].Value = decimal.Parse(drSub["LogCost"].ToString());
                myExcel.myExcelST.Cells[cellQty].Style.Numberformat.Format = "#,##0.00";
                myExcel.myExcelST.Cells[cellQty].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                cellQty = myExcel.myExcelST.Cells[refCell].Offset(0, 5).Address;
                myExcel.myExcelST.Cells[cellQty].Value = decimal.Parse(drSub["PKGCost"].ToString());
                myExcel.myExcelST.Cells[cellQty].Style.Numberformat.Format = "#,##0.00";
                myExcel.myExcelST.Cells[cellQty].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                cellQty = myExcel.myExcelST.Cells[refCell].Offset(0, 6).Address;
                myExcel.myExcelST.Cells[cellQty].Value = decimal.Parse(drSub["OtherCost"].ToString());
                myExcel.myExcelST.Cells[cellQty].Style.Numberformat.Format = "#,##0.00";
                myExcel.myExcelST.Cells[cellQty].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                myExcel.myExcelST.Cells[refCell].Offset(0, 7).Value = drSub["Currency"].ToString();

                //单个总成本
                cellSingleCost = myExcel.myExcelST.Cells[refCell].Offset(0, 8).Address;
                myExcel.myExcelST.Cells[cellSingleCost].Formula = "sum(" + cellPrice + ":" + cellQty + ")";
                myExcel.myExcelST.Cells[cellSingleCost].Style.Numberformat.Format = "#,##0.00";
                myExcel.myExcelST.Cells[cellSingleCost].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                //总成本
                myExcel.myExcelST.Cells[refCell].Offset(0, 9).Formula = cellSingleCost + "*" + myExcel.myExcelST.Cells[refCell].Offset(0, 2).Address;
                myExcel.myExcelST.Cells[refCell].Offset(0, 9).Style.Numberformat.Format = "#,##0.00";
                myExcel.myExcelST.Cells[refCell].Offset(0, 9).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                //总物流成本
                myExcel.myExcelST.Cells[refCell].Offset(0, 10).Formula = myExcel.myExcelST.Cells[refCell].Offset(0, 4).Address + "*" + myExcel.myExcelST.Cells[refCell].Offset(0, 2).Address;
                myExcel.myExcelST.Cells[refCell].Offset(0, 10).Style.Numberformat.Format = "#,##0.00";
                myExcel.myExcelST.Cells[refCell].Offset(0, 10).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                //总商品成本(Qty*(Price+PKGCost))
                cellQty = myExcel.myExcelST.Cells[refCell].Offset(0, 2).Address;

                myExcel.myExcelST.Cells[refCell].Offset(0, 11).Formula = cellQty+"*(" +cellPrice+ "+" + myExcel.myExcelST.Cells[refCell].Offset(0, 5).Address+")";
                myExcel.myExcelST.Cells[refCell].Offset(0, 11).Style.Numberformat.Format = "#,##0.00";
                myExcel.myExcelST.Cells[refCell].Offset(0, 11).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                refCell = myExcel.myExcelST.Cells[refCell].Offset(1, 0).Address;
            }

            //汇总信息
            string cellSum1 = myExcel.myExcelST.Cells["A12"].Offset(0, 9).Address;
            string cellSum2 = myExcel.myExcelST.Cells[refCell].Offset(-1, 9).Address;
            myExcel.myExcelST.Cells[refCell].Offset(0, 9).Formula = string.Format("SUBTOTAL(9,{0})", new ExcelAddress(cellSum1 + ":" + cellSum2).Address);
            myExcel.myExcelST.Cells[refCell].Offset(0, 9).Style.Numberformat.Format = "#,##0.00";
            myExcel.myExcelST.Cells[refCell].Offset(0, 9).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            using (var grid = myExcel.myExcelST.Cells[refCell].Offset(0, 8))
            {
                grid.Value = "直接总成本:"; grid.Style.Font.Size = 12; grid.Style.Font.Name = "Arial"; grid.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                grid.Offset(0, 1).Style.Font.Size = 12; grid.Offset(0, 1).Style.Font.Name = "Arial";

                grid.Offset(1, 0).Value = "占比%"; grid.Offset(1, 0).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                grid.Offset(1, 1).Formula = grid.Offset(0, 1).Address + "/G2"; grid.Offset(1, 1).Style.Numberformat.Format = "0.00%";
                grid.Offset(1, 1).Style.Fill.PatternType = ExcelFillStyle.Solid;
                grid.Offset(1, 1).Style.Fill.BackgroundColor.SetColor(Color.Red);
            }
            //设定表格边框
            using (var grid = myExcel.myExcelST.Cells["A12:" + myExcel.myExcelST.Cells[refCell].Offset(-1,11).Address])
            {
                grid.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                grid.Style.Font.Size = 12; grid.Style.Font.Name = "Arial";
            }

            //写商品利润和毛利润 
            using (var grid = myExcel.myExcelST.Cells[refCell].Offset(3, 0))
            {
                grid.Value = "商品利润";
                grid.Offset(0, 1).Value = "商品利润率(%)";
                grid.Offset(1, 0).Formula = "L2-" + myExcel.myExcelST.Cells[refCell].Offset(0, 9).Address + "-C5";
                grid.Offset(1, 0).Style.Numberformat.Format = "#,##0.00";
                grid.Offset(1, 1).Formula = grid.Offset(1, 0).Address + "/G2";
                grid.Offset(1, 1).Style.Numberformat.Format = "0.00%";

                grid.Offset(2, 0).Value = "毛利润 ";
                grid.Offset(2, 1).Value = "毛利润率(%)";

                grid.Offset(3, 0).Formula = grid.Offset(1, 0).Address+"-C6-C7-C8";
                grid.Offset(3, 0).Style.Numberformat.Format = "#,##0.00";
                grid.Offset(3,1).Formula = grid.Offset(3, 0).Address + "/G2";
                grid.Offset(3, 1).Style.Numberformat.Format = "0.00%";

                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        grid.Offset(i, j).Style.Fill.PatternType = ExcelFillStyle.Solid;
                        grid.Offset(i, j).Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    }
                }
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

            myExcel.myExcelST.View.ZoomScale = 100;
            myExcel.SaveXls(path);
            myExcel = null;
            return "/FJLBS/TempFiles/" + filename;

        }
    }
    public class Ecom : ModelBase
    {
        [PrimaryKey]
        public int EcomID { get; set; }
        public string SaleNo { get; set; }
        public string SaleMonth { get; set; }

        public string ExRate { get; set; }
        public string SaleTotal { get; set; }
        public string SaleNet { get; set; }
        public string AdsCost { get; set; }
        public string ManCost { get; set; }
        public string OverHead { get; set; }
        public string FixedCost { get; set; }
        public string OtherCost { get; set; }
        public string SaleCNCY { get; set; }
        public string SettleCNCY { get; set; }
        public string OffSet { get; set; }
        public string Remark { get; set; }
        public string Plat { get; set; }
        public string InputBy { get; set; }
        public DateTime? InputDT { get; set; }

        //public string Country { get; set; }
        //public DateTime? CFMDate { get; set; }

        /*SELECT  AutoID, SaleNo, SaleMonth, ExRate, SaleTotal, SaleNet, AdsCost, ManCost, OtherCost, Currency, OffSet, 
                Remark, Plat, Creator, CreateDate
        FROM      DG_EComHeader  */

    }
}