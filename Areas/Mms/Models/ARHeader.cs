using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;
using Zephyr.Core;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ECGroup.Models
{
    [Module("EC")]
    public class ARListService : ServiceBase<ARList>
    {
        public static string strSP = "DG_ARHeader_sp";
        internal IList<ARList> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<ARList> myList = db.Select<ARList>(strSQL).QueryMany();

            return myList;
        }

        public static List<dynamic> getARStatusList()
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "GetARStatusList");
            List<dynamic> resultA = new ARListService().GetDynamicList(ps);
            resultA.Insert(0, new {value="",text="全部" });
            return resultA; 
        }
       
        internal static string ExportARListExcel(ParamSP ps)
        {
            string fileTemp = Zephyr.Utils.ZHttp.RootPhysicalPath + @"Content\Template\ARCheckList.xlsx";
            DataSet ds = new ARListService().StoredProcedureDS(ps);
            EPPlusNew myExcel = new EPPlusNew(fileTemp);

            DataRow dtHeaderRow = ds.Tables[0].Rows[0]; //A.custCode,A.Customer,B.CustAdd,A.BatchNo,Currency
            DataTable dtSub = ds.Tables[1];

            string BatchNo = dtHeaderRow["BatchNo"].ToString();
            string Currency = dtHeaderRow["Currency"].ToString();
            string filename = BatchNo + ".xlsx";
            string fileURL = "/FJLBS/TempFiles/" + filename;  // 格式： /FJLBS/TempFiles/myExcel.xlsx
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            using (var cK2 = myExcel.myExcelST.Cells["I2"])
            {
                cK2.Value = BatchNo;
                cK2.Offset(1, 0).Value = DateTime.Today.ToString("yyyy-MM-dd");
                cK2.Style.Font.Bold = true;
                cK2.Style.Font.Size = 12;
                cK2.Offset(2, 0).Value = Currency;
            }

            myExcel.myExcelST.Cells["B6"].Value = dtHeaderRow["CustName"].ToString();
            myExcel.myExcelST.Cells["B7"].Value = dtHeaderRow["CustAdd"].ToString(); myExcel.myExcelST.Cells["B8"].Value = "";

            myExcel.myExcelST.Cells["H7"].Value = "To:" + dtHeaderRow["Contact"].ToString();
            myExcel.myExcelST.Cells["H8"].Value = " Tel:" + dtHeaderRow["Tel"].ToString();
            //写明细PO,CustPN,ShipDate,ShipNo,Qty,Unit,UnitPrice,Amount,
            string refCell = "A12", cellQty = "", cellPrice = "";
            for (int i = 0; i < dtSub.Rows.Count; i++)
            {
                DataRow drSub = dtSub.Rows[i];

                myExcel.myExcelST.Cells[refCell].Value = (i + 1).ToString().PadLeft(3,'0');
                myExcel.myExcelST.Cells[refCell].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                myExcel.myExcelST.Cells[refCell].Offset(0, 1).Value = drSub["PO"].ToString();
                myExcel.myExcelST.Cells[refCell].Offset(0, 2).Value = drSub["CustPN"].ToString();
                myExcel.myExcelST.Cells[refCell].Offset(0, 3).Value = drSub["ShipDate"].ToString();
                myExcel.myExcelST.Cells[refCell].Offset(0, 4).Value = drSub["ShipNo"].ToString();

                myExcel.myExcelST.Cells[refCell].Offset(0, 1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                myExcel.myExcelST.Cells[refCell].Offset(0, 2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                myExcel.myExcelST.Cells[refCell].Offset(0, 3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                cellQty = myExcel.myExcelST.Cells[refCell].Offset(0, 5).Address;
                myExcel.myExcelST.Cells[cellQty].Value = decimal.Parse(drSub["Qty"].ToString());
                myExcel.myExcelST.Cells[cellQty].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                myExcel.myExcelST.Cells[cellQty].Style.Numberformat.Format = "#,##0";

                myExcel.myExcelST.Cells[refCell].Offset(0, 6).Value = drSub["Unit"].ToString();

                cellPrice = myExcel.myExcelST.Cells[refCell].Offset(0, 7).Address;
                myExcel.myExcelST.Cells[cellPrice].Value = decimal.Parse(drSub["UnitPrice"].ToString());
                myExcel.myExcelST.Cells[cellPrice].Style.Numberformat.Format = "#,##0.000";

                myExcel.myExcelST.Cells[refCell].Offset(0, 8).Formula = cellQty + "*" + cellPrice;
                myExcel.myExcelST.Cells[refCell].Offset(0, 8).Style.Numberformat.Format = "#,##0.00";
                myExcel.myExcelST.Cells[refCell].Offset(0, 8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                refCell = myExcel.myExcelST.Cells[refCell].Offset(1, 0).Address;
            }

            //汇总信息
            string cellSum1 = myExcel.myExcelST.Cells["A12"].Offset(0, 8).Address;
            string cellSum2 = myExcel.myExcelST.Cells[refCell].Offset(-1, 8).Address;

            myExcel.myExcelST.Cells[refCell].Offset(0, 7).Value = "TOTAL:";
            myExcel.myExcelST.Cells[refCell].Offset(0, 8).Formula = string.Format("SUBTOTAL(9,{0})", new ExcelAddress(cellSum1 + ":" + cellSum2).Address);
            myExcel.myExcelST.Cells[refCell].Offset(0, 8).Style.Numberformat.Format = "#,##0.00";

            //设定表格主体
            using (var grid = myExcel.myExcelST.Cells["A12:" + myExcel.myExcelST.Cells[refCell].Offset(-1, 8).Address])
            {
                grid.Style.Border.Left.Style = ExcelBorderStyle.Thin; grid.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                grid.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }   
      
             //设定表格边框
            using (var grid = myExcel.myExcelST.Cells["A12:" + myExcel.myExcelST.Cells[refCell].Offset(-1, 8).Address])
            {
                grid.Style.Border.BorderAround(ExcelBorderStyle.Medium);
            }

            cellSum1 = myExcel.myExcelST.Cells[refCell].Offset(3, 5).Address;
            cellSum2 = myExcel.myExcelST.Cells[refCell].Offset(3, 8).Address;
            myExcel.myExcelST.Cells[cellSum1 + ":" + cellSum2].Merge = true;

            using (var grid = myExcel.myExcelST.Cells[cellSum1])
            {
                grid.Value = "E&C Industrial (HongKong) Co.Ltd";
                grid.Style.Font.Bold = true;
                grid.Style.Font.Size = 16;
                grid.Style.Font.Name = "Times New Roman";
                grid.Style.Font.UnderLineType = ExcelUnderLineType.DoubleAccounting;
            }            

            //设定行高
            for (int i = 0; i < dtSub.Rows.Count + 12; i++)
            {
                myExcel.myExcelST.Row(13 + i).Height = 21;
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
    }

    public class ARList : ModelBase
    {
        [PrimaryKey]
        public int ARID { get; set; }
        public string ShipNo { get; set; }
        public string CustCode { get; set; }
        public string Customer { get; set; }
        public string Currency { get; set; }
        public string Amount { get; set; }
        public DateTime? DueDate { get; set; }
        public string ARStatus { get; set; }
        public string BatchNo { get; set; }
        public DateTime? BatchDate { get; set; }
        public string RcvNo { get; set; }
        public DateTime? RcvDate { get; set; }
        public string Remarks { get; set; }
        public string InputBy { get; set; }

         /*ARID, ShipNo, RcvDate, CustCode, Customer, Currency, Amount, DueDate, ARStatus, BatchNo, BatchDate, 
                Remarks, RcvNo, InputBy */
    }
}