using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Zephyr.Core;

namespace ECGroup.Models
{
    [Module("EC")]
    public class PartNoService : ServiceBase<PartNo>
    {
        public static string strSP = "DG_BasicInfoPN";
        internal IList<PartNo> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<PartNo> myList = db.Select<PartNo>(strSQL).QueryMany();

            return myList;
        }

        public static List<dynamic> getIdeasSupplierCodeList(string q)
        {
            ParamSP ps = new ParamSP().Name("usp_BasisData");
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "getIdeasSupplierCodeList");
            ps.Parameter("data", q);
            List<dynamic> resultA = new PartNoService().GetDynamicList(ps);

            return resultA;
        }

        public static List<dynamic> getSuppPNList(string q)
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "getSuppPNList");
            ps.Parameter("data", q);
            List<dynamic> resultA = new SupplierService().GetDynamicList(ps);

            return resultA;
        }

        public static List<dynamic> getCustPNList(string q)
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "getCustPNList");
            ps.Parameter("data", q);
            List<dynamic> resultA = new SupplierService().GetDynamicList(ps);

            return resultA;
        }       

        public static List<dynamic> getEPNList()
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ps.Parameter("ActionType", "GetAllEcomPNList");
            List<dynamic> resultA = new SupplierService().GetDynamicList(ps);

            return resultA;
        }

        internal static string ExportExcel(ParamSP ps)
        {
            DataTable dt = new CompanyService().StoredProcedureDS(ps).Tables[0];
            dt.Columns.Remove("PNID"); dt.Columns.Remove("TypeCode"); dt.Columns.Remove("Attach");
            dt.Columns["SuppPN"].ColumnName = "制造商料号"; dt.Columns["CDesc"].ColumnName = "品名";
            dt.Columns["CSpec"].ColumnName = "规格"; dt.Columns["Brand"].ColumnName = "品牌";
            dt.Columns["TypeName"].ColumnName = "类型"; dt.Columns["SuppAbbr"].ColumnName = "代理商"; dt.Columns["CustPN"].ColumnName = "客户料号";
            dt.Columns["ProxyCount"].ColumnName = "代理商数"; dt.Columns["CustPNCount"].ColumnName = "客户料号数";           

            EPPlusNew myExcel = new EPPlusNew("");

            string filename = "MPN.xlsx";
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            myExcel.ExportExcel(dt, path);
            myExcel = null;
            return "/FJLBS/TempFiles/" + filename;
        }
    }
    public class PartNo:ModelBase
    {
        [PrimaryKey]
        public int PNID { get; set; }
        public string SuppPN { get; set; }
        public string CDesc { get; set; }
        public string CSpec { get; set; }
        public string Brand { get; set; }
        public string TypeCode { get; set; }
        public string HotSale { get; set; }
        public int LeadTime { get; set; }       
    }
}