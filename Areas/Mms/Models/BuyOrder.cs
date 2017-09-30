using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using Zephyr.Core;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ECGroup.Models
{
    [Module("EC")]
    public class BuyOrderService : ServiceBase<BuyOrder>
    {
        //test github
        public static string strSP = "DG_BuyOrder_SP";
        internal IList<BuyOrder> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<BuyOrder> myList = db.Select<BuyOrder>(strSQL).QueryMany();

            return myList;
        }

        public static List<dynamic> getBuyOrderStatusList()
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ps.Parameter("ActionType", "getBuyOrderStatus");
            List<dynamic> resultA = new BuyOrderService().GetDynamicList(ps);

            resultA.Insert(0, new {value="",text="全部" });
            return resultA;

        }

        public static List<dynamic> getPriceTypeList()
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ps.Parameter("ActionType", "getPriceTypeList");
            List<dynamic> resultA = new BuyOrderService().GetDynamicList(ps);
            
            return resultA;
        }


        public static List<dynamic> getPendingStatusList()
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ps.Parameter("ActionType", "getPendingStatus");
            List<dynamic> resultA = new BuyOrderService().GetDynamicList(ps);

            resultA.Insert(0, new { value = "", text = "全部" });
            return resultA;
        }

        internal static string ExportBOExcel(ParamSP ps)
        {
            //double[] colWidth = {13,50,16,14,9,4,10,9,6,9,10,8};
            //double[] rowHeight = { 18, 29, 21, 24, 10, 24, 18, 19, 20, 18, 8, 19, 17 };

            //string logomap=Zephyr.Utils.ZHttp.RootPhysicalPath + @"Content\Template\logo48.png";
            string fileTemp = Zephyr.Utils.ZHttp.RootPhysicalPath + @"Content\Template\BuyOrder.xlsx";
            DataSet ds = new BuyOrderService().StoredProcedureDS(ps);
            EPPlusNew myExcel = new EPPlusNew(fileTemp);

            DataRow dtHeaderRow = ds.Tables[0].Rows[0]; //SuppName,B.SuppAdd,A.Contact,A.Tel,B.CellNo,A.PriceType,A.Currency
            DataTable dtSub = ds.Tables[1];

            string BuyNo = dtHeaderRow["BuyNo"].ToString();
            string Currency = dtHeaderRow["Currency"].ToString();
            string filename = BuyNo + ".xlsx";
            string fileURL = "/FJLBS/TempFiles/" + filename;  // 格式： /FJLBS/TempFiles/myExcel.xlsx
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            using (var cK2 = myExcel.myExcelST.Cells["K2"])
            {
                cK2.Value = BuyNo;
                cK2.Offset(1,0).Value = DateTime.Today.ToString("yyyy-MM-dd");
                cK2.Style.Font.Bold = true;
                cK2.Style.Font.Size = 10;                
            }

            myExcel.myExcelST.Cells["B6"].Value = dtHeaderRow["SuppName"].ToString();
            myExcel.myExcelST.Cells["B7"].Value = dtHeaderRow["SuppAdd"].ToString(); myExcel.myExcelST.Cells["B8"].Value = "";

            myExcel.myExcelST.Cells["H6"].Value ="Direct:"+ dtHeaderRow["Tel"].ToString();
            myExcel.myExcelST.Cells["H7"].Value = "Mobil:" + dtHeaderRow["CellNo"].ToString(); myExcel.myExcelST.Cells["H8"].Value = dtHeaderRow["Contact"].ToString();
            myExcel.myExcelST.Cells["J10"].Value ="价格条款："+ dtHeaderRow["PriceType"].ToString();

            //写明细 SuppPN,CDesc,Brand,BuyPrice,Qty,Unit
            string refCell = "A13",cellQty="",cellPrice="";
            for (int i = 0; i < dtSub.Rows.Count; i++)
            {
                DataRow drSub=dtSub.Rows[i];

                myExcel.myExcelST.Cells[refCell].Value = (i + 1).ToString();
                myExcel.myExcelST.Cells[refCell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                myExcel.myExcelST.Cells[refCell].Offset(0, 1).Value = drSub["SuppPN"].ToString();
                myExcel.myExcelST.Cells[refCell].Offset(0, 2).Value = drSub["CDesc"].ToString();
                myExcel.myExcelST.Cells[refCell].Offset(0, 3).Value = drSub["Brand"].ToString();

                myExcel.myExcelST.Cells[refCell].Offset(0, 1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                myExcel.myExcelST.Cells[refCell].Offset(0, 2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                myExcel.myExcelST.Cells[refCell].Offset(0, 3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                
                cellQty = myExcel.myExcelST.Cells[refCell].Offset(0, 4).Address;
                myExcel.myExcelST.Cells[cellQty].Value = drSub["Qty"].ToString();
                myExcel.myExcelST.Cells[cellQty].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                myExcel.myExcelST.Cells[refCell].Offset(0, 5).Value = "pcs";
                myExcel.myExcelST.Cells[refCell].Offset(0, 6).Value = decimal.Parse(drSub["Qty"].ToString());
                myExcel.myExcelST.Cells[refCell].Offset(0, 6).Style.Numberformat.Format = "#,##0";
                myExcel.myExcelST.Cells[refCell].Offset(0, 6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                myExcel.myExcelST.Cells[refCell].Offset(0, 7).Value = drSub["ReqDate"].ToString();

                cellPrice = myExcel.myExcelST.Cells[refCell].Offset(0, 8).Address;
                myExcel.myExcelST.Cells[cellPrice].Value =decimal.Parse(drSub["BuyPrice"].ToString());
                myExcel.myExcelST.Cells[cellPrice].Style.Numberformat.Format = "#,##0.000";

                myExcel.myExcelST.Cells[refCell].Offset(0, 9).Value = Currency;
                myExcel.myExcelST.Cells[refCell].Offset(0, 10).Formula = cellQty+"*"+cellPrice;
                myExcel.myExcelST.Cells[refCell].Offset(0, 10).Style.Numberformat.Format = "#,##0.00";
                myExcel.myExcelST.Cells[refCell].Offset(0, 10).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                myExcel.myExcelST.Cells[refCell].Offset(0, 11).Value = Currency;
                refCell = myExcel.myExcelST.Cells[refCell].Offset(1, 0).Address;
            }

            //汇总信息
            string cellSum1 = myExcel.myExcelST.Cells["A13"].Offset(0, 6).Address;
            string cellSum2 = myExcel.myExcelST.Cells[refCell].Offset(-1, 6).Address;
            myExcel.myExcelST.Cells[refCell].Offset(0, 6).Formula = string.Format("SUBTOTAL(9,{0})", new ExcelAddress(cellSum1+":"+cellSum2).Address);
            myExcel.myExcelST.Cells[refCell].Offset(0, 6).Style.Numberformat.Format = "#,##0";

            myExcel.myExcelST.Cells[refCell].Offset(0, 7).Value = "PCS";

            cellSum1 = myExcel.myExcelST.Cells["A13"].Offset(0, 10).Address;
            cellSum2 = myExcel.myExcelST.Cells[refCell].Offset(-1, 10).Address;
            myExcel.myExcelST.Cells[refCell].Offset(0, 10).Formula = string.Format("SUBTOTAL(9,{0})", new ExcelAddress(cellSum1 + ":" + cellSum2).Address);
            myExcel.myExcelST.Cells[refCell].Offset(0, 10).Style.Numberformat.Format = "#,##0.00";
            myExcel.myExcelST.Cells[refCell].Offset(0, 10).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            myExcel.myExcelST.Cells[refCell].Offset(0, 11).Value = Currency;

            //设定表格边框
            using (var grid = myExcel.myExcelST.Cells["A13:" + myExcel.myExcelST.Cells[refCell].Offset(-1, 11).Address])
            {                
                grid.Style.Border.BorderAround(ExcelBorderStyle.Medium);
            }

            using (var grid = myExcel.myExcelST.Cells["B13:" + myExcel.myExcelST.Cells[refCell].Offset(-1, 10).Address])
            {
                grid.Style.Border.Left.Style = ExcelBorderStyle.Thin; grid.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            //写最后三行PLT/NO:CTN/NO:INV. NO: 
            myExcel.myExcelST.Cells[refCell].Offset(3, 4).Value = "PLT/NO:";
            myExcel.myExcelST.Cells[refCell].Offset(4, 4).Value = "CTN/NO:";
            myExcel.myExcelST.Cells[refCell].Offset(5, 4).Value = "INV. NO:";
            myExcel.myExcelST.Cells[refCell].Offset(5, 5).Value = BuyNo;

            //设定表格边框
            using (var grid = myExcel.myExcelST.Cells["A13:" + myExcel.myExcelST.Cells[refCell].Offset(5, 11).Address])
            {
                grid.Style.Border.BorderAround(ExcelBorderStyle.Medium);
            }

            myExcel.myExcelST.Cells[refCell].Offset(6, 7).Value = "E&C Industrial (HongKong) Co.Ltd";
            myExcel.myExcelST.Cells[refCell].Offset(6, 7).Style.Font.Bold = true;
            myExcel.myExcelST.Cells[refCell].Offset(6, 7).Style.Font.Size = 16;

            //设定行高
            for (int i = 0; i < dtSub.Rows.Count + 7; i++)
            { 
                myExcel.myExcelST.Row(13+i).Height = 21;     
            }

            myExcel.myExcelST.HeaderFooter.OddFooter.RightAlignedText =
                string.Format("Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);            
            myExcel.myExcelST.HeaderFooter.OddFooter.CenteredText ="EC Group "+ ExcelHeaderFooter.SheetName;

            myExcel.myExcelST.PrinterSettings.FitToPage = true;
            myExcel.myExcelST.PrinterSettings.FitToWidth = 1;
            myExcel.myExcelST.PrinterSettings.FitToHeight = 0;

            myExcel.SaveXls(path);
            myExcel = null;
            return "/FJLBS/TempFiles/" + filename;

        }

        internal static string ExportExcel(ParamSP ps)
        {
            //ck,BuyID, BuyNo, BuyDate, SuppAbbr, SuppPN, CDesc,BuyPrice,Currency,Qty,Amount,RcvQty, PriceType,BuyStatus,text,Remarks
            string filename = "BuyOrderList.xlsx";
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;       

            DataTable dt = new BuyOrderService().StoredProcedureDS(ps).Tables[0];
            dt.Columns.Remove("ck"); dt.Columns.Remove("BuyID"); 

            double[] titleWidth = { 12, 12, 12, 20, 15, 10, 8, 10, 10, 8, 10, 12, 12, 12};
            string[] title = { "采购单號", "采购日期", "供应商", "MPN", "品名", "采购价", "幣別", "采购数", "總金額", "入库数", "含税", "狀態", "入库状态", "备注" };

            IList<string> BuyOrderList = new List<string>();

            foreach (DataRow dr in dt.Rows)
            {
                if (!BuyOrderList.Contains(dr["BuyNo"].ToString()) && !string.IsNullOrEmpty(dr["SuppPN"].ToString()))
                { BuyOrderList.Add(dr["BuyNo"].ToString()); }
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
            string BuyNo = "", BuyDate = "", SuppAbbr = "",RcvStatus="";
            decimal PendingQty = 0;
            foreach (string item in BuyOrderList)
            {
                BuyNo = item; DataRow[] drDetail = dt.Select("BuyNo='" + BuyNo + "' and len(SuppPN)>1");
                BuyDate = drDetail[0]["BuyDate"].ToString(); SuppAbbr = drDetail[0]["SuppAbbr"].ToString();

                for (int i = 0; i < drDetail.Length; i++)
                {
                    using (var grid = myExcel.myExcelST.Cells[startCell])
                    {
                        if (i == 0)
                        {
                            grid.Value = BuyNo; grid.Style.Font.Color.SetColor(Color.Blue);
                            grid.Offset(0, 1).Value = DateTime.Parse(BuyDate); grid.Offset(0, 1).Style.Numberformat.Format = "yyyy/MM/dd";
                            grid.Offset(0, 2).Value = SuppAbbr;                        

                            for (int p = 0; p < 3; p++)
                            {
                                grid.Offset(0, p).Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                grid.Offset(0, p).Style.Fill.PatternType = ExcelFillStyle.Solid;
                                grid.Offset(0, p).Style.Fill.BackgroundColor.SetColor(Color.Cyan);
                            }
                        }

                        grid.Offset(0, 3).Value = drDetail[i]["SuppPN"].ToString(); grid.Offset(0, 3).Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        grid.Offset(0, 4).Value = drDetail[i]["CDesc"].ToString();
                        grid.Offset(0, 5).Value = decimal.Parse(drDetail[i]["BuyPrice"].ToString()); grid.Offset(0, 5).Style.Numberformat.Format = "#,##0.00";
                        grid.Offset(0, 6).Value = drDetail[i]["Currency"].ToString();
                        grid.Offset(0, 7).Value = decimal.Parse(drDetail[i]["Qty"].ToString()); grid.Offset(0, 7).Style.Numberformat.Format = "#,##0";
                        grid.Offset(0, 8).Formula = grid.Offset(0, 5).Address + "*" + grid.Offset(0, 7).Address; grid.Offset(0, 8).Style.Numberformat.Format = "#,##0.00";
                        grid.Offset(0, 9).Value = decimal.Parse(drDetail[i]["RcvQty"].ToString()); grid.Offset(0, 8).Style.Numberformat.Format = "#,##0";
                        grid.Offset(0, 10).Value = drDetail[i]["PriceType"].ToString();
                        grid.Offset(0, 11).Value = drDetail[i]["BuyStatus"].ToString();
                        
                        RcvStatus = drDetail[i]["text"].ToString();grid.Offset(0, 12).Value = RcvStatus;
                        if (RcvStatus != "已入库") { grid.Offset(0, 12).Style.Font.Color.SetColor(Color.Red); }                     
                        grid.Offset(0, 13).Value = drDetail[i]["Remarks"].ToString();

                        for (int k = 3; k <= 13; k++)
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
            myExcel.myExcelST.HeaderFooter.OddFooter.CenteredText = "EC Tek BuyOrder Receiving Check";

            myExcel.myExcelST.PrinterSettings.Orientation = eOrientation.Landscape;
            myExcel.myExcelST.PrinterSettings.FitToPage = true;
            myExcel.myExcelST.PrinterSettings.FitToWidth = 1;
            myExcel.myExcelST.PrinterSettings.FitToHeight = 0;

            myExcel.myExcelST.View.ZoomScale = 100;

            myExcel.SaveXls(path);
            myExcel = null;
            return "/FJLBS/TempFiles/" + filename;
        }

        internal static string ExportPendingExcel(ParamSP ps)
        {
            string filename = "BuyOrderPending.xlsx";
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            //报表中的状态为采购单的交货状态,不是客户订单的出货状态
            double[] titleWidth = { 12, 12, 12, 12, 20, 20, 20, 10, 10, 8, 12, 12, 12, 12, 12, 12 };
            string[] title = { "采购单號", "采购日期", "采购单狀態", "代理商", "制造商料号", "品名", "规格", "采购数量", "采购单价", "币别", "入库数量", "入库单号", "入库日期", "入库单狀態", "待入库数量", "交货狀態" };       

            DataTable dt = new BuyOrderService().StoredProcedureDS(ps).Tables[0];
            IList<string> BuyOrderList = new List<string>();

            foreach(DataRow dr in dt.Rows)
            {
                if (!BuyOrderList.Contains(dr["BuyNo"].ToString()))
                { BuyOrderList.Add(dr["BuyNo"].ToString());}
            }

            dt.Columns.Remove("BuyID"); dt.Columns.Remove("SuppCode"); dt.Columns.Remove("RcvID");
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
            string BuyNo = "", BuyDate = "", RcvDate = "", BuyStatus = "", SuppAbbr = "", PendingStatus = "";
            decimal PendingQty = 0;
            foreach (string item in BuyOrderList)
            {
                BuyNo = item; DataRow[] drDetail = dt.Select("BuyNo='" + BuyNo + "'");
                BuyDate = drDetail[0]["BuyDate"].ToString(); BuyStatus = drDetail[0]["BuyStatus"].ToString(); SuppAbbr = drDetail[0]["SuppName"].ToString();

                for (int i = 0; i < drDetail.Length; i++)
                {
                    using (var grid = myExcel.myExcelST.Cells[startCell])
                    {
                        if (i == 0)
                        {
                            grid.Value = BuyNo; grid.Style.Font.Color.SetColor(Color.Blue);
                            grid.Offset(0, 1).Value = DateTime.Parse(BuyDate); grid.Offset(0, 1).Style.Numberformat.Format = "yyyy/MM/dd";
                            grid.Offset(0, 2).Value = BuyStatus;
                            grid.Offset(0, 3).Value = SuppAbbr;

                            for (int p = 0; p < 4; p++)
                            {
                                grid.Offset(0, p).Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                grid.Offset(0, p).Style.Fill.PatternType = ExcelFillStyle.Solid;
                                grid.Offset(0, p).Style.Fill.BackgroundColor.SetColor(Color.Cyan);
                            }
                        }

                        grid.Offset(0, 4).Value = drDetail[i]["SuppPN"].ToString(); grid.Offset(0, 4).Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        grid.Offset(0, 5).Value = drDetail[i]["CDesc"].ToString();
                        grid.Offset(0, 6).Value = drDetail[i]["CSpec"].ToString();
                        grid.Offset(0, 7).Value = decimal.Parse(drDetail[i]["BuyQty"].ToString()); grid.Offset(0, 7).Style.Numberformat.Format = "#,##0";
                        grid.Offset(0, 8).Value = decimal.Parse(drDetail[i]["BuyPrice"].ToString()); grid.Offset(0, 8).Style.Numberformat.Format = "#,##0.00";
                        grid.Offset(0, 9).Value = drDetail[i]["Currency"].ToString();
                        grid.Offset(0, 10).Value = decimal.Parse(drDetail[i]["RcvQty"].ToString()); grid.Offset(0, 10).Style.Numberformat.Format = "#,##0";
                        grid.Offset(0, 11).Value = drDetail[i]["RcvNo"].ToString();

                        RcvDate = drDetail[i]["RcvDate"].ToString();
                        if (!string.IsNullOrEmpty(RcvDate))
                        { grid.Offset(0, 12).Value = DateTime.Parse(drDetail[i]["RcvDate"].ToString()); grid.Offset(0, 12).Style.Numberformat.Format = "yyyy/MM/dd"; }
                        grid.Offset(0, 13).Value = drDetail[i]["RcvStatus"].ToString();

                        grid.Offset(0, 14).Formula = grid.Offset(0, 7).Address + "-" + grid.Offset(0, 10).Address; grid.Offset(0, 14).Style.Numberformat.Format = "#,##0";

                        PendingQty = decimal.Parse(grid.Offset(0, 7).Value.ToString()) - decimal.Parse(grid.Offset(0, 10).Value.ToString());
                        if (PendingQty>0)
                        {                            
                            grid.Offset(0, 14).Style.Font.Color.SetColor(Color.Blue); 
                        }

                        PendingStatus = drDetail[i]["PendingStatus"].ToString();
                        grid.Offset(0, 15).Value = drDetail[i]["PendingStatus"].ToString();
                        if (PendingStatus == "入库异常") {grid.Offset(0, 15).Style.Font.Color.SetColor(Color.Red); }

                        for (int k = 4; k <= 15; k++)
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
            myExcel.myExcelST.HeaderFooter.OddFooter.CenteredText = "EC Tek BuyOrder Pending Analysis";

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

    public class BuyOrder : ModelBase
    {
        [PrimaryKey]
        public int BuyID { get; set; }
        public string BuyNo { get; set; }
        public DateTime? BuyDate { get; set; }
        public string SuppCode { get; set; }
        //public string SuppName { get; set; }
        //public string SuppAddress { get; set; }
        public string Contact { get; set; }
        public string Tel { get; set; }
        //public string Email { get; set; }
        public string Currency { get; set; }
        public string Remarks { get; set; }
        public string PriceType { get; set; }
        public string InputBy { get; set; }
        public DateTime? InputDT { get; set; }
        //public string CFMFlag { get; set; }
        //public DateTime? CFMDate { get; set; }
        public string BuyStatus { get; set; }
        //public DateTime? StatusDT { get; set; }       

        /* BuyID, BuyNo, BuyDate, SuppCode, SuppName, SuppAddress, Contact, Tel, Email, Currency, Remarks, BuyStatus, StatusDT, 
                 PriceType, InputBy, InputDT, EditBy, EditDT, CFMFlag, CFMDate
 */
    }
}