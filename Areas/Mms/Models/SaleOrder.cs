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
    public class SaleOrderService : ServiceBase<SaleOrder>
    {
        public static string strSP = "DG_SaleOrder_sp";
        internal IList<SaleOrder> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<SaleOrder> myList = db.Select<SaleOrder>(strSQL).QueryMany();

            return myList;
        }

        public static List<dynamic> getSaleOrderStatusList()
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "getSaleOrderStatusList");
            List<dynamic> resultA = new SaleOrderService().GetDynamicList(ps);

            return resultA;
        }

        public static List<dynamic> GetIncotermList()
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "getIncotermList");
            List<dynamic> resultA = new SaleOrderService().GetDynamicList(ps);

            return resultA;
        }

        public static List<dynamic> getPendingStatusList()
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ps.Parameter("ActionType", "getPendingStatus");
            List<dynamic> resultA = new SaleOrderService().GetDynamicList(ps);

            resultA.Insert(0, new { value = "", text = "全部" });
            return resultA;
        }

        internal static string ExportExcel(ParamSP ps)
        {
            //POID,PO,PODate,Customer,POLineNo,CustPN,SuppPN,CDesc,CSpec,ReqDate,Qty,Unit,UnitPrice,TaxFlag,Amount,Incoterms,Currency,POStatus,CFMFlag
            string filename = "SaleOrderList.xlsx";
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            DataTable dt = new SaleOrderService().StoredProcedureDS(ps).Tables[0];
            dt.Columns.Remove("POID");

            double[] titleWidth = { 12, 12, 12, 6, 15, 20, 15, 20, 12, 8, 8, 10, 10, 12,12,10,12,10 };
            string[] title = "客户订单号,报价日期,客户,项次,客户料号,制造商料号,品名,规格,交期,数量,单位,单价,含税,总金额,付款条件,币别,订单状态,确认".Split(',');

            IList<string> BuyOrderList = new List<string>();

            foreach (DataRow dr in dt.Rows)
            {
                if (!BuyOrderList.Contains(dr["PO"].ToString()) && !string.IsNullOrEmpty(dr["CustPN"].ToString()))
                { BuyOrderList.Add(dr["PO"].ToString()); }
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
            string PO = "", PODate = "", Customer = "";
            foreach (string item in BuyOrderList)
            {
                PO = item; DataRow[] drDetail = dt.Select("PO='" + PO + "' and len(CustPN)>1");
                PODate = drDetail[0]["PODate"].ToString(); Customer = drDetail[0]["Customer"].ToString();

                for (int i = 0; i < drDetail.Length; i++)
                {
                    using (var grid = myExcel.myExcelST.Cells[startCell])
                    {
                        if (i == 0)
                        {
                            grid.Value = PO; grid.Style.Font.Color.SetColor(Color.Blue);
                            grid.Offset(0, 1).Value = DateTime.Parse(PODate); grid.Offset(0, 1).Style.Numberformat.Format = "yyyy/MM/dd";
                            grid.Offset(0, 2).Value = Customer;

                            for (int p = 0; p < 3; p++)
                            {
                                grid.Offset(0, p).Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                grid.Offset(0, p).Style.Fill.PatternType = ExcelFillStyle.Solid;
                                grid.Offset(0, p).Style.Fill.BackgroundColor.SetColor(Color.Cyan);
                            }
                        }

                        //POLineNo,CustPN,SuppPN,CDesc,CSpec,ReqDate,Qty,Unit,UnitPrice,TaxFlag,Amount,Incoterms,Currency,POStatus,CFMFlag
                        grid.Offset(0, 3).Value = drDetail[i]["POLineNo"].ToString(); grid.Offset(0, 3).Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        grid.Offset(0, 4).Value = drDetail[i]["CustPN"].ToString(); 
                        grid.Offset(0, 5).Value = drDetail[i]["SuppPN"].ToString();
                        grid.Offset(0, 6).Value = drDetail[i]["CDesc"].ToString();
                        grid.Offset(0, 7).Value = drDetail[i]["CSpec"].ToString();
                        grid.Offset(0, 8).Value = DateTime.Parse(drDetail[i]["ReqDate"].ToString()); grid.Offset(0, 8).Style.Numberformat.Format = "yyyy/MM/dd";
                        grid.Offset(0, 9).Value = decimal.Parse(drDetail[i]["Qty"].ToString()); grid.Offset(0, 9).Style.Numberformat.Format = "#,##0";
                        grid.Offset(0, 10).Value = drDetail[i]["Unit"].ToString();
                        grid.Offset(0, 11).Value = decimal.Parse(drDetail[i]["UnitPrice"].ToString()); grid.Offset(0, 11).Style.Numberformat.Format = "#,##0.00";
                        grid.Offset(0, 12).Value = drDetail[i]["TaxFlag"].ToString();
                        grid.Offset(0, 13).Formula = grid.Offset(0, 9).Address + "*" + grid.Offset(0, 11).Address; grid.Offset(0, 13).Style.Numberformat.Format = "#,##0.00";
                        grid.Offset(0, 14).Value = drDetail[i]["Incoterms"].ToString();
                        grid.Offset(0, 15).Value = drDetail[i]["Currency"].ToString();
                        grid.Offset(0, 16).Value = drDetail[i]["POStatus"].ToString();
                        grid.Offset(0, 17).Value = drDetail[i]["CFMFlag"].ToString();

                        for (int k = 3; k <= 17; k++)
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
            myExcel.myExcelST.HeaderFooter.OddFooter.CenteredText = "EC Tek SaleOrder Detail List";

            myExcel.myExcelST.PrinterSettings.Orientation = eOrientation.Landscape;
            myExcel.myExcelST.PrinterSettings.FitToPage = true;
            myExcel.myExcelST.PrinterSettings.FitToWidth = 1;
            myExcel.myExcelST.PrinterSettings.FitToHeight = 0;

            myExcel.myExcelST.View.ZoomScale = 100;

            myExcel.SaveXls(path);
            myExcel = null;
            return "/FJLBS/TempFiles/" + filename;
        }

        internal static string ExportSOPendingExcel(ParamSP ps)
        {
            //POID,PO,PODate,POStatus,Customer,CustPN,CDesc,CSpec,OrderQty,Currency,UnitPrice,ShipQty,RemainQty,ShipNo,ShipDate,ShipStatus,PendingStatus
            //        PO,PO日期,PO状态,客户,客户料号,品名,规格,PO数量,币别,PO单价,已出数量,PO余量,出货单号,出货日期,出货单状态,出货状态
            string filename = "SO-ShipingList.xlsx";
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            DataTable dt = new SaleOrderService().StoredProcedureDS(ps).Tables[0];
            dt.Columns.Remove("POID");

            double[] titleWidth = { 12, 12, 10, 12, 20, 15, 20, 10, 8, 8, 8, 10, 15, 12,12,15};
            string[] title = "PO,PO日期,PO状态,客户,客户料号,品名,规格,PO数量,币别,PO单价,已出数量,PO余量,出货单号,出货日期,出货单状态,出货状态".Split(',');

            IList<string> BuyOrderList = new List<string>();

            foreach (DataRow dr in dt.Rows)
            {
                if (!BuyOrderList.Contains(dr["PO"].ToString()) && !string.IsNullOrEmpty(dr["CustPN"].ToString()))
                { BuyOrderList.Add(dr["PO"].ToString()); }
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
            string PO = "", PODate = "", Customer = "", POStatus="",ShipDate="",PendingStatus="";
            foreach (string item in BuyOrderList)
            {
                PO = item; DataRow[] drDetail = dt.Select("PO='" + PO + "' and len(CustPN)>1");
                PODate = drDetail[0]["PODate"].ToString(); POStatus = drDetail[0]["POStatus"].ToString(); Customer = drDetail[0]["Customer"].ToString();

                for (int i = 0; i < drDetail.Length; i++)
                {
                    using (var grid = myExcel.myExcelST.Cells[startCell])
                    {
                        if (i == 0)
                        {
                            grid.Value = PO; grid.Style.Font.Color.SetColor(Color.Blue);
                            grid.Offset(0, 1).Value = DateTime.Parse(PODate); grid.Offset(0, 1).Style.Numberformat.Format = "yyyy/MM/dd";
                            grid.Offset(0, 2).Value = POStatus; grid.Offset(0, 3).Value = Customer;

                            for (int p = 0; p < 4; p++)
                            {
                                grid.Offset(0, p).Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                grid.Offset(0, p).Style.Fill.PatternType = ExcelFillStyle.Solid;
                                grid.Offset(0, p).Style.Fill.BackgroundColor.SetColor(Color.Cyan);
                            }
                        }

                        //CustPN,CDesc,CSpec,OrderQty,Currency,UnitPrice,ShipQty,RemainQty,ShipNo,ShipDate,ShipStatus,PendingStatus
                        grid.Offset(0, 4).Value = drDetail[i]["CustPN"].ToString(); grid.Offset(0, 4).Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        grid.Offset(0, 5).Value = drDetail[i]["CDesc"].ToString();
                        grid.Offset(0, 6).Value = drDetail[i]["CSpec"].ToString();
                        grid.Offset(0, 7).Value = decimal.Parse(drDetail[i]["OrderQty"].ToString()); grid.Offset(0, 7).Style.Numberformat.Format = "#,##0";
                        grid.Offset(0, 8).Value = drDetail[i]["Currency"].ToString();
                        grid.Offset(0, 9).Value = decimal.Parse(drDetail[i]["UnitPrice"].ToString()); grid.Offset(0, 9).Style.Numberformat.Format = "#,##0.00";
                        grid.Offset(0, 10).Value = decimal.Parse(drDetail[i]["ShipQty"].ToString()); grid.Offset(0, 10).Style.Numberformat.Format = "#,##0";
                        grid.Offset(0, 11).Formula = grid.Offset(0, 7).Address + "-" + grid.Offset(0, 10).Address; grid.Offset(0, 11).Style.Numberformat.Format = "#,##0";
                        grid.Offset(0, 12).Value = drDetail[i]["ShipNo"].ToString();

                        if (drDetail[i]["ShipDate"] != null)
                        {
                            ShipDate = drDetail[i]["ShipDate"].ToString();
                            if (ShipDate.Length > 0)
                            { grid.Offset(0, 13).Value = DateTime.Parse(ShipDate); grid.Offset(0, 13).Style.Numberformat.Format = "yyyy/MM/dd"; }
                        }

                        grid.Offset(0, 14).Value = drDetail[i]["ShipStatus"].ToString();

                        PendingStatus = drDetail[i]["PendingStatus"].ToString();
                        grid.Offset(0, 15).Value = PendingStatus;
                        if (PendingStatus == "出货异常") { grid.Offset(0, 15).Style.Font.Color.SetColor(Color.Red); }

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
            myExcel.myExcelST.HeaderFooter.OddFooter.CenteredText = "EC Tek SaleOrder-Shiping Check List";

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
    public class SaleOrder : ModelBase
    {
        [PrimaryKey]
        public int POID { get; set; }
        public string PO { get; set; }
        public string Customer { get; set; }
        public DateTime? PODate { get; set; }
        public string Incoterms { get; set; }
        public string CustCode { get; set; }
        public string Consignee { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string Destination { get; set; }
        public string Country { get; set; }
        public string POStatus { get; set; }
        public string Currency { get; set; }
        public string Remark { get; set; }
        //public string PriceType { get; set; }
        public string InputBy { get; set; }
        public DateTime? InputDT { get; set; }
        public string CFMFlag { get; set; }
        //public DateTime? CFMDate { get; set; }

        /*POID,PO,Customer,PODate,Incoterms,CustCode,Consignee,Address,Contact,Destination,Country, 
                POStatus,Currency,InputBy,InputDT,CFMFlag 
               */
        /*POID, POLineNo, CustPN, SuppPN, CDesc, CSpec, ReqDate, Qty, Unit, UnitPrice, UnitPriceT, Currency, Amount*/

    }
}