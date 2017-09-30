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
    public class ShipingService : ServiceBase<Shiping>
    {
        public static string strSP = "DG_Shiping_sp";
        internal IList<Shiping> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<Shiping> myList = db.Select<Shiping>(strSQL).QueryMany();

            return myList;
        }

        public static List<dynamic> getShipingStatusList()
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "getShipingStatusList");
            List<dynamic> resultA = new ShipingService().GetDynamicList(ps);

            return resultA;
        }

        public static List<dynamic> GetIncotermList()
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "getIncotermList");
            List<dynamic> resultA = new ShipingService().GetDynamicList(ps);

            return resultA;
        }

        internal static string ExportSHExcel(ParamSP ps)
        {
            string fileTemp = Zephyr.Utils.ZHttp.RootPhysicalPath + @"Content\Template\PKL.xlsx";
            DataSet ds = new ShipingService().StoredProcedureDS(ps);
            EPPlusNew myExcel = new EPPlusNew(fileTemp);

            DataRow dtHeaderRow = ds.Tables[0].Rows[0]; //ShipID,ShipNo,CustName,CustAdd,Contact,Tel,CellNo,Incoterms,Currency
            DataTable dtSub = ds.Tables[1];

            string ShipNo = dtHeaderRow["ShipNo"].ToString();
            string Currency = dtHeaderRow["Currency"].ToString();
            string filename = ShipNo + ".xlsx";
            string fileURL = "/FJLBS/TempFiles/" + filename;  // 格式： /FJLBS/TempFiles/myExcel.xlsx
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            using (var cK2 = myExcel.myExcelST.Cells["K2"])
            {
                myExcel.myExcelST.Cells["K2"].Value = ShipNo;
                myExcel.myExcelST.Cells["K2"].Style.Font.Bold = true;
                myExcel.myExcelST.Cells["K2"].Style.Font.Size = 12;
            }

            using (var cK2 = myExcel.myExcelST.Cells["K3"])
            {
                cK2.Value = DateTime.Today.ToString("yyyy-MM-dd");
                cK2.Style.Font.Bold = true;
                cK2.Style.Font.Size = 12;
            }

            using (var cK2 = myExcel.myExcelST.Cells["B6"])
            {
                cK2.Value = dtHeaderRow["CustName"].ToString();
                cK2.Style.Font.Size = 12;
            }

            using (var cK2 = myExcel.myExcelST.Cells["B7"])
            {
                cK2.Value = dtHeaderRow["CustAdd"].ToString();
                cK2.Style.Font.Size = 12;
            }

            using (var cK2 = myExcel.myExcelST.Cells["I7"])
            {
                cK2.Value = "TEL: " + dtHeaderRow["Tel"].ToString();
                cK2.Style.Font.Size = 12;
            }

            using (var cK2 = myExcel.myExcelST.Cells["I8"])
            {
                cK2.Value = "ATT: " + dtHeaderRow["Contact"].ToString(); 
                cK2.Style.Font.Size = 12;
            }

            using (var cK2 = myExcel.myExcelST.Cells["K9"])
            {
                cK2.Value = "(" + Currency + ")交易"; 
                cK2.Style.Font.Size = 12;
            }

            ////写明细 Brand,PO,CustPN,CDesc,Qty,Unit
            string refCell = "A12", cellQty = "", cellPrice = "", strNo = "", strCellDesc1 = "", strCellDesc2 = "";
            for (int i = 0; i < dtSub.Rows.Count; i++)
            {
                strNo = (i + 1).ToString().PadLeft(3, '0');
                DataRow drSub = dtSub.Rows[i];

                myExcel.myExcelST.Cells[refCell].Value = strNo;
                myExcel.myExcelST.Cells[refCell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                myExcel.myExcelST.Cells[refCell].Offset(0, 1).Value = drSub["PO"].ToString();
                myExcel.myExcelST.Cells[refCell].Offset(0, 2).Value = drSub["CustPN"].ToString();

                strCellDesc1 = myExcel.myExcelST.Cells[refCell].Offset(0, 3).Address;
                strCellDesc2 = myExcel.myExcelST.Cells[refCell].Offset(0, 5).Address;
                myExcel.myExcelST.Cells[strCellDesc1].Value = drSub["CDesc"].ToString();

                cellQty = myExcel.myExcelST.Cells[refCell].Offset(0, 6).Address;
                myExcel.myExcelST.Cells[cellQty].Value = decimal.Parse(drSub["Qty"].ToString());
                myExcel.myExcelST.Cells[cellQty].Style.Numberformat.Format = "#,##0";
                myExcel.myExcelST.Cells[cellQty].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                myExcel.myExcelST.Cells[refCell].Offset(0, 7).Value = drSub["Unit"].ToString();
                myExcel.myExcelST.Cells[refCell].Offset(0, 8).Value = drSub["Brand"].ToString();

                myExcel.myExcelST.Cells[strCellDesc1 + ":" + strCellDesc2].Merge = true;

                strCellDesc1 = myExcel.myExcelST.Cells[refCell].Offset(0, 8).Address;
                strCellDesc2 = myExcel.myExcelST.Cells[refCell].Offset(0, 9).Address;
                myExcel.myExcelST.Cells[strCellDesc1 + ":" + strCellDesc2].Merge = true;

                strCellDesc1 = myExcel.myExcelST.Cells[refCell].Offset(0, 10).Address;
                strCellDesc2 = myExcel.myExcelST.Cells[refCell].Offset(0, 11).Address;
                myExcel.myExcelST.Cells[strCellDesc1 + ":" + strCellDesc2].Merge = true;

                
                using(var cells = myExcel.myExcelST.Cells[refCell + ":" + myExcel.myExcelST.Cells[refCell].Offset(0, 11).Address])
                {
                    int RowFrom=cells.Start.Row;int ColFrom=cells.Start.Column;
                    int RowEnd=cells.End.Row;int ColEnd=cells.End.Column;
                    
                    for(int j=RowFrom;j<=RowEnd;j++)
                    {
                        for(int k=ColFrom;k<=ColEnd;k++)
                    {
                        myExcel.myExcelST.Cells[j,k].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    }
                }
                //myExcel.myExcelST.Cells[refCell + ":" + myExcel.myExcelST.Cells[refCell].Offset(0, 11).Address].Style.Border.BorderAround(ExcelBorderStyle.Medium);
                refCell = myExcel.myExcelST.Cells[refCell].Offset(1, 0).Address;
            }

            //汇总信息
            string cellSum1 = myExcel.myExcelST.Cells["A12"].Offset(0, 6).Address;
            string cellSum2 = myExcel.myExcelST.Cells[refCell].Offset(-1, 6).Address;
            myExcel.myExcelST.Cells[refCell].Offset(0, 6).Formula = string.Format("SUBTOTAL(9,{0})", new ExcelAddress(cellSum1 + ":" + cellSum2).Address);
            myExcel.myExcelST.Cells[refCell].Offset(0, 6).Style.Numberformat.Format = "#,##0";
            myExcel.myExcelST.Cells[refCell].Offset(0, 6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            using (var grid = myExcel.myExcelST.Cells[refCell].Offset(0, 5))
            {
                grid.Value = "TOTAL:"; grid.Style.Font.Size = 12; grid.Style.Font.Name = "Arial"; grid.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                grid.Offset(0, 1).Style.Font.Size = 12; grid.Offset(0, 1).Style.Font.Name = "Arial";
            }

            cellSum1 = myExcel.myExcelST.Cells[cellSum1].Offset(0, 4).Address;
            cellSum2 = myExcel.myExcelST.Cells[cellSum2].Offset(0, 4).Address;
            myExcel.myExcelST.Cells[cellSum2].Offset(1, 0).Formula = string.Format("SUBTOTAL(9,{0})", new ExcelAddress(cellSum1 + ":" + cellSum2).Address);
            myExcel.myExcelST.Cells[cellSum2].Offset(1, 0).Style.Numberformat.Format = "#,##0";
            myExcel.myExcelST.Cells[cellSum2].Offset(1, 0).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;          

            //设定表格边框
            using (var grid = myExcel.myExcelST.Cells["A12:" + myExcel.myExcelST.Cells[refCell].Offset(-1, 11).Address])
            {
                grid.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                grid.Style.Font.Size = 12; grid.Style.Font.Name = "Arial";
            }

            //写最后一行 
            using (var grid = myExcel.myExcelST.Cells[refCell].Offset(1, 6))
            {
                grid.Value = "E&C Technology(Shenzhen) Co.Ltd."; grid.Style.Font.Size = 16; grid.Style.Font.Name = "Arial"; grid.Style.Font.Bold = true;
                grid.Style.Font.UnderLine = true; grid.Style.Font.UnderLineType = ExcelUnderLineType.DoubleAccounting;
                myExcel.myExcelST.Cells[grid.Address + ":" + grid.Offset(0, 5).Address].Merge = true;
    
                grid.Offset(2, 2).Value = "MANAGER"; grid.Offset(2, 2).Style.Font.Size = 16; grid.Offset(2, 2).Style.Font.Name = "Arial"; grid.Offset(2, 2).Style.Font.Bold = true;
            }

            //设定行高
            for (int i = 0; i < dtSub.Rows.Count + 1; i++)
            {
                myExcel.myExcelST.Row(12 + i).Height = 21;
            }

            myExcel.myExcelST.View.TabSelected = true;
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
            //ShipID,ShipNo, ShipDate,Customer,CustPN,SuppPN,CDesc,CSpec,Qty,Unit,Currency,Amount,Incoterms,ShipStatus,BLNo
            string filename = "ShipingList.xlsx";
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            DataTable dt = new ShipingService().StoredProcedureDS(ps).Tables[0];

            double[] titleWidth = { 12, 12, 10, 15, 20, 15, 15, 10, 8, 8,8,10, 12, 8, 12};
            string[] title = "出库单号,出库日期,客户,客户料号,制造商料号,品名,规格,数量,单位,单价,币别,总金额,付款条件,出库状态,运单号码".Split(',');

            IList<string> BuyOrderList = new List<string>();

            foreach (DataRow dr in dt.Rows)
            {
                if (!BuyOrderList.Contains(dr["ShipNo"].ToString()) && !string.IsNullOrEmpty(dr["CustPN"].ToString()))
                { BuyOrderList.Add(dr["ShipNo"].ToString()); }
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
            string ShipNo = "", ShipDate = "", Customer = "";
            decimal PendingQty = 0;
            foreach (string item in BuyOrderList)
            {
                ShipNo = item; DataRow[] drDetail = dt.Select("ShipNo='" + ShipNo + "' and len(CustPN)>1");
                ShipDate = drDetail[0]["ShipDate"].ToString(); Customer = drDetail[0]["Customer"].ToString();                

                for (int i = 0; i < drDetail.Length; i++)
                {
                    using (var grid = myExcel.myExcelST.Cells[startCell])
                    {
                        if (i == 0)
                        {
                            grid.Value = ShipNo; grid.Style.Font.Color.SetColor(Color.Blue);
                            grid.Offset(0, 1).Value = DateTime.Parse(ShipDate); grid.Offset(0, 1).Style.Numberformat.Format = "yyyy/MM/dd";
                            grid.Offset(0, 2).Value = Customer; 

                            for (int p = 0; p < 3; p++)
                            {
                                grid.Offset(0, p).Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                grid.Offset(0, p).Style.Fill.PatternType = ExcelFillStyle.Solid;
                                grid.Offset(0, p).Style.Fill.BackgroundColor.SetColor(Color.Cyan);
                            }
                        }

                        //CustPN,SuppPN,CDesc,CSpec,Qty,Unit,Currency,Amount,Incoterms,ShipStatus,BLNo

                        grid.Offset(0, 3).Value = drDetail[i]["CustPN"].ToString(); grid.Offset(0, 3).Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        grid.Offset(0, 4).Value = drDetail[i]["SuppPN"].ToString();
                        grid.Offset(0, 5).Value = drDetail[i]["CDesc"].ToString();
                        grid.Offset(0, 6).Value = drDetail[i]["CSpec"].ToString();
                        grid.Offset(0, 7).Value = decimal.Parse(drDetail[i]["Qty"].ToString()); grid.Offset(0, 7).Style.Numberformat.Format = "#,##0";
                        grid.Offset(0, 8).Value = drDetail[i]["Unit"].ToString();
                        grid.Offset(0, 9).Value = decimal.Parse(drDetail[i]["UnitPrice"].ToString()); grid.Offset(0, 10).Style.Numberformat.Format = "#,##0.00";
                        grid.Offset(0, 10).Value = drDetail[i]["Currency"].ToString();
                        grid.Offset(0, 11).Formula = grid.Offset(0, 7).Address + "*" + grid.Offset(0, 9).Address; grid.Offset(0, 11).Style.Numberformat.Format = "#,##0.00";

                        grid.Offset(0, 12).Value = drDetail[i]["Incoterms"].ToString();
                        grid.Offset(0, 13).Value = drDetail[i]["ShipStatus"].ToString();
                        grid.Offset(0, 14).Value = drDetail[i]["BLNo"].ToString();

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
            myExcel.myExcelST.HeaderFooter.OddFooter.CenteredText = "EC Tek Shiping List";

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
    public class Shiping : ModelBase
    {
        [PrimaryKey]
        public int ShipID { get; set; }
        public string ShipNo { get; set; }
        public DateTime? ShipDate { get; set; }
        public string BLNo { get; set; }
        public string Customer { get; set; }
        public string Incoterms { get; set; }
        public string Currency { get; set; }
        public string CustCode { get; set; }

        public string Consignee { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string Tel { get; set; }
        public string Destination { get; set; }
        public string ShipStatus { get; set; }
        public string Remark { get; set; }
        public string CFMFlag { get; set; }
        public string InputBy { get; set; }
        public DateTime? InputDT { get; set; }

        //public string Country { get; set; }
        //public DateTime? CFMDate { get; set; }

        /*ShipID,ShipNo, ShipDate, BLNo, Customer, Incoterms,Currency, Plts, Ctns, CustCode, Consignee, Address, Contact, 
            Tel, Destination,ShipStatus,Remark, CFMFlag, InputBy, InputDT */
        /*SNO, ShipID, PO, CustPN, SuppPN, CDesc, EDesc, CSpec, ShipDate, Qty, Unit, UnitPrice, Currency  */

    }
}