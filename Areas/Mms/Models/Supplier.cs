using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Zephyr.Core;

namespace ECGroup.Models
{
    [Module("EC")]
    public class SupplierService : ServiceBase<Supplier>
    {
        public static string strSP = "DG_BasicInfo";
        internal IList<Supplier> GetList(ParamQuery pQuery)
        {
            string strSQL = pQuery.GetSQL();
            List<Supplier> myList = db.Select<Supplier>(strSQL).QueryMany();

            return myList;
        }

        public static List<dynamic> getIdeasSupplierCodeList(string q)
        {
            ParamSP ps = new ParamSP().Name("usp_BasisData");
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "getIdeasSupplierCodeList");
            ps.Parameter("data", q);
            List<dynamic> resultA = new SupplierService().GetDynamicList(ps);

            return resultA;
        }

        public static List<dynamic> getSupplierCodeList(string q)
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "getSupplierCodeList");
            ps.Parameter("data", q);
            List<dynamic> resultA = new SupplierService().GetDynamicList(ps);
            if(q==""){resultA.Insert(0, new { value = "", text = "全部" }); }
            return resultA;
        }

        internal static List<dynamic> getSupplierAbbr(string q)
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ps.Parameter("ActionType", "getSupplierAbbr");
            ps.Parameter("SuppAbbr", q);
            List<dynamic> resultA = new SupplierService().GetDynamicList(ps);

            return resultA;
        }

        public static List<dynamic> getSupplierInfoList(ParamSP ps)
        {            
            List<dynamic> resultA = new SupplierService().GetDynamicList(ps);

            return resultA;
        }

        public static List<dynamic> getSuppPNListForCombo(bool showBlank)
        {
            ParamSP ps = new ParamSP().Name(strSP);
            ParamSPData psd = ps.GetData();
            ps.Parameter("ActionType", "getSuppPNListForCombo");
            List<dynamic> resultA = new MaterialTypeService().GetDynamicList(ps);

            if (showBlank)
            { resultA.Insert(0, new { value = "", text = "全部" }); }
            return resultA;
        }

        internal static string ExportExcel(ParamSP ps)
        {
            //SuppID,SuppAbbr,SuppCode,SuppName,SuppAdd,Contact,Tel,CellNo,Email,SWIFICode,AccountName,AccountNo,Currency,PayTerms,BankName 
            DataTable dt = new CompanyService().StoredProcedureDS(ps).Tables[0];

            dt.Columns.Remove("SuppID");
            dt.Columns["SuppCode"].ColumnName = "代理商代码"; dt.Columns["SuppAbbr"].ColumnName = "代理商简称"; dt.Columns["SuppName"].ColumnName = "全称";
            dt.Columns["SuppAdd"].ColumnName = "地址"; dt.Columns["Contact"].ColumnName = "联系人";
            dt.Columns["Tel"].ColumnName = "电话"; dt.Columns["CellNo"].ColumnName = "手机";
            dt.Columns["AccountNo"].ColumnName = "银行账号"; dt.Columns["AccountName"].ColumnName = "账号持有人";
            dt.Columns["Currency"].ColumnName = "结算币别"; dt.Columns["BankName"].ColumnName = "开户行"; dt.Columns["PayTerms"].ColumnName = "结算条件";

            EPPlusNew myExcel = new EPPlusNew("");

            string filename = "Supplier.xlsx";
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            myExcel.ExportExcel(dt, path);
            myExcel = null;
            return "/FJLBS/TempFiles/" + filename;
        }

        internal static string ExportExcelRelate(ParamSP ps)
        {
            DataTable dt = new CompanyService().StoredProcedureDS(ps).Tables[0];
            EPPlusNew myExcel = new EPPlusNew("");

            string filename = "SupplierPNRelate.xlsx";
            string path = Zephyr.Utils.ZHttp.RootPhysicalPath + @"FJLBS\TempFiles\" + filename;

            myExcel.ExportExcel(dt, path);
            myExcel = null;
            return "/FJLBS/TempFiles/" + filename;
        }
    }
    public class Supplier:ModelBase
    {
        [PrimaryKey]
        public int SuppID { get; set; }
        public string SuppAbbr { get; set; }
        public string SuppCode { get; set; }
        public string SuppName { get; set; }
        public string SuppAdd { get; set; }
        public string Contact { get; set; }
        public string Tel { get; set; }
        public string CellNo { get; set; }
        public string Email { get; set; }
        public string SWIFICode { get; set; }
        public string AccountName { get; set; }
        public string AccountNo { get; set; }
        public string Currency { get; set; }
        public string PayTerms { get; set; }
        public string BankName { get; set; } 

        /*SuppID,SuppAbbr,SuppCode,SuppName,SuppAdd,Contact,Tel,CellNo,Email,SWIFICode,AccountName,AccountNo,Currency,PayTerms,BankName*/
    }
}