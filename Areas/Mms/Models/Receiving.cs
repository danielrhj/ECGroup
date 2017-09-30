using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using Zephyr.Core;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Data;

namespace ECGroup.Models
{
    [Module("EC")]
    public class ReceivingService : ServiceBase<Receiving>
    {
        public static string strSP = "DG_Receiving_SP";
        internal IList<Receiving> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<Receiving> myList = db.Select<Receiving>(strSQL).QueryMany();

            return myList;
        }

        public static List<dynamic> getReceivingStatus()
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ps.Parameter("ActionType", "getReceivingStatus");
            List<dynamic> resultA = new ReceivingService().GetDynamicList(ps);

            resultA.Insert(0, new {value="",text="全部" });
            return resultA;

        }

        public static List<dynamic> getPriceTypeList()
        {
            ParamSP ps = new ParamSP().Name(BuyOrderService.strSP);
            ps.Parameter("ActionType", "getPriceTypeList");
            List<dynamic> resultA = new ReceivingService().GetDynamicList(ps);
            
            return resultA;
        }

        internal static string ExportExcel(ParamSP ps)
        {
            //ck,RcvID,RcvNo,RcvDate,SuppAbbr,BuyNo,SuppPN,CDesc,CSpec,RcvPrice,Qty,Amount,Currency,PriceType,CFMFlag, RcvStatus,Remarks,DO
            //        入庫单號,入庫日期,供应商,採購单號,MPN,品名,规格,入库价格,入库数量,總金額,幣別,含税,确认,入庫单狀態,备注,送貨單
            string filename = "ReceivingList.xlsx";
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            DataTable dt = new ReceivingService().StoredProcedureDS(ps).Tables[0];
            dt.Columns.Remove("RcvID"); dt.Columns.Remove("CFMFlag");

            double[] titleWidth = { 12, 12, 12, 15, 15, 20, 20, 10, 10, 8, 8, 8, 12,15,15 };
            string[] title = "入库单号, 入库日期, 供应商, 采购单号, MPN, 品名, 规格, 入库价格, 入库数量, 总金额, 币别, 含税, 入库单状态, 备注, 送货单".Split(',');

            IList<string> BuyOrderList = new List<string>();

            foreach (DataRow dr in dt.Rows)
            {
                if (!BuyOrderList.Contains(dr["RcvNo"].ToString()) && !string.IsNullOrEmpty(dr["SuppPN"].ToString()))
                { BuyOrderList.Add(dr["RcvNo"].ToString()); }
            }
            EPPlusNew myExcel = new EPPlusNew("");

            //标题行
            for (int i = 0; i < title.Length; i++)
            {
                using (var grid = myExcel.myExcelST.Cells[1, i + 1])
                {
                    grid.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    grid.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    grid.Value = title[i].Trim();
                }
                myExcel.myExcelST.Column(i + 1).Width = titleWidth[i];
            }

            string startCell = "A2";
            string RcvNo = "", RcvDate = "", SuppAbbr = "", RcvStatus="";
            decimal PendingQty = 0;
            foreach (string item in BuyOrderList)
            {
                RcvNo = item; DataRow[] drDetail = dt.Select("RcvNo='" + RcvNo + "' and len(SuppPN)>1");
                RcvDate = drDetail[0]["RcvDate"].ToString(); SuppAbbr = drDetail[0]["SuppAbbr"].ToString();

                for (int i = 0; i < drDetail.Length; i++)
                {
                    using (var grid = myExcel.myExcelST.Cells[startCell])
                    {
                        if (i == 0)
                        {
                            grid.Value = RcvNo; grid.Style.Font.Color.SetColor(Color.Blue);
                            grid.Offset(0, 1).Value = DateTime.Parse(RcvDate); grid.Offset(0, 1).Style.Numberformat.Format = "yyyy/MM/dd";
                            grid.Offset(0, 2).Value = SuppAbbr;

                            for (int p = 0; p < 3; p++)
                            {
                                grid.Offset(0, p).Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                grid.Offset(0, p).Style.Fill.PatternType = ExcelFillStyle.Solid;
                                grid.Offset(0, p).Style.Fill.BackgroundColor.SetColor(Color.Cyan);
                            }
                        }

                        //BuyNo,SuppPN,CDesc,CSpec,RcvPrice,Qty,Amount,Currency,PriceType,RcvStatus,Remarks,DO
                        grid.Offset(0, 3).Value = drDetail[i]["BuyNo"].ToString(); grid.Offset(0, 3).Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        grid.Offset(0, 4).Value = drDetail[i]["SuppPN"].ToString();
                        grid.Offset(0, 5).Value = drDetail[i]["CDesc"].ToString();
                        grid.Offset(0, 6).Value = drDetail[i]["CSpec"].ToString();

                        grid.Offset(0, 7).Value = decimal.Parse(drDetail[i]["RcvPrice"].ToString()); grid.Offset(0, 7).Style.Numberformat.Format = "#,##0.00";
                        grid.Offset(0, 8).Value = decimal.Parse(drDetail[i]["Qty"].ToString()); grid.Offset(0, 8).Style.Numberformat.Format = "#,##0";
                        grid.Offset(0, 9).Formula = grid.Offset(0, 7).Address + "*" + grid.Offset(0, 8).Address; grid.Offset(0, 9).Style.Numberformat.Format = "#,##0.00";
                        grid.Offset(0, 10).Value = drDetail[i]["Currency"].ToString();
                        grid.Offset(0, 11).Value = drDetail[i]["PriceType"].ToString();

                        RcvStatus = drDetail[i]["RcvStatus"].ToString(); grid.Offset(0, 12).Value = RcvStatus;
                        //if (RcvStatus != "已入库") { grid.Offset(0, 12).Style.Font.Color.SetColor(Color.Red); }
                        grid.Offset(0, 13).Value = drDetail[i]["Remarks"].ToString();
                        grid.Offset(0, 14).Value = drDetail[i]["DO"].ToString();

                        for (int k = 3; k <= 14; k++)
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
            myExcel.myExcelST.HeaderFooter.OddFooter.CenteredText = "EC Tek Receiving Analysis";

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

    public class Receiving : ModelBase
    {
        [PrimaryKey]
        public int RcvID { get; set; }
        public string RcvNo { get; set; }
        public string BuyNo { get; set; }
        public string DO { get; set; }
        public DateTime? RcvDate { get; set; }
        public string SuppCode { get; set; }
        public string SuppAbbr { get; set; }
        public string Contact { get; set; }
        public string Tel { get; set; }
        public string Email { get; set; }
        public string Currency { get; set; }
        public string Remarks { get; set; }
        public string RcvStatus { get; set; }
        public DateTime? StatusDT { get; set; }
        public string PriceType { get; set; }
        public string InputBy { get; set; }
        public DateTime? InputDT { get; set; }
        public string CFMFlag { get; set; }
        public DateTime? CFMDate { get; set; }



        /* RcvID, BuyNo, RcvNo, DO, RcvDate, SuppCode, SuppAbbr, Contact, Tel, Email, Currency, Remarks, 
                RcvStatus, StatusDT, PriceType, InputBy, InputDT,CFMFlag, CFMDate

 */
    }
}