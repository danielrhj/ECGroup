using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Zephyr.Core;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ECGroup.Models
{
    [Module("EC")]
    public class ProfitService : ServiceBase<Profit>
    {
        public static string strSP = "DG_Profit_sp";
        internal IList<Profit> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<Profit> myList = db.Select<Profit>(strSQL).QueryMany();

            return myList;
        }

        public static List<dynamic> getShipingStatusList()
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "getShipingStatusList");
            List<dynamic> resultA = new ProfitService().GetDynamicList(ps);
            resultA.Insert(0, new {value="",text="全部" });
            return resultA;
        }

        internal static string ExportProfitExcel(ParamSP ps,string Type)
        {           
           //PO	PODate	Customer	CustPN	SuppPN	CDesc	Qty	UnitPrice	PriceType	Amount	Currency	ReqDate	ShipQty	ShipDate	BuyPrice	BuyAmount	ShipStatus
            //订单号码	料号	品牌	MPN	描述	数量	价格	Amount	订单日期	要求纳期	采购价	采购额	供应商	订购数	订购时间	交货期	状态	实际出货时间
            string ActionType = ps.GetData().Parameter["ActionType"].ToString();

            if (ActionType == "CreateProfitExcelSimple")
            {
                DataTable dt = new ProfitService().StoredProcedureDS(ps).Tables[0];
                dt.Columns["PO"].ColumnName = "订单号码"; dt.Columns["CustPN"].ColumnName = "料号";
                dt.Columns["SuppPN"].ColumnName = "MPN"; dt.Columns["CDesc"].ColumnName = "描述";
                dt.Columns["Qty"].ColumnName = "数量"; dt.Columns["UnitPrice"].ColumnName = "价格";
                dt.Columns["PODate"].ColumnName = "订单日期"; dt.Columns["ReqDate"].ColumnName = "要求纳期";
                dt.Columns["BuyPrice"].ColumnName = "采购价"; dt.Columns["BuyAmount"].ColumnName = "采购额";
                dt.Columns["ShipStatus"].ColumnName = "状态"; dt.Columns["ShipDate"].ColumnName = "实际出货时间";

                EPPlusNew myExcel = new EPPlusNew("");

                string filename = "profitSimple.xlsx";
                string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

                myExcel.ExportExcel(dt, path);
                myExcel = null;
                return "/FJLBS/TempFiles/" + filename;
            }
            else
            {
                return ExportProfitExcelFull(ps);
            }
        }

        private static string ExportProfitExcelFull(ParamSP ps)
        {   
            string filename = "profitDetail.xlsx";
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            //报表中的状态为采购单的交货状态,不是客户订单的出货状态
            double[] titleWidth = { 15, 20, 15, 20, 20, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 15,15 };
            string[] title = { "订单号码","料号","品牌","MPN","描述","数量","价格","Amount","订单日期","要求纳期","采购价","采购额","供应商","订购数","订购时间","交货期","状态","实际出货时间"};

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

            myExcel.myExcelST.Cells[1, 1, 1, titleWidth.Length].AutoFilter = true;
            DataSet ds = new ProfitService().StoredProcedureDS(ps);
            DataTable dtPOList = ds.Tables[0]; DataTable dtPODetail = ds.Tables[1];

            string startCell="A2";
            string PO="",PODate="",CustPN="",sumStartCell="";
            int intPOLines = 0;
            foreach (DataRow dr in dtPOList.Rows)
            {
                intPOLines=0;
                PO=dr["PO"].ToString();PODate=dr["PODate"].ToString();
                DataRow[] drDetail = dtPODetail.Select("PO='" + PO + "'"); intPOLines = drDetail.Length;
                for (int i = 0; i < intPOLines; i++)
                {
                    CustPN=drDetail[i]["CustPN"].ToString();
                    using (var grid = myExcel.myExcelST.Cells[startCell])
                    {
                        if (i == 0)
                        { 
                            grid.Value = PO; grid.Style.Font.Color.SetColor(Color.Blue);                      
                        }
                        grid.Offset(0,1).Value=CustPN;
                        grid.Offset(0, 3).Value = drDetail[i]["SuppPN"].ToString();
                        grid.Offset(0, 4).Value = drDetail[i]["CDesc"].ToString();
                        grid.Offset(0, 5).Value = decimal.Parse(drDetail[i]["Qty"].ToString()); grid.Offset(0, 5).Style.Numberformat.Format = "#,##0";
                        grid.Offset(0, 6).Value = decimal.Parse(drDetail[i]["UnitPrice"].ToString()); grid.Offset(0, 6).Style.Numberformat.Format = "#,##0.00";
                        grid.Offset(0, 7).Formula = grid.Offset(0, 5).Address + "*" + grid.Offset(0, 6).Address; grid.Offset(0, 7).Style.Numberformat.Format = "#,##0.00";
                        grid.Offset(0, 8).Value = DateTime.Parse(PODate); grid.Offset(0, 8).Style.Numberformat.Format = "yyyy/MM/dd";  
                        grid.Offset(0, 9).Value = DateTime.Parse(drDetail[i]["ReqDate"].ToString()); grid.Offset(0, 9).Style.Numberformat.Format = "yyyy/MM/dd";

                        //根据PO和CustPN获取采购信息
                        DataRow drBuyInfo = GetBuyInfo(PO,CustPN);
                        if (drBuyInfo != null)
                        {
                            grid.Offset(0, 10).Value = decimal.Parse(drBuyInfo["RcvPrice"].ToString()); grid.Offset(0, 6).Style.Numberformat.Format = "#,##0.00";
                            grid.Offset(0, 11).Formula = grid.Offset(0, 5).Address + "*" + grid.Offset(0, 10).Address; grid.Offset(0, 11).Style.Numberformat.Format = "#,##0.00";
                            grid.Offset(0, 12).Value = drBuyInfo["Supplier"].ToString();
                            grid.Offset(0, 13).Value = decimal.Parse(drBuyInfo["BuyQty"].ToString()); grid.Offset(0, 13).Style.Numberformat.Format = "#,##0";

                            if (!string.IsNullOrEmpty(drBuyInfo["BuyDate"].ToString()))
                            { grid.Offset(0, 14).Value = DateTime.Parse(drBuyInfo["BuyDate"].ToString()); grid.Offset(0, 14).Style.Numberformat.Format = "yyyy/MM/dd"; }

                            if (!string.IsNullOrEmpty(drBuyInfo["BuyDate"].ToString()))
                            { grid.Offset(0, 14).Value = DateTime.Parse(drBuyInfo["BuyDate"].ToString()); grid.Offset(0, 14).Style.Numberformat.Format = "yyyy/MM/dd"; }

                            if (!string.IsNullOrEmpty(drBuyInfo["ReqDate"].ToString()))
                            { grid.Offset(0, 15).Value = DateTime.Parse(drBuyInfo["ReqDate"].ToString()); grid.Offset(0, 15).Style.Numberformat.Format = "yyyy/MM/dd"; }

                            grid.Offset(0, 16).Value = string.IsNullOrEmpty(drBuyInfo["DODate"].ToString()) ? "未完成" : "已完成";

                            if (!string.IsNullOrEmpty(drBuyInfo["DODate"].ToString()))
                            { grid.Offset(0, 17).Value = DateTime.Parse(drBuyInfo["DODate"].ToString()); grid.Offset(0, 17).Style.Numberformat.Format = "yyyy/MM/dd"; }
                        }
                        else
                        { grid.Offset(0, 16).Value = "采购单尚未入库."; }

                    }
                    startCell = myExcel.myExcelST.Cells[startCell].Offset(1, 0).Address;                    
                }

                myExcel.myExcelST.Cells[startCell + ":" + myExcel.myExcelST.Cells[startCell].Offset(0, titleWidth.Length-1).Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;

                using (var grid = myExcel.myExcelST.Cells[startCell])
                {
                    //订单总额
                    sumStartCell = grid.Offset(-intPOLines, 7).Address; grid.Offset(0, 6).Value = "销售额:";
                    grid.Offset(0, 7).Formula = "sum(" + sumStartCell + ":" + grid.Offset(-1, 7).Address + ")"; //grid.Offset(0, 7).Style.Font.Color.SetColor(Color.Blue);   

                    //成本
                    sumStartCell = grid.Offset(-intPOLines, 11).Address; grid.Offset(0, 10).Value = "采购额:";
                    grid.Offset(0, 11).Formula = "sum(" + sumStartCell + ":" + grid.Offset(-1, 11).Address + ")";

                    grid.Offset(0, 13).Value = "利润额:"; grid.Offset(0, 13).Style.Font.Color.SetColor(Color.White);
                    grid.Offset(0, 13).Style.Fill.PatternType = ExcelFillStyle.Solid; grid.Offset(0, 13).Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    
                    grid.Offset(0, 14).Formula = grid.Offset(0, 7).Address + "-" + grid.Offset(0, 11).Address;
                    grid.Offset(0, 14).Style.Font.Color.SetColor(Color.Green); grid.Offset(0, 14).Style.Font.Bold = true;

                    grid.Offset(0, 15).Value = "利润率:"; grid.Offset(0, 15).Style.Font.Color.SetColor(Color.White);
                    grid.Offset(0, 15).Style.Fill.PatternType = ExcelFillStyle.Solid; grid.Offset(0, 15).Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

                    grid.Offset(0, 16).Formula = grid.Offset(0, 14).Address + "/" + grid.Offset(0, 7).Address;
                    grid.Offset(0, 16).Style.Numberformat.Format = "0.0%";
                    grid.Offset(0, 16).Style.Font.Color.SetColor(Color.Green); grid.Offset(0, 16).Style.Font.Bold = true;

                    startCell = grid.Offset(1, 0).Address;
                }
            }

            myExcel.myExcelST.HeaderFooter.OddFooter.RightAlignedText =
                string.Format("Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
            myExcel.myExcelST.HeaderFooter.OddFooter.CenteredText = "EC Tek Profit Analysis";

            myExcel.myExcelST.PrinterSettings.Orientation = eOrientation.Landscape;
            myExcel.myExcelST.PrinterSettings.FitToPage = true;
            myExcel.myExcelST.PrinterSettings.FitToWidth = 1;
            myExcel.myExcelST.PrinterSettings.FitToHeight = 0;

            myExcel.myExcelST.View.ZoomScale = 100;
            myExcel.SaveXls(path);
            myExcel = null;
            return "/FJLBS/TempFiles/" + filename;
        } 
      
        private static DataRow GetBuyInfo(string PO,string CustPN)
        {
            try
            {
                ParamSP ps = new ParamSP().Name(ProfitService.strSP);
                ps.Parameter("ActionType", "GetBuyInfoByPO");
                ps.Parameter("PO", PO);
                ps.Parameter("CustPN", CustPN);

                DataTable dt = new ProfitService().StoredProcedureDT(ps);

                if (dt != null && dt.Rows.Count > 0)
                { return dt.Rows[0]; }
                else { return null; }
            }
            catch (Exception ex)
            { return null; }
        }
    }
    public class Profit : ModelBase
    {

    }
}